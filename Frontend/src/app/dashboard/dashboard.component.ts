import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { CatalogService } from '../CatalogService/catalog.service';
import { OrdersService } from '../OrderService/orders.service';



interface Product { id: string; name: string; price: number; }
interface Order { id: string; orderNumber: string; deliveryAddress: string; status: string; }



@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {

  products: Product[] = [];
  orders: Order[] = [];
  cart: Product [] = [];
  deliveryAddress: string = '';

  loadingProducts = false;
  loadingOrders = false;
  productLoadError = false;
  orderLoadError = false;

  constructor(
    private catalogService: CatalogService,
    private ordersService: OrdersService
  ) {}

  ngOnInit() {
    this.loadProducts();
    this.loadOrders();
  }

  loadProducts() {
    this.loadingProducts = true;
    this.productLoadError = false;

    this.catalogService.getProducts()
      .pipe(finalize(() => this.loadingProducts = false))
      .subscribe({
        next: (data) => {
          this.products = data;
        },
        error: (err) => {
          console.error(err);
          this.productLoadError = true;
        }
      });
  }

  loadOrders() {
    this.loadingOrders = true;
    this.orderLoadError = false;

    this.ordersService.getOrders()
      .pipe(finalize(() => this.loadingOrders = false))
      .subscribe({
        next: (data) => {
          this.orders = data;
        },
        error: (err) => {
          console.error(err);
          this.orderLoadError = true;
        }
      });
  }

  addToCart(product: any) {
    this.cart.push(product);
  }

  getCartTotal() {
    return this.cart.reduce((sum, item) => sum + item.price, 0);
  }

  checkout() {
    if (!this.deliveryAddress.trim() || this.cart.length === 0) return;

    const payload = {
      deliveryAddress: this.deliveryAddress,
      totalAmount: this.getCartTotal()
    };

    this.catalogService.checkout(payload).subscribe(() => {
      this.cart = [];
      this.deliveryAddress = '';
      setTimeout(() => this.loadOrders(), 1200);
    });
  }
}
