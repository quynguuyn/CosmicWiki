import { Injectable, inject } from '@angular/core';
import { OrderingApi } from '../api/ordering.api';
import { CreateOrderRequest } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderingService {
  private readonly api = inject(OrderingApi);

  createOrder(request: CreateOrderRequest) {
    return this.api.createOrder(request, crypto.randomUUID());
  }

  getOrder(orderId: string) {
    return this.api.getOrder(orderId);
  }
}
