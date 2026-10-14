export interface OrderLineRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerId: string;
  currency: string;
  lines: OrderLineRequest[];
}

export interface CreateOrderResult {
  orderId: string;
  paymentIntentId: string;
  totalAmount: number;
  currency: string;
  status: string;
  clientSecret?: string | null;
}

export interface OrderStatus {
  orderId: string;
  status: string;
  paymentIntentId: string | null;
  clientSecret: string | null;
  totalAmount: number;
  currency: string;
  paidAtUtc: string | null;
  createdAtUtc: string;
  paymentExpiresAtUtc?: string;
}

export interface AdminOrder extends OrderStatus {
  customerId: string;
  customerEmail: string | null;
}
