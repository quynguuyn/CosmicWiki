import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '../../../core/config/api.config';
import { PagedResult } from '../../../core/models/paged-result.model';
import { AdminOrder, CreateOrderRequest, CreateOrderResult, OrderStatus } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderingApi {
  private readonly baseUrl = API_CONFIG.baseUrl;

  constructor(private readonly http: HttpClient) {}

  createOrder(request: CreateOrderRequest, idempotencyKey: string) {
    return this.http.post<CreateOrderResult>(
      `${this.baseUrl}/api/orders`,
      request,
      { headers: { 'Idempotency-Key': idempotencyKey } }
    );
  }

  getOrder(orderId: string) {
    return this.http.get<OrderStatus>(`${this.baseUrl}/api/orders/${orderId}`);
  }

  getOrders() {
    return this.http.get<OrderStatus[]>(`${this.baseUrl}/api/orders`);
  }

  updateStatus(orderId: string, status: string) {
    return this.http.patch<{ orderId: string; status: string }>(
      `${this.baseUrl}/api/orders/${orderId}/status`,
      { status }
    );
  }

  getAdminOrdersToday(page: number, pageSize: number, status?: string) {
    const params: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
    if (status) params['status'] = status;
    return this.http.get<PagedResult<AdminOrder>>(
      `${this.baseUrl}/api/admin/orders/today/page`,
      { params }
    );
  }

  updateAdminStatus(orderId: string, status: string) {
    return this.http.patch<AdminOrder>(
      `${this.baseUrl}/api/admin/orders/${orderId}/status`,
      { status }
    );
  }
}
