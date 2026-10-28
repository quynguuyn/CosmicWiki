import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.scss'
})
export class PaginationComponent implements OnChanges {
  private static nextInputId = 0;

  @Input() page = 1;
  @Input() totalPages = 1;
  @Input() totalCount?: number;
  @Input() itemLabel = '';
  @Input() ariaLabel = 'Phân trang';
  @Output() readonly pageChange = new EventEmitter<number>();

  readonly inputId = `pagination-page-${PaginationComponent.nextInputId++}`;
  targetPage = 1;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['page']) this.targetPage = this.page;
  }

  selectPage(page: number): void {
    if (!Number.isInteger(page) || page < 1 || page > this.totalPages || page === this.page) return;
    this.targetPage = page;
    this.pageChange.emit(page);
  }

  jumpToPage(): void {
    const page = Number(this.targetPage);
    if (!Number.isInteger(page) || page < 1 || page > this.totalPages) {
      this.targetPage = this.page;
      return;
    }
    this.selectPage(page);
  }
}
