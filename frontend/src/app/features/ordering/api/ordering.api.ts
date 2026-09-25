import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '../../../core/config/api.config';
import { CreateOrderRequest, CreateOrderResult, OrderStatus } from '../models/order.model';

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
}
