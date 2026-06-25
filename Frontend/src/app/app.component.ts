import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface Order {
  id: string;
  orderNumber: string;
  deliveryAddress: string;
  status: string;
}
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule], // <-- MUST have both of these here!
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  apiUrl = 'http://localhost:5257/api/orders';
  orders: Order[] = [];
  newAddress: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.http.get<Order[]>(this.apiUrl).subscribe(data => this.orders = data);
  }

  submitOrder() {
    if (!this.newAddress.trim()) return;

    this.http.post(this.apiUrl, { deliveryAddress: this.newAddress })
      .subscribe(() => {
        this.newAddress = '';
        this.loadOrders(); // Refresh the list
      });
  }
}