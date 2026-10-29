import { DestroyRef, Injectable, effect, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from '../auth/services/auth.service';
import { NotificationsResponse, UserNotification } from './notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private connection: HubConnection | null = null;
  private connecting = false;
  private retryTimer?: ReturnType<typeof setTimeout>;
  readonly items = signal<UserNotification[]>([]);
  readonly unreadCount = signal(0);
  readonly error = signal('');

  constructor() {
    effect(() => {
      if (this.auth.authenticated()) {
        this.connect();
      } else {
        this.disconnect();
        this.items.set([]);
        this.unreadCount.set(0);
        this.error.set('');
      }
    });
    this.destroyRef.onDestroy(() => this.disconnect());
  }

  markRead(notification: UserNotification): void {
    if (notification.isRead) return;
    this.http.patch<UserNotification>(`/api/notifications/${notification.id}/read`, {}).subscribe({
      next: updated => {
        this.items.update(items => items.map(item => item.id === updated.id ? updated : item));
        this.unreadCount.update(count => Math.max(0, count - 1));
      },
      error: () => this.error.set('Không thể cập nhật trạng thái đã đọc. Vui lòng thử lại.')
    });
  }

  markAllRead(): void {
    this.http.post<void>('/api/notifications/read-all', {}).subscribe({
      next: () => {
        this.items.update(items => items.map(item => ({ ...item, isRead: true, readAtUtc: item.readAtUtc ?? new Date().toISOString() })));
        this.unreadCount.set(0);
      },
      error: () => this.error.set('Không thể đánh dấu đã đọc. Vui lòng thử lại.')
    });
  }

  private connect(): void {
    if (this.connection || this.connecting) return;
    this.connecting = true;
    this.error.set('');
    const connection = new HubConnectionBuilder()
      .withUrl('/api/notifications/hub', {
        accessTokenFactory: () => this.auth.getAccessToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    this.connection = connection;
    connection.on('NotificationReceived', (notification: UserNotification) => {
      this.items.update(items => [notification, ...items.filter(item => item.id !== notification.id)].slice(0, 30));
      if (!notification.isRead) this.unreadCount.update(count => count + 1);
    });
    connection.onreconnected(() => this.load());
    connection.onclose(error => {
      if (this.connection !== connection || !this.auth.authenticated()) return;
      this.connection = null;
      this.error.set('Mất kết nối thông báo trực tiếp. Đang thử kết nối lại.');
      if (error) console.error('SignalR notification connection closed', error);
      this.scheduleReconnect();
    });
    this.load();
    connection.start()
      .then(() => {
        this.connecting = false;
      })
      .catch(error => {
        this.connecting = false;
        this.connection = null;
        this.error.set('Không thể kết nối thông báo trực tiếp. Danh sách thông báo vẫn có thể tải lại.');
        console.error('SignalR notification connection failed', error);
        this.scheduleReconnect();
      });
  }

  private scheduleReconnect(): void {
    if (this.retryTimer) clearTimeout(this.retryTimer);
    this.retryTimer = setTimeout(() => {
      this.retryTimer = undefined;
      if (this.auth.authenticated()) this.connect();
    }, 10000);
  }

  private load(): void {
    this.http.get<NotificationsResponse>('/api/notifications').subscribe({
      next: response => {
        this.items.set(response.items);
        this.unreadCount.set(response.unreadCount);
        this.error.set('');
      },
      error: () => this.error.set('Không thể tải thông báo. Vui lòng thử lại sau.')
    });
  }

  private disconnect(): void {
    if (this.retryTimer) clearTimeout(this.retryTimer);
    this.retryTimer = undefined;
    const connection = this.connection;
    this.connection = null;
    this.connecting = false;
    if (connection?.state === 'Connected' || connection?.state === 'Connecting' || connection?.state === 'Reconnecting') {
      void connection.stop().catch(error => console.error('SignalR notification disconnect failed', error));
    }
  }
}
