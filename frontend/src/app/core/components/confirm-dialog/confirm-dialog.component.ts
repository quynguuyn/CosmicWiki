import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    <div class="confirm-backdrop" role="presentation" (click)="cancel.emit()">
      <section class="confirm-dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" (click)="$event.stopPropagation()">
        <h2 id="confirm-title">{{ title }}</h2>
        <p>{{ message }}</p>
        <div class="confirm-actions">
          <button type="button" (click)="cancel.emit()">Hủy</button>
          <button type="button" class="confirm-danger" (click)="confirmed.emit()">Xóa</button>
        </div>
      </section>
    </div>`,
  styles: [`
    .confirm-backdrop { position: fixed; inset: 0; z-index: 1000; display: grid; place-items: center; padding: 1rem; background: rgba(0,0,0,.62); }
    .confirm-dialog { width: min(100%, 28rem); padding: 1.5rem; border: 1px solid rgba(184,212,232,.3); border-radius: 1rem; background: #132238; color: var(--on-surface); box-shadow: 0 1.5rem 4rem rgba(0,0,0,.45); }
    h2 { margin: 0 0 .7rem; font-size: 1.2rem; } p { margin: 0; color: var(--on-surface-variant); } .confirm-actions { display:flex; justify-content:flex-end; gap:.75rem; margin-top:1.5rem; }
    button { padding:.65rem 1rem; border:1px solid var(--glass-border); border-radius:.6rem; background:transparent; color:var(--on-surface); cursor:pointer; } .confirm-danger { border-color:#d97070; background:#b64141; color:#fff; }
  `]
})
export class ConfirmDialogComponent {
  @Input() title = 'Xác nhận xóa';
  @Input({ required: true }) message = '';
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
