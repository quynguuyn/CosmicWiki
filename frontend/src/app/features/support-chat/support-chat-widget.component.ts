import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AiChatMessage, SupportChatConversation, SupportChatMessage, SupportChatOpenResponse } from './support-chat.models';
import { AuthService } from '../auth/services/auth.service';

const API = '/api/stories/support-chat';
const AI_API = '/api/stories/ai-chat/messages';
const GUEST_TOKEN_KEY = 'cosmicwiki_support_chat_guest_token';
const MAX_QUEUED_AI_MESSAGES = 5;

@Component({
  selector: 'app-support-chat-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './support-chat-widget.component.html',
  styleUrl: './support-chat-widget.component.scss'
})
export class SupportChatWidgetComponent implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  @ViewChild('chatMessages') private chatMessages?: ElementRef<HTMLDivElement>;
  private pollTimer?: ReturnType<typeof setInterval>;
  private readonly aiRequestQueue: Array<{ id: string; content: string }> = [];
  private aiConversationHistory: Array<{ role: 'user' | 'assistant'; content: string }> = [];
  private aiRequestActive = false;
  readonly open = signal(false);
  readonly loading = signal(false);
  readonly sending = signal(false);
  readonly supportSending = signal(false);
  readonly error = signal('');
  readonly conversation = signal<SupportChatConversation | null>(null);
  readonly messages = signal<SupportChatMessage[]>([]);
  readonly aiMessages = signal<AiChatMessage[]>([]);
  readonly mode = signal<'ai' | 'support'>('ai');
  draft = '';

  get queuedAiMessageCount(): number {
    return this.aiRequestQueue.length;
  }

  get maxQueuedAiMessages(): number {
    return MAX_QUEUED_AI_MESSAGES;
  }

  toggle(): void {
    if (this.open()) {
      this.open.set(false);
      this.stopPolling();
      return;
    }

    this.open.set(true);
    if (this.mode() === 'support') {
      this.ensureConversation();
    }
  }

  send(): void {
    if (this.mode() === 'ai') {
      this.sendAiMessage();
      return;
    }

    const conversation = this.conversation();
    const body = this.draft.trim();
    if (!conversation || !body || this.supportSending()) return;

    this.supportSending.set(true);
    this.error.set('');
    this.http.post<SupportChatMessage>(
      `${API}/conversations/${conversation.id}/messages`,
      { body },
      { headers: this.guestHeaders() }
    ).subscribe({
      next: message => {
        this.draft = '';
        this.messages.update(items => [...items, message]);
        this.supportSending.set(false);
        this.scrollChatToBottom();
      },
      error: () => {
        this.supportSending.set(false);
        this.error.set('Không gửi được tin nhắn. Hãy thử lại.');
      }
    });
  }

  onComposerEnter(event: Event): void {
    if (!(event instanceof KeyboardEvent) || event.isComposing || event.ctrlKey) return;
    event.preventDefault();
    this.send();
  }

  selectMode(mode: 'ai' | 'support'): void {
    this.mode.set(mode);
    this.error.set('');
    if (mode === 'support' && !this.conversation()) {
      this.ensureConversation();
    }
  }

  closeConversation(): void {
    const conversation = this.conversation();
    if (!conversation) return;
    this.http.post<void>(
      `${API}/conversations/${conversation.id}/close`,
      {},
      { headers: this.guestHeaders() }
    ).subscribe({
      next: () => {
        localStorage.removeItem(GUEST_TOKEN_KEY);
        this.conversation.set(null);
        this.messages.set([]);
        this.stopPolling();
        this.open.set(false);
      },
      error: () => this.error.set('Không thể kết thúc cuộc trò chuyện.')
    });
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  private ensureConversation(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.post<SupportChatOpenResponse>(
      `${API}/conversations`,
      {},
      { headers: this.guestHeaders() }
    ).subscribe({
      next: result => {
        if (result.guestAccessToken) {
          localStorage.setItem(GUEST_TOKEN_KEY, result.guestAccessToken);
        } else if (this.auth.authenticated()) {
          localStorage.removeItem(GUEST_TOKEN_KEY);
        }
        this.conversation.set(result.conversation);
        this.messages.set(result.conversation.messages ?? []);
        this.loading.set(false);
        this.startPolling();
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Không thể mở chat hỗ trợ. Vui lòng thử lại.');
      }
    });
  }

  private sendAiMessage(): void {
    const body = this.draft.trim();
    if (!body || this.aiRequestQueue.length >= MAX_QUEUED_AI_MESSAGES) return;

    const id = crypto.randomUUID();
    this.aiMessages.update(items => [
      ...items,
      { id, role: 'user', content: body, createdAt: new Date(), status: 'queued' }
    ]);
    this.scrollChatToBottom();
    this.aiRequestQueue.push({ id, content: body });
    this.draft = '';
    this.error.set('');
    this.processNextAiMessage();
  }

  private processNextAiMessage(): void {
    if (this.aiRequestActive || this.aiRequestQueue.length === 0) return;

    const next = this.aiRequestQueue.shift()!;
    const history = [
      ...this.aiConversationHistory,
      { role: 'user' as const, content: next.content }
    ].slice(-12);

    this.aiRequestActive = true;
    this.sending.set(true);
    this.updateAiMessage(next.id, { status: 'thinking' });
    this.http.post<{ reply: string }>(AI_API, { messages: history }).subscribe({
      next: result => {
        this.aiConversationHistory = [
          ...history,
          { role: 'assistant' as const, content: result.reply }
        ].slice(-12);
        this.updateAiMessage(next.id, { status: 'sent' });
        this.aiMessages.update(items => [
          ...items,
          {
            id: crypto.randomUUID(),
            role: 'assistant',
            content: result.reply,
            createdAt: new Date(),
            status: 'sent'
          }
        ]);
        this.scrollChatToBottom();
        this.finishAiRequest();
      },
      error: error => {
        this.updateAiMessage(next.id, { status: 'failed' });
        this.error.set(
          error?.error?.message ??
          'Trợ lý AI đang tạm thời không khả dụng. Vui lòng thử lại sau.'
        );
        this.finishAiRequest();
      }
    });
  }

  private updateAiMessage(id: string, update: Partial<AiChatMessage>): void {
    this.aiMessages.update(items =>
      items.map(message => message.id === id ? { ...message, ...update } : message)
    );
  }

  private finishAiRequest(): void {
    this.aiRequestActive = false;
    this.sending.set(false);
    this.processNextAiMessage();
  }

  private scrollChatToBottom(): void {
    requestAnimationFrame(() => {
      const element = this.chatMessages?.nativeElement;
      if (element) element.scrollTop = element.scrollHeight;
    });
  }

  hasSupportReply(): boolean {
    return this.messages().some(message => message.senderType === 'Support');
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollTimer = setInterval(() => this.loadMessages(), 4000);
  }

  private loadMessages(): void {
    const conversation = this.conversation();
    if (!conversation || !this.open()) return;
    this.http.get<SupportChatConversation>(
      `${API}/conversations/${conversation.id}`,
      { headers: this.guestHeaders() }
    ).subscribe({
      next: result => {
        this.conversation.set(result);
        this.messages.set(result.messages ?? []);
      },
      error: () => this.error.set('Mất kết nối chat. Đang thử tải lại...')
    });
  }

  private guestHeaders(): HttpHeaders {
    const token = localStorage.getItem(GUEST_TOKEN_KEY);
    return token ? new HttpHeaders({ 'X-Guest-Chat-Token': token }) : new HttpHeaders();
  }

  private stopPolling(): void {
    if (this.pollTimer) clearInterval(this.pollTimer);
    this.pollTimer = undefined;
  }
}
