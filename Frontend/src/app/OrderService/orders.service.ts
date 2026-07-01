import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {

  constructor(private http: HttpClient) {}

  getOrders() {
    return this.http.get<any[]>(
      '/api/orders'
    );
  }

  checkout(payload: { deliveryAddress: string }) {
    return this.http.post<any>(
      '/api/orders',
      payload
    );
  }
}