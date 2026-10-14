import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '../../core/config/api.config';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <main class="admin-page">
      <section class="admin-hero">
        <a routerLink="/" class="back-home">← Về trang chủ</a>
        <p class="eyebrow">COSMICWIKI · COMMAND CENTER</p>
        <h1>Admin Dashboard</h1>
        <p>Điều phối các hoạt động của bảo tàng kỹ thuật số từ một trung tâm duy nhất.</p>
      </section>
      <section class="admin-metrics cosmic-glass">
        <div><span>Doanh thu hôm nay</span><strong>{{ revenue | currency:'USD' }}</strong></div>
        <div><span>Đơn hôm nay</span><strong>{{ ordersCount }}</strong></div>
        <div><span>Đã thanh toán</span><strong>{{ paidCount }}</strong></div>
      </section>
      <section class="admin-chart cosmic-glass">
        <h2>Đơn hàng hôm nay</h2>
        <div class="chart-row" *ngFor="let item of statusChart">
          <span>{{ item.label }}</span>
          <div class="bar-track"><i [style.width.%]="item.percent"></i></div>
          <b>{{ item.count }}</b>
        </div>
      </section>
      <section class="admin-jobs cosmic-glass">
        <h2>Jobs hệ thống</h2>
        <p><span class="job-dot"></span> Tự hủy đơn chưa thanh toán: mỗi phút, quá 15 phút</p>
        <p><span class="job-dot"></span> Tự xác nhận đơn đã thanh toán: theo giờ cutoff cấu hình backend</p>
        <p class="muted">Index tìm kiếm sản phẩm và star được cập nhật qua Kafka.</p>
        <div class="hangfire-job">
          <div class="hangfire-job__heading">
            <div>
              <span class="job-status" [class.job-status--off]="!researchJob.enabled"></span>
              <strong>{{ researchJob.name }}</strong>
            </div>
            <span class="job-badge">{{ researchJob.enabled ? 'Đang bật' : 'Đang tắt' }}</span>
          </div>
          <div class="hangfire-job__details">
            <span>Nguồn: {{ researchJob.source }}</span>
            <span>Lịch: {{ researchJob.schedule }}</span>
            <span>Tối đa mỗi lần: {{ researchJob.maxRecordsPerRun }} bản ghi</span>
          </div>
          <div class="hangfire-job__actions">
            <span class="muted">{{ researchJobMessage }}</span>
            <button type="button" class="run-job-button" [disabled]="researchJobRunning || !researchJob.enabled" (click)="runResearchJob()">
              {{ researchJobRunning ? 'Đang xếp hàng...' : 'Chạy ngay' }}
            </button>
          </div>
        </div>
      </section>
      <section class="admin-jobs cosmic-glass">
        <div class="hangfire-job__heading">
          <h2>Hangfire Management</h2>
          <button type="button" class="run-job-button" (click)="loadHangfireOverview()">Làm mới</button>
        </div>
        <div class="hangfire-tabs">
          <button type="button" [class.active]="hangfireTab === 'jobs'" (click)="setHangfireTab('jobs')">Jobs</button>
          <button type="button" [class.active]="hangfireTab === 'retries'" (click)="setHangfireTab('retries')">Retries</button>
          <button type="button" [class.active]="hangfireTab === 'recurring'" (click)="setHangfireTab('recurring')">Recurring</button>
          <button type="button" [class.active]="hangfireTab === 'servers'" (click)="setHangfireTab('servers')">Servers</button>
        </div>
        <div class="hangfire-stats" *ngIf="hangfireTab === 'jobs' || hangfireTab === 'retries'">
          <div *ngFor="let item of hangfireStats">{{ item.label }} <strong>{{ item.value }}</strong></div>
        </div>
        <div class="hangfire-job-list" *ngIf="hangfireTab === 'jobs' || hangfireTab === 'retries'">
          <div class="hangfire-list-heading"><strong>{{ hangfireTab === 'retries' ? 'Failed jobs / retries' : 'Recent succeeded jobs' }}</strong><button type="button" (click)="loadHangfireJobs(hangfireTab === 'retries' ? 'failed' : 'succeeded')">Làm mới</button></div>
          <p class="muted" *ngIf="hangfireJobsError">{{ hangfireJobsError }}</p>
          <p class="muted" *ngIf="hangfireJobsLoading">Đang tải Hangfire jobs...</p>
          <div class="hangfire-list-row" *ngFor="let job of hangfireJobs">
            <code>{{ job.id }}</code><span>{{ job.job?.method?.name || 'Background job' }}</span>
            <button type="button" *ngIf="hangfireTab === 'retries'" (click)="requeueHangfireJob(job.id)">Requeue</button>
            <button type="button" (click)="deleteHangfireJob(job.id)">Xóa</button>
          </div>
          <p class="muted" *ngIf="!hangfireJobsLoading && !hangfireJobsError && hangfireJobs.length === 0">Không có job trong trạng thái này.</p>
          <nav class="hangfire-pagination" aria-label="Phân trang Hangfire jobs" *ngIf="!hangfireJobsLoading && totalHangfirePages > 1">
            <button type="button" [disabled]="hangfirePage <= 1" (click)="goToHangfirePage(hangfirePage - 1)">Trước</button>
            <span>Trang {{ hangfirePage }} / {{ totalHangfirePages }} · {{ hangfireTotalCount }} jobs</span>
            <button type="button" [disabled]="hangfirePage >= totalHangfirePages" (click)="goToHangfirePage(hangfirePage + 1)">Sau</button>
          </nav>
        </div>
        <div class="hangfire-recurring" *ngIf="hangfireTab === 'recurring'">
          <p class="muted" *ngIf="recurringJobsLoading">Đang tải recurring jobs...</p>
          <p class="muted" *ngIf="recurringJobsError">{{ recurringJobsError }}</p>
          <div class="hangfire-list-row" *ngFor="let job of recurringJobs">
            <strong>{{ job.id }}</strong><code>{{ job.cron }}</code><span>{{ job.nextExecution | date:'short' }}</span>
            <button type="button" (click)="triggerRecurringJob(job.id)">Chạy ngay</button>
          </div>
          <p class="muted" *ngIf="!recurringJobsLoading && !recurringJobsError && recurringJobs.length === 0">Không có recurring job.</p>
          <nav class="hangfire-pagination" aria-label="Phân trang recurring jobs" *ngIf="!recurringJobsLoading && recurringTotalPages > 1">
            <button type="button" [disabled]="recurringPage <= 1" (click)="goToRecurringPage(recurringPage - 1)">Trước</button>
            <span>Trang {{ recurringPage }} / {{ recurringTotalPages }} · {{ recurringTotalCount }} jobs</span>
            <button type="button" [disabled]="recurringPage >= recurringTotalPages" (click)="goToRecurringPage(recurringPage + 1)">Sau</button>
          </nav>
        </div>
        <div class="hangfire-servers" *ngIf="hangfireTab === 'servers'">
          <div class="hangfire-server" *ngFor="let server of hangfireServers">
            <strong>{{ server.name }}</strong>
            <span>{{ server.workers }} workers</span>
            <span>{{ server.queues.join(', ') }}</span>
            <span>Heartbeat: {{ server.heartbeatUtc | date:'short' }}</span>
          </div>
          <p class="muted" *ngIf="hangfireServers.length === 0">Chưa có Hangfire server hoạt động.</p>
        </div>
      </section>
      <section class="admin-grid">
        <a routerLink="/admin/orders" class="admin-card cosmic-glass">
          <span class="material-symbols-outlined">local_shipping</span>
          <div><h2>Đơn hàng & giao hàng</h2><p>Kiểm tra và cập nhật trạng thái vận chuyển.</p></div>
          <span class="material-symbols-outlined arrow">arrow_forward</span>
        </a>
        <a routerLink="/admin/inventory" class="admin-card cosmic-glass">
          <span class="material-symbols-outlined">inventory_2</span>
          <div><h2>Kho hiện vật</h2><p>Quản lý danh mục và sản phẩm trong cửa hàng.</p></div>
          <span class="material-symbols-outlined arrow">arrow_forward</span>
        </a>
        <a routerLink="/admin/accounts" class="admin-card cosmic-glass">
          <span class="material-symbols-outlined">manage_accounts</span>
          <div><h2>Tài khoản</h2><p>Quản lý employee, role và permission.</p></div>
          <span class="material-symbols-outlined arrow">arrow_forward</span>
        </a>
      </section>
    </main>
  `,
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnDestroy, OnInit {
  private readonly http = inject(HttpClient);
  private readonly base = API_CONFIG.baseUrl;
  revenue = 0;
  ordersCount = 0;
  paidCount = 0;
  researchJob = {
    name: 'Nghiên cứu thiên văn',
    enabled: false,
    schedule: 'Đang tải...',
    pageSize: 0,
    maxRecordsPerRun: 0,
    source: 'SIMBAD'
  };
  researchJobRunning = false;
  researchJobMessage = 'Đang tải trạng thái Hangfire...';
  private researchPoll?: ReturnType<typeof setInterval>;
  hangfireTab: 'jobs' | 'retries' | 'recurring' | 'servers' = 'jobs';
  hangfireServers: Array<{ name: string; workers: number; queues: string[]; heartbeatUtc: string }> = [];
  hangfireJobs: Array<any> = [];
  hangfirePage = 1;
  readonly hangfirePageSize = 10;
  hangfireTotalCount = 0;
  totalHangfirePages = 0;
  hangfireJobsLoading = false;
  hangfireJobsError = '';
  recurringJobs: Array<any> = [];
  recurringPage = 1;
  readonly recurringPageSize = 10;
  recurringTotalCount = 0;
  recurringTotalPages = 0;
  recurringJobsLoading = false;
  recurringJobsError = '';
  hangfireStats = [
    { label: 'Enqueued', key: 'enqueued', value: 0 },
    { label: 'Scheduled', key: 'scheduled', value: 0 },
    { label: 'Processing', key: 'processing', value: 0 },
    { label: 'Succeeded', key: 'succeeded', value: 0 },
    { label: 'Failed', key: 'failed', value: 0 },
    { label: 'Retries', key: 'retries', value: 0 }
  ];
  statusChart = [
    { label: 'Chờ thanh toán', status: 'PendingPayment', count: 0, percent: 0 },
    { label: 'Đã thanh toán', status: 'Paid', count: 0, percent: 0 },
    { label: 'Đang giao hàng', status: 'Shipping', count: 0, percent: 0 },
    { label: 'Đã giao hàng', status: 'Delivered', count: 0, percent: 0 }
  ];
  ngOnInit(): void {
    this.http.get<any[]>(`${this.base}/api/admin/orders/today`).subscribe({
      next: orders => {
        this.ordersCount = orders.length;
        this.paidCount = orders.filter(order => ['Paid', 'Shipping', 'Delivered'].includes(order.status)).length;
        this.revenue = orders.filter(order => order.status !== 'Cancelled').reduce((sum, order) => sum + Number(order.totalAmount || 0), 0);
        this.statusChart = this.statusChart.map(item => ({
          ...item,
          count: orders.filter(order => order.status === item.status).length,
          percent: this.ordersCount ? Math.round(orders.filter(order => order.status === item.status).length * 100 / this.ordersCount) : 0
        }));
      }
    });
    this.loadResearchJob();
    this.loadHangfireOverview();
  }

  loadHangfireOverview(): void {
    this.http.get<{
      jobs: Record<string, number>;
      servers: Array<{ name: string; workers: number; queues: string[]; heartbeatUtc: string }>;
    }>(`${this.base}/api/admin/jobs/hangfire`).subscribe({
      next: overview => {
        this.hangfireServers = overview.servers;
        this.hangfireStats = this.hangfireStats.map(item => ({
          ...item,
          value: overview.jobs[item.key] ?? 0
        }));
        this.loadHangfireJobs(this.hangfireTab === 'retries' ? 'failed' : 'succeeded');
      }
    });
  }

  setHangfireTab(tab: 'jobs' | 'retries' | 'recurring' | 'servers'): void {
    this.hangfireTab = tab;
    if (tab === 'jobs' || tab === 'retries') {
      this.hangfirePage = 1;
      this.loadHangfireJobs(tab === 'retries' ? 'failed' : 'succeeded');
    } else if (tab === 'recurring') {
      this.recurringPage = 1;
      this.loadRecurringJobs();
    }
  }

  goToHangfirePage(page: number): void {
    if (page < 1 || page > this.totalHangfirePages || page === this.hangfirePage) return;
    this.hangfirePage = page;
    this.loadHangfireJobs(this.hangfireTab === 'retries' ? 'failed' : 'succeeded');
  }

  loadHangfireJobs(state: string): void {
    this.hangfireJobsLoading = true;
    this.hangfireJobsError = '';
    const params = { page: String(this.hangfirePage), pageSize: String(this.hangfirePageSize) };
    this.http.get<{
      jobs: Array<any>;
      page: number;
      totalCount: number;
      totalPages: number;
    }>(`${this.base}/api/admin/jobs/hangfire/jobs/${state}`, { params }).subscribe({
      next: result => {
        this.hangfireJobs = result.jobs;
        this.hangfirePage = result.page;
        this.hangfireTotalCount = result.totalCount;
        this.totalHangfirePages = result.totalPages;
        this.hangfireJobsLoading = false;
      },
      error: () => {
        this.hangfireJobsError = 'Không thể tải danh sách Hangfire jobs.';
        this.hangfireJobsLoading = false;
      }
    });
  }

  loadRecurringJobs(): void {
    this.recurringJobsLoading = true;
    this.recurringJobsError = '';
    const params = { page: String(this.recurringPage), pageSize: String(this.recurringPageSize) };
    this.http.get<{
      jobs: Array<any>;
      page: number;
      totalCount: number;
      totalPages: number;
    }>(`${this.base}/api/admin/jobs/hangfire/recurring`, { params }).subscribe({
      next: result => {
        this.recurringJobs = result.jobs;
        this.recurringPage = result.page;
        this.recurringTotalCount = result.totalCount;
        this.recurringTotalPages = result.totalPages;
        this.recurringJobsLoading = false;
      },
      error: () => {
        this.recurringJobsError = 'Không thể tải recurring jobs.';
        this.recurringJobsLoading = false;
      }
    });
  }

  goToRecurringPage(page: number): void {
    if (page < 1 || page > this.recurringTotalPages || page === this.recurringPage) return;
    this.recurringPage = page;
    this.loadRecurringJobs();
  }

  triggerRecurringJob(jobId: string): void {
    this.http.post(`${this.base}/api/admin/jobs/hangfire/recurring/${encodeURIComponent(jobId)}/trigger`, {}).subscribe({
      next: () => this.loadHangfireOverview()
    });
  }

  requeueHangfireJob(jobId: string): void {
    this.http.post(`${this.base}/api/admin/jobs/hangfire/jobs/${encodeURIComponent(jobId)}/requeue`, {}).subscribe({
      next: () => this.loadHangfireOverview()
    });
  }

  deleteHangfireJob(jobId: string): void {
    this.http.delete(`${this.base}/api/admin/jobs/hangfire/jobs/${encodeURIComponent(jobId)}`).subscribe({
      next: () => this.loadHangfireOverview()
    });
  }

  ngOnDestroy(): void {
    this.stopResearchPolling();
  }

  private loadResearchJob(): void {
    this.http.get<{
      name: string;
      enabled: boolean;
      schedule: string;
      pageSize: number;
      maxPagesPerRun: number;
      maxRecordsPerRun: number;
      source: string;
    }>(`${this.base}/api/admin/jobs/astronomical-research`).subscribe({
      next: job => {
        this.researchJob = job;
        this.researchJobMessage = job.enabled
          ? 'Sẵn sàng chạy thủ công hoặc theo lịch.'
          : 'Job đang bị tắt trong cấu hình backend.';
      },
      error: () => {
        this.researchJobMessage = 'Không thể tải trạng thái Hangfire.';
      }
    });
  }

  runResearchJob(): void {
    if (this.researchJobRunning || !this.researchJob.enabled) {
      return;
    }

    this.researchJobRunning = true;
    this.researchJobMessage = 'Đang đưa job vào hàng đợi...';
    this.http.post<{ jobId: string }>(
      `${this.base}/api/admin/jobs/astronomical-research/run`,
      {}).subscribe({
        next: response => {
          this.researchJobMessage = `Đã xếp hàng (${response.jobId}). Đang chờ worker...`;
          this.pollResearchJob(response.jobId);
        },
        error: () => {
          this.researchJobRunning = false;
          this.researchJobMessage = 'Không thể xếp hàng job. Vui lòng thử lại.';
        }
      });
  }

  private pollResearchJob(jobId: string): void {
    let attempts = 0;
    this.researchPoll = setInterval(() => {
      attempts++;
      this.http.get<{
        state: string;
        lastRun?: {
          candidateCount: number;
          existingCount: number;
          createdCount: number;
          pagesProcessed: number;
          sweepCompleted: boolean;
          nextCursor?: string;
        };
      }>(`${this.base}/api/admin/jobs/astronomical-research/runs/${jobId}`).subscribe({
        next: result => {
          if (result.state === 'Succeeded') {
            this.stopResearchPolling();
            this.researchJobRunning = false;
            const created = result.lastRun?.createdCount ?? 0;
            const candidates = result.lastRun?.candidateCount ?? 0;
            const existing = result.lastRun?.existingCount ?? 0;
            const pages = result.lastRun?.pagesProcessed ?? 0;
            this.researchJobMessage = result.lastRun?.sweepCompleted
              ? `Đã quét hết nguồn: ${created} mới / ${existing} đã có trên ${candidates} bản ghi (${pages} trang).`
              : `Đã xử lý batch: ${created} mới / ${existing} đã có trên ${candidates} bản ghi (${pages} trang). Sẽ tiếp tục từ ${result.lastRun?.nextCursor ?? 'cursor đã lưu'}.`;
          } else if (result.state === 'Failed') {
            this.stopResearchPolling();
            this.researchJobRunning = false;
            this.researchJobMessage = 'Job thất bại. Kiểm tra log Catalog để biết chi tiết.';
          } else if (attempts >= 300) {
            this.stopResearchPolling();
            this.researchJobRunning = false;
            this.researchJobMessage = `Job hiện ở trạng thái ${result.state}.`;
          }
        },
        error: () => {
          if (attempts >= 300) {
            this.stopResearchPolling();
            this.researchJobRunning = false;
            this.researchJobMessage = 'Không thể kiểm tra trạng thái job.';
          }
        }
      });
    }, 2000);
  }

  private stopResearchPolling(): void {
    if (this.researchPoll) {
      clearInterval(this.researchPoll);
      this.researchPoll = undefined;
    }
  }
}
