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
  deliveryPersonName?: string | null;
  deliveryLatitude?: number | null;
  deliveryLongitude?: number | null;
  deliveryLocationUpdatedAtUtc?: string | null;
  statusHistory?: OrderStatusHistoryEntry[];
  lines?: OrderLineStatus[];
}

export interface OrderLineStatus {
  productId: string;
  sku: string;
  productName: string;
  unitPrice: number;
  productImageUrl: string | null;
  quantity: number;
  lineTotal: number;
}

export interface OrderStatusHistoryEntry {
  status: string;
  changedAtUtc: string;
}

export interface DeliveryOrder {
  orderId: string;
  status: string;
  availableToClaim: boolean;
  totalAmount: number;
  currency: string;
  createdAtUtc: string;
  deliveryPersonName: string | null;
  deliveryLatitude: number | null;
  deliveryLongitude: number | null;
  deliveryLocationUpdatedAtUtc: string | null;
  lines?: OrderLineStatus[];
}

export interface AdminOrder extends OrderStatus {
  customerId: string;
  customerEmail: string | null;
}
