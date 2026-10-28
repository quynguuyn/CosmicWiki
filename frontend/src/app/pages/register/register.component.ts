import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);

  username = '';
  email = '';
  password = '';
  isSubmitting = false;
  registered = false;
  errorMessage = '';

  submit(): void {
    if (this.isSubmitting) return;

    this.isSubmitting = true;
    this.errorMessage = '';
    this.auth.register({
      username: this.username.trim(),
      email: this.email.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.registered = true;
        this.password = '';
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.status === 409
          ? 'Tên tài khoản hoặc email đã được sử dụng.'
          : 'Không thể tạo tài khoản. Vui lòng kiểm tra thông tin và thử lại.';
        this.isSubmitting = false;
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }
}
