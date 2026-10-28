import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user.component.html',
  styleUrl: './user.component.scss'
})
export class UserComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly email = this.auth.getEmail();

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
