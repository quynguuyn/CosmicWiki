import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '../../core/config/api.config';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

type HangfireJobState =
  | 'enqueued'
  | 'scheduled'
  | 'processing'
  | 'succeeded'
  | 'failed'
  | 'deleted';

interface HangfireJobSummary {
  id: string;
  state: HangfireJobState;
  type: string;
  method: string;
  queue?: string | null;
  timestamp?: string | null;
  retryCount?: string | null;
  error?: string | null;
  duration?: number | null;
}

interface HangfireJobDetails {
  id: string;
  type: string;
  method: string;
  currentState: string;
  createdAt: string | null;
  expireAt: string | null;
  retryCount: string | null;
  exceptionType: string | null;
  exceptionMessage: string | null;
  exceptionDetails: string | null;
  parameters: Record<string, string>;
  arguments: Array<{ name: string; value: string }>;
  history: Array<{
    state: string;
    at: string;
    reason: string | null;
    data: Record<string, string>;
  }>;
}

interface HangfireRecurringJob {
  id: string;
  cron: string;
  queue: string;
  timeZoneId: string;
  type: string | null;
  method: string | null;
  createdAt: string | null;
  nextExecution: string | null;
  lastExecution: string | null;
  lastJobId: string | null;
  error: string | null;
}

interface HangfireQueue {
  name: string;
  length: number;
  fetched: number | null;
  nextJobs: HangfireJobSummary[];
}

interface HangfireMetricPoint {
  atUtc: string;
  count: number;
}

interface HangfireActivityPoint {
  atUtc: string;
  succeeded: number;
  failed: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './admin-dashboard.component.html',
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
  hangfireTab: 'overview' | 'jobs' | 'retries' | 'recurring' | 'servers' = 'overview';
  hangfireServers: Array<{
    name: string;
    workers: number;
    queues: string[];
    startedAtUtc: string;
    heartbeatUtc: string;
  }> = [];
  hangfireQueues: HangfireQueue[] = [];
  hourlySucceeded: HangfireMetricPoint[] = [];
  hourlyFailed: HangfireMetricPoint[] = [];
  dailySucceeded: HangfireMetricPoint[] = [];
  dailyFailed: HangfireMetricPoint[] = [];
  selectedQueue = 'default';
  overviewJobCounts = { recurring: 0, queues: 0, servers: 0 };
  retriesLoading = false;
  retriesError = '';
  hangfireRetries: HangfireJobSummary[] = [];
  retryPage = 1;
  retryPageSize = 20;
  retryTotalCount = 0;
  retryTotalPages = 0;
  retryScanLimit = 5000;
  retryScannedCount = 0;
  retryTotalScheduledCount = 0;
  retryScanLimited = false;
  hangfireJobs: HangfireJobSummary[] = [];
  hangfirePage = 1;
  hangfirePageSizeValue = 20;
  hangfireTotalCount = 0;
  totalHangfirePages = 0;
  hangfireJobsLoading = false;
  hangfireJobsError = '';
  hangfireOverviewError = '';
  hangfireActionMessage = '';
  hangfireActionError = '';
  hangfireActionInProgress = false;
  selectedHangfireState: HangfireJobState = 'enqueued';
  selectedHangfireJob?: HangfireJobDetails;
  private pendingHangfireJobId?: string;
  hangfireDetailsLoading = false;
  hangfireDetailsError = '';
  recurringJobs: HangfireRecurringJob[] = [];
  recurringPage = 1;
  readonly recurringPageSize = 10;
  recurringTotalCount = 0;
  recurringTotalPages = 0;
  recurringJobsLoading = false;
  recurringJobsError = '';
  hangfireStats: Array<{ label: string; key: HangfireJobState; value: number }> = [
    { label: 'Enqueued', key: 'enqueued', value: 0 },
    { label: 'Scheduled', key: 'scheduled', value: 0 },
    { label: 'Processing', key: 'processing', value: 0 },
    { label: 'Succeeded', key: 'succeeded', value: 0 },
    { label: 'Failed', key: 'failed', value: 0 },
    { label: 'Deleted', key: 'deleted', value: 0 }
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
    this.hangfireOverviewError = '';
    this.http.get<{
      jobs: Record<string, number>;
      queues: HangfireQueue[];
      hourlySucceeded: HangfireMetricPoint[];
      hourlyFailed: HangfireMetricPoint[];
      dailySucceeded: HangfireMetricPoint[];
      dailyFailed: HangfireMetricPoint[];
      servers: Array<{
        name: string;
        workers: number;
        queues: string[];
        startedAtUtc: string;
        heartbeatUtc: string;
      }>;
    }>(`${this.base}/api/admin/jobs/hangfire`).subscribe({
      next: overview => {
        this.hangfireServers = overview.servers;
        this.hangfireQueues = overview.queues;
        if (!overview.queues.some(queue => queue.name === this.selectedQueue)) {
          this.selectedQueue = overview.queues[0]?.name ?? 'default';
        }
        this.hourlySucceeded = overview.hourlySucceeded;
        this.hourlyFailed = overview.hourlyFailed;
        this.dailySucceeded = overview.dailySucceeded;
        this.dailyFailed = overview.dailyFailed;
        this.overviewJobCounts = {
          recurring: overview.jobs['recurring'] ?? 0,
          queues: overview.queues.length,
          servers: overview.jobs['servers'] ?? overview.servers.length
        };
        this.hangfireStats = this.hangfireStats.map(item => ({
          ...item,
          value: overview.jobs[item.key] ?? 0
        }));
        this.loadHangfireJobs(this.selectedHangfireState);
        if (this.hangfireTab === 'retries') this.loadHangfireRetries();
      },
      error: () => {
        this.hangfireOverviewError = 'Không thể tải Hangfire overview.';
      }
    });
  }

