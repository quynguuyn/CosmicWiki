import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="status-page">
      <section class="status-card cosmic-glass">
        <span class="material-symbols-outlined status-icon">explore_off</span>
        <p class="eyebrow">COSMICWIKI · DEEP SPACE</p>
        <h1>Không tìm thấy trang</h1>
        <p>Tọa độ bạn truy cập không tồn tại trong bảo tàng.</p>
        <a routerLink="/" class="status-action">Về trang chủ</a>
      </section>
    </main>
  `,
  styleUrl: '../forbidden/status-page.scss'
})
export class NotFoundComponent {}
