import { Component } from '@angular/core';
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
  template: `
    <app-header *ngIf="!adminShell()"></app-header>
    <main class="pt-20">
      <router-outlet></router-outlet>
    </main>
    <app-footer *ngIf="!adminShell()"></app-footer>
  `,
  styles: [`
    main {
      min-height: calc(100vh - 5rem);
    }
  `]
})
export class AppComponent {
  private readonly router = inject(Router);
  readonly adminShell = signal(false);
  title = 'CosmicWiki';

  constructor() {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(event => {
      this.adminShell.set((event as NavigationEnd).urlAfterRedirects.startsWith('/admin'));
    });
  }
}