  get selectedHangfireStateLabel(): string {
    return this.hangfireStats.find(item => item.key === this.selectedHangfireState)?.label ?? 'Jobs';
  }

  get totalWorkers(): number {
    return this.hangfireServers.reduce((total, server) => total + server.workers, 0);
  }

  get hourlyActivity(): HangfireActivityPoint[] {
    return this.mergeActivity(this.hourlySucceeded, this.hourlyFailed);
  }

  get dailyActivity(): HangfireActivityPoint[] {
    return this.mergeActivity(this.dailySucceeded, this.dailyFailed);
  }

  selectHangfireState(state: HangfireJobState): void {
    this.selectedHangfireState = state;
    this.hangfireTab = 'jobs';
    this.hangfirePage = 1;
    this.selectedHangfireJob = undefined;
    this.loadHangfireJobs(this.selectedHangfireState);
  }

  setHangfireTab(tab: 'overview' | 'jobs' | 'retries' | 'recurring' | 'servers'): void {
    this.hangfireTab = tab;
    if (tab === 'jobs') {
      this.loadHangfireJobs(this.selectedHangfireState);
    } else if (tab === 'retries') {
      this.loadHangfireRetries();
    } else if (tab === 'recurring') {
      this.recurringPage = 1;
      this.loadRecurringJobs();
    }
  }

  goToHangfirePage(page: number): void {
    if (page < 1 || page > this.totalHangfirePages || page === this.hangfirePage) return;
    this.hangfirePage = page;
    this.loadHangfireJobs(this.selectedHangfireState);
  }

