import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface Product { id: string; name: string; price: number; }
interface Order { id: string; orderNumber: string; deliveryAddress: string; status: string; }

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html'
})

export class AppComponent implements OnInit {
  catalogUrl = 'http://localhost:5200/api/products';
  checkoutUrl = 'http://localhost:5200/api/cart/checkout';
  ordersUrl = 'http://localhost:5257/api/orders';

  products: Product[] = [];
  cart: Product[] = [];
  orders: Order[] = [];
  deliveryAddress: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadProducts();
    this.loadOrders();
  }

  loadProducts() {
    this.http.get<Product[]>(this.catalogUrl).subscribe(data => this.products = data);
  }

  loadOrders() {
    this.http.get<Order[]>(this.ordersUrl).subscribe(data => this.orders = data);
  }

  addToCart(product: Product) {
    this.cart.push(product);
  }

  getCartTotal() {
    return this.cart.reduce((sum, item) => sum + item.price, 0);
  }

  checkout() {
    if (!this.deliveryAddress.trim() || this.cart.length === 0) return;

    const payload = { deliveryAddress: this.deliveryAddress, totalAmount: this.getCartTotal() };
    
    this.http.post(this.checkoutUrl, payload).subscribe(() => {
      this.cart = [];
      this.deliveryAddress = '';
      setTimeout(() => this.loadOrders(), 1000); // Wait for the queue processing to complete
    });
  }
}