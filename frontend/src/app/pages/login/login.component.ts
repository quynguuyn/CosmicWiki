import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../features/auth/services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterLink
    ],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent {

    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    username = '';
    password = '';

    rememberMe = false;
    showPassword = false;
    isLoading = false;
    errorMessage = '';

    submit(): void {
        if (!this.username || !this.password) {
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        const request = {
            username: this.username,
            password: this.password
        };

        this.authService.login(request).subscribe({
            next: response => {
                this.authService.saveSession(response);

                this.isLoading = false;

                this.router.navigate([
                    this.authService.hasPermission('shipping.write') ? '/admin' : '/'
                ]);
            },

            error: error => {
                this.isLoading = false;

                this.errorMessage =
                    'Invalid username or password.';
            }
        });
    }

    togglePassword(): void {
        this.showPassword = !this.showPassword;
    }
}