import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

interface ObservatoryEvent {
  date: string;
  name: string;
  description: string;
  specification: string;
  location: string;
  status: string;
  statusType: 'primary' | 'tertiary' | 'secondary' | 'outline';
  action: string;
}

interface Exhibition {
  number: string;
  category: string;
  title: string;
  description: string;
  status: string;
  image: string;
}

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-visit',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './visit.component.html',
  styleUrl: './visit.component.scss'
})
export class VisitComponent implements OnInit {

  constructor(private readonly api: ApiService) {}

  contentError = '';

  ngOnInit(): void {
    this.exhibitions = [];
    this.events = [];
    this.faqs = [];
    this.api.get<{ items: Array<{
      number: string;
      category: string;
      title: string;
      description: string;
      status: string;
      imageUrl: string | null;
    }> }>('/api/visit/exhibitions').subscribe({
      next: result => {
        if (result.items.length > 0) {
          this.exhibitions = result.items.map(item => ({
            number: item.number,
            category: item.category,
            title: item.title,
            description: item.description,
            status: item.status,
            image: item.imageUrl ?? 'assets/images/cosmic-placeholder.svg'
          }));
        } else {
          this.exhibitions = [];
        }
      },
      error: () => this.contentError = 'Không thể tải dữ liệu triển lãm.'
    });

    this.api.get<{ items: Array<{
      eventDateUtc: string;
      name: string;
      description: string;
      specification: string | null;
      location: string | null;
      status: string;
      statusType: ObservatoryEvent['statusType'] | null;
      action: string | null;
    }> }>('/api/visit/events').subscribe({
      next: result => {
        if (result.items.length > 0) {
          this.events = result.items.map(item => ({
            date: new Intl.DateTimeFormat('vi-VN', { dateStyle: 'long' })
              .format(new Date(item.eventDateUtc)),
            name: item.name,
            description: item.description,
            specification: item.specification ?? '',
            location: item.location ?? '',
            status: item.status,
            statusType: item.statusType ?? 'outline',
            action: item.action ?? 'Xem chi tiết'
          }));
        } else {
          this.events = [];
        }
      },
      error: () => this.contentError = 'Không thể tải dữ liệu sự kiện.'
    });

    this.api.get<{ items: FaqItem[] }>('/api/visit/faqs').subscribe({
      next: result => {
        if (result.items.length > 0) {
          this.faqs = result.items;
        } else {
          this.faqs = [];
        }
      },
      error: () => this.contentError = 'Không thể tải câu hỏi thường gặp.'
    });
  }

  currentYear = new Date().getFullYear();

  isSubmitting = false;
  showConfirmationModal = false;
  submissionError = '';
  referenceCode = '';

  form = {
    name: '',
    email: '',
    subject: 'booking',
    message: ''
  };

    exhibitions: Exhibition[] = [];

    events: ObservatoryEvent[] = [];

    faqs: FaqItem[] = [];

  submitTransmission(): void {
    if (this.isSubmitting || !this.form.name.trim() || !this.form.email.trim() || !this.form.message.trim()) {
      return;
    }

    this.isSubmitting = true;
    this.submissionError = '';

    this.api.post<
      {
        name: string;
        email: string;
        requestedDateUtc: string;
        partySize: number;
        notes: string;
      },
      { referenceCode?: string }
    >('/api/visit/requests', {
      name: this.form.name,
      email: this.form.email,
      requestedDateUtc: new Date().toISOString(),
      partySize: 1,
      notes: `[${this.form.subject}] ${this.form.message}`
    }).subscribe({
      next: response => {
        this.referenceCode = response.referenceCode ?? '';
        this.showConfirmationModal = true;
        this.form = {
          name: '',
          email: '',
          subject: 'booking',
          message: ''
        };
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.submissionError = this.getSubmissionError(error);
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }

  private getSubmissionError(error: HttpErrorResponse): string {
    if (error.status === 400) {
      const validationErrors = error.error?.errors;
      if (validationErrors) {
        const messages = Object.values(validationErrors).flatMap(value =>
          Array.isArray(value) ? value : [String(value)]
        );
        if (messages.length > 0) {
          return messages.join(' ');
        }
      }
      return 'Vui lòng kiểm tra lại thông tin đăng ký và thử lại.';
    }

    if (error.status === 0) {
      return 'Không thể kết nối với hệ thống đặt lịch. Vui lòng thử lại sau.';
    }

    return 'Tín hiệu chưa được ghi nhận. Vui lòng thử lại sau.';
  }

  closeModal(): void {
    this.showConfirmationModal = false;
  }

  scrollToBooking(): void {
    document
      .getElementById('booking-transmission')
      ?.scrollIntoView({ behavior: 'smooth' });
  }
}