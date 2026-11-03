export interface SupportChatMessage {
  id: string;
  senderType: 'Customer' | 'Support';
  senderUserId?: string | null;
  senderDisplayName?: string | null;
  body: string;
  createdAtUtc: string;
}

export interface SupportChatConversation {
  id: string;
  status: 'Queued' | 'Open' | 'Closed';
  assignedSupportId?: string | null;
  customerDisplayName?: string | null;
  assignedSupportDisplayName?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  lastMessageAtUtc?: string | null;
  messages: SupportChatMessage[];
}

export interface SupportChatOpenResponse {
  conversation: SupportChatConversation;
  guestAccessToken?: string | null;
}

export interface SupportChatQueueItem {
  id: string;
  status: 'Queued' | 'Open' | 'Closed';
  assignedSupportId?: string | null;
  customerDisplayName?: string | null;
  createdAtUtc: string;
  lastMessageAtUtc?: string | null;
  lastMessage?: string | null;
  lastSenderType?: string | null;
}

export interface AiChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: Date;
  status?: 'queued' | 'thinking' | 'sent' | 'failed';
}