  loadHangfireJobs(state: HangfireJobState): void {
    this.hangfireJobsLoading = true;
    this.hangfireJobsError = '';
    const params: Record<string, string> = {
      page: String(this.hangfirePage),
      pageSize: String(this.hangfirePageSizeValue)
    };
    if (state === 'enqueued') params['queue'] = this.selectedQueue;
    this.http.get<{
      jobs: HangfireJobSummary[];
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
        if (this.pendingHangfireJobId && result.jobs.some(job => job.id === this.pendingHangfireJobId)) {
          const jobId = this.pendingHangfireJobId;
          this.pendingHangfireJobId = undefined;
          this.toggleHangfireJobDetails(jobId);
        }
      },
      error: () => {
        this.hangfireJobsError = 'Không thể tải danh sách Hangfire jobs.';
        this.hangfireJobsLoading = false;
      }
    });
  }

  openQueue(queueName: string): void {
    this.selectedQueue = queueName;
    this.selectedHangfireState = 'enqueued';
    this.hangfirePage = 1;
    this.hangfireTab = 'jobs';
    this.loadHangfireJobs('enqueued');
  }

  openQueueJob(queueName: string, jobId: string): void {
    this.selectedQueue = queueName;
    this.selectedHangfireState = 'enqueued';
    this.hangfirePage = 1;
    this.hangfireTab = 'jobs';
    this.pendingHangfireJobId = jobId;
    this.loadHangfireJobs('enqueued');
  }

  openScheduledJob(jobId: string): void {
    this.selectedHangfireState = 'scheduled';
    this.hangfirePage = 1;
    this.hangfireTab = 'jobs';
    this.pendingHangfireJobId = jobId;
    this.loadHangfireJobs('scheduled');
  }

  selectQueue(event: Event): void {
    this.selectedQueue = (event.target as HTMLSelectElement).value;
    this.hangfirePage = 1;
    this.loadHangfireJobs('enqueued');
  }

  changeHangfirePageSize(event: Event): void {
    this.hangfirePageSizeValue = Number((event.target as HTMLSelectElement).value);
    this.hangfirePage = 1;
    this.loadHangfireJobs(this.selectedHangfireState);
  }

  chartHeight(value: number): number {
    const max = Math.max(1, ...this.hourlySucceeded.map(point => point.count),
      ...this.hourlyFailed.map(point => point.count));
    return Math.max(value > 0 ? 4 : 0, value * 100 / max);
  }

  hourlyFailedCount(atUtc: string): number {
    return this.hourlyFailed.find(point => point.atUtc === atUtc)?.count ?? 0;
  }

  historyChartWidth(value: number): number {
    const max = Math.max(1, ...this.dailySucceeded.map(point =>
      point.count + this.dailyFailedCount(point.atUtc)));
    return Math.max(value > 0 ? 2 : 0, value * 100 / max);
  }

  dailyFailedCount(atUtc: string): number {
    return this.dailyFailed.find(point => point.atUtc === atUtc)?.count ?? 0;
  }

  private mergeActivity(
    succeeded: HangfireMetricPoint[],
    failed: HangfireMetricPoint[]
  ): HangfireActivityPoint[] {
    const points = new Map<string, HangfireActivityPoint>();
    for (const point of succeeded) {
      points.set(point.atUtc, { atUtc: point.atUtc, succeeded: point.count, failed: 0 });
    }
    for (const point of failed) {
      const activity = points.get(point.atUtc) ?? { atUtc: point.atUtc, succeeded: 0, failed: 0 };
      activity.failed = point.count;
      points.set(point.atUtc, activity);
    }
    return [...points.values()].sort((left, right) =>
      Date.parse(left.atUtc) - Date.parse(right.atUtc));
  }

  objectEntries(value: Record<string, string> | null | undefined): Array<[string, string]> {
    return Object.entries(value ?? {});
  }

  loadHangfireRetries(): void {
    this.retriesLoading = true;
    this.retriesError = '';
    const params = { page: String(this.retryPage), pageSize: String(this.retryPageSize) };
    this.http.get<{
      jobs: HangfireJobSummary[];
      page: number;
      totalCount: number;
      totalPages: number;
      scanLimit: number;
      totalScheduledCount: number;
      scannedScheduledCount: number;
      isScanLimited: boolean;
    }>(`${this.base}/api/admin/jobs/hangfire/retries`, { params }).subscribe({
      next: result => {
        this.hangfireRetries = result.jobs;
        this.retryPage = result.page;
        this.retryTotalCount = result.totalCount;
        this.retryTotalPages = result.totalPages;
        this.retryScanLimit = result.scanLimit;
        this.retryScannedCount = result.scannedScheduledCount;
        this.retryTotalScheduledCount = result.totalScheduledCount;
        this.retryScanLimited = result.isScanLimited;
        this.retriesLoading = false;
      },
      error: () => {
        this.retriesError = 'Không thể tải danh sách retry jobs.';
        this.retriesLoading = false;
      }
    });
  }

  goToRetryPage(page: number): void {
    if (page < 1 || page > this.retryTotalPages || page === this.retryPage) return;
    this.retryPage = page;
    this.loadHangfireRetries();
  }

  changeRetryPageSize(event: Event): void {
    this.retryPageSize = Number((event.target as HTMLSelectElement).value);
    this.retryPage = 1;
    this.loadHangfireRetries();
  }

  deleteJobFromList(jobId: string): void {
    this.deleteHangfireJob(jobId);
  }

  toggleHangfireJobDetails(jobId: string): void {
    if (this.selectedHangfireJob?.id === jobId) {
      this.selectedHangfireJob = undefined;
      this.hangfireDetailsError = '';
      return;
    }

    this.selectedHangfireJob = undefined;
    this.hangfireDetailsLoading = true;
    this.hangfireDetailsError = '';
    this.http.get<HangfireJobDetails>(
      `${this.base}/api/admin/jobs/hangfire/job-details/${encodeURIComponent(jobId)}`
    ).subscribe({
      next: details => {
        this.selectedHangfireJob = details;
        this.hangfireDetailsLoading = false;
      },
      error: () => {
        this.hangfireDetailsError = 'Không thể tải chi tiết job.';
        this.hangfireDetailsLoading = false;
      }
    });
  }

  loadRecurringJobs(): void {
    this.recurringJobsLoading = true;
    this.recurringJobsError = '';
    const params = { page: String(this.recurringPage), pageSize: String(this.recurringPageSize) };
    this.http.get<{
      jobs: HangfireRecurringJob[];
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

  deleteRecurringJob(jobId: string): void {
    if (!window.confirm(`Xóa recurring job ${jobId}?`)) return;
    this.startHangfireAction('Đang xóa recurring job...');
    this.http.delete(`${this.base}/api/admin/jobs/hangfire/recurring/${encodeURIComponent(jobId)}`).subscribe({
      next: () => {
        this.finishHangfireAction('Recurring job đã được xóa.');
        this.loadRecurringJobs();
        this.loadHangfireOverview();
      },
      error: () => this.failHangfireAction('Không thể xóa recurring job.')
    });
  }

  goToRecurringPage(page: number): void {
    if (page < 1 || page > this.recurringTotalPages || page === this.recurringPage) return;
    this.recurringPage = page;
    this.loadRecurringJobs();
  }

  triggerRecurringJob(jobId: string): void {
    this.startHangfireAction('Đang kích hoạt recurring job...');
    this.http.post(`${this.base}/api/admin/jobs/hangfire/recurring/${encodeURIComponent(jobId)}/trigger`, {}).subscribe({
      next: () => {
        this.finishHangfireAction('Recurring job đã được kích hoạt.');
        this.loadHangfireOverview();
        this.loadRecurringJobs();
      },
      error: () => this.failHangfireAction('Không thể kích hoạt recurring job.')
    });
  }

  requeueHangfireJob(jobId: string): void {
    this.startHangfireAction('Đang đưa job trở lại hàng đợi...');
    this.http.post(`${this.base}/api/admin/jobs/hangfire/jobs/${encodeURIComponent(jobId)}/requeue`, {}).subscribe({
      next: () => {
        this.finishHangfireAction('Job đã được đưa lại vào hàng đợi.');
        this.loadHangfireOverview();
      },
      error: () => this.failHangfireAction('Không thể requeue job.')
    });
  }

  deleteHangfireJob(jobId: string): void {
    if (!window.confirm(`Xóa Hangfire job ${jobId}?`)) return;
    this.startHangfireAction('Đang xóa job...');
    this.http.delete(`${this.base}/api/admin/jobs/hangfire/jobs/${encodeURIComponent(jobId)}`).subscribe({
      next: () => {
        this.finishHangfireAction('Job đã được xóa.');
        this.loadHangfireOverview();
      },
      error: () => this.failHangfireAction('Không thể xóa job.')
    });
  }

  private startHangfireAction(message: string): void {
    this.hangfireActionInProgress = true;
    this.hangfireActionMessage = message;
    this.hangfireActionError = '';
  }

  private finishHangfireAction(message: string): void {
    this.hangfireActionInProgress = false;
    this.hangfireActionMessage = message;
  }

  private failHangfireAction(message: string): void {
    this.hangfireActionInProgress = false;
    this.hangfireActionMessage = '';
    this.hangfireActionError = message;
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
