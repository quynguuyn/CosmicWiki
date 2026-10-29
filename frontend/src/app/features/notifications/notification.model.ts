export interface UserNotification {
  id: string;
  orderId: string;
  title: string;
  message: string;
  status: string;
  createdAtUtc: string;
  readAtUtc: string | null;
  isRead: boolean;
}

export interface NotificationsResponse {
  items: UserNotification[];
  unreadCount: number;
}
