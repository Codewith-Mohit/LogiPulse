import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div style="max-width: 320px; margin: 80px auto; font-family: sans-serif;">
      <h2>Login</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div style="margin-bottom: 12px;">
          <input formControlName="email" placeholder="Email" style="width: 100%; padding: 8px;" />
        </div>
        <div style="margin-bottom: 12px;">
          <input formControlName="password" type="password" placeholder="Password" style="width: 100%; padding: 8px;" />
        </div>
        <button type="submit" style="width: 100%; padding: 8px;">Login</button>
      </form>
    </div>
  `
})
export class LoginComponent {
  form: FormGroup;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    const { email, password } = this.form.value;
    this.authService.login(email, password).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: () => alert('Invalid login')
    });
  }
}
