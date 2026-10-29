import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { signal, inject } from '@angular/core';
import { HeaderComponent } from './layout/header/header.component';
import { FooterComponent } from './layout/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private readonly router = inject(Router);
  readonly adminShell = signal(false);
  online = navigator.onLine;
  title = 'CosmicWiki';

  constructor() {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(event => {
      this.adminShell.set((event as NavigationEnd).urlAfterRedirects.startsWith('/admin'));
    });
  }

  @HostListener('window:online')
  handleOnline(): void {
    this.online = true;
  }

  @HostListener('window:offline')
  handleOffline(): void {
    this.online = false;
  }
}
