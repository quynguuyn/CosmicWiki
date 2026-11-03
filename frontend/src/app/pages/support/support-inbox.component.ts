import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SupportChatConversation, SupportChatMessage, SupportChatQueueItem } from '../../features/support-chat/support-chat.models';

const API = '/api/stories/support-chat';

@Component({
  selector: 'app-support-inbox',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './support-inbox.component.html',
  styleUrl: './support-inbox.component.scss'
})
export class SupportInboxComponent implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private pollTimer?: ReturnType<typeof setInterval>;
  readonly queue = signal<SupportChatQueueItem[]>([]);
  readonly active = signal<SupportChatConversation | null>(null);
  readonly messages = signal<SupportChatMessage[]>([]);
  readonly selectedId = signal('');
  readonly acceptingChats = signal(true);
  readonly error = signal('');
  readonly loading = signal(false);
  draft = '';

  ngOnInit(): void {
    this.setPresence(true);
    this.refresh();
    this.pollTimer = setInterval(() => this.refresh(), 4000);
  }

  ngOnDestroy(): void {
    if (this.pollTimer) clearInterval(this.pollTimer);
    this.setPresence(false);
  }

  toggleAvailability(): void {
    this.acceptingChats.update(value => !value);
    this.setPresence(this.acceptingChats());
  }

  hasAgentReplied(): boolean {
    return this.messages().some(message => message.senderType === 'Support');
  }

  openConversation(item: SupportChatQueueItem): void {
    this.error.set('');
    if (item.status !== 'Queued') {
      this.loadConversation(item.id);
      return;
    }

    this.loading.set(true);
    this.http.post<void>(`${API}/agent/conversations/${item.id}/claim`, {}).subscribe({
      next: () => this.loadConversation(item.id),
      error: () => {
        this.loading.set(false);
        this.error.set('Cuộc trò chuyện vừa được nhân viên khác nhận. Đang làm mới hàng đợi.');
        this.refresh();
      }
    });
  }

  send(): void {
    const conversation = this.active();
    const body = this.draft.trim();
    if (!conversation || !body) return;
    this.http.post<SupportChatMessage>(
      `${API}/agent/conversations/${conversation.id}/messages`, { body }
    ).subscribe({
      next: message => {
        this.draft = '';
        this.messages.update(items => [...items, message]);
        this.refreshQueue();
      },
      error: () => this.error.set('Không thể gửi tin nhắn.')
    });
  }

  closeConversation(): void {
    const conversation = this.active();
    if (!conversation) return;
    this.http.post<void>(`${API}/agent/conversations/${conversation.id}/close`, {}).subscribe({
      next: () => {
        this.active.set(null);
        this.messages.set([]);
        this.selectedId.set('');
        this.refresh();
      },
      error: () => this.error.set('Không thể kết thúc cuộc trò chuyện.')
    });
  }

  private refresh(): void {
    this.refreshQueue();
    this.setPresence(this.acceptingChats());
    const id = this.selectedId();
    if (id) this.loadConversation(id, false);
  }

  private refreshQueue(): void {
    this.http.get<SupportChatQueueItem[]>(`${API}/agent/conversations`).subscribe({
      next: items => this.queue.set(items),
      error: () => this.error.set('Không thể tải hàng đợi hỗ trợ.')
    });
  }

  private loadConversation(id: string, showLoading = true): void {
    if (showLoading) this.loading.set(true);
    this.http.get<SupportChatConversation>(`${API}/agent/conversations/${id}`).subscribe({
      next: conversation => {
        this.active.set(conversation);
        this.selectedId.set(id);
        this.messages.set(conversation.messages ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Không thể tải cuộc trò chuyện này.');
        this.refreshQueue();
      }
    });
  }

  private setPresence(acceptingChats: boolean): void {
    this.http.post<void>(`${API}/agent/presence`, { acceptingChats }).subscribe({
      error: () => this.error.set('Không thể cập nhật trạng thái nhận chat.')
    });
  }
}
