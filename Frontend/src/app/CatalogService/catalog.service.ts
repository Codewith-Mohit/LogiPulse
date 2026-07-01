import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {

  constructor(private http: HttpClient) {}

  getProducts() {
    return this.http.get<any[]>(
      '/api/products'
    );
  }

  checkout(payload: { deliveryAddress: string; totalAmount: number }) {
    return this.http.post<void>(
      '/api/cart/checkout',
      payload
    );
  }
}
