import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '../../core/config/api.config';

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
      <section class="admin-jobs hangfire-console cosmic-glass">
        <header class="hangfire-console__header">
          <div>
            <p class="eyebrow">BACKGROUND PROCESSING</p>
            <h2>Hangfire Console</h2>
            <p class="muted">Theo dõi queues, workers, lịch chạy và lịch sử xử lý.</p>
          </div>
          <div class="hangfire-console__health">
            <span class="hangfire-live-dot" [class.hangfire-live-dot--off]="hangfireServers.length === 0"></span>
            <span>{{ hangfireServers.length }} server đang hoạt động</span>
            <button type="button" class="hangfire-refresh" (click)="loadHangfireOverview()">
              <span class="material-symbols-outlined">refresh</span>
              Làm mới
            </button>
          </div>
        </header>
        <p class="hangfire-message hangfire-message--error" *ngIf="hangfireOverviewError">{{ hangfireOverviewError }}</p>
        <p class="hangfire-message" role="status" *ngIf="hangfireActionMessage">{{ hangfireActionMessage }}</p>
        <p class="hangfire-message hangfire-message--error" role="alert" *ngIf="hangfireActionError">{{ hangfireActionError }}</p>
        <div class="hangfire-stats">
          <button type="button" class="hangfire-stat" *ngFor="let item of hangfireStats"
            [class.hangfire-stat--active]="hangfireTab === 'jobs' && selectedHangfireState === item.key"
            [class.hangfire-stat--warning]="item.key === 'failed' && item.value > 0"
            (click)="selectHangfireState(item.key)">
            <span>{{ item.label }}</span><strong>{{ item.value }}</strong>
          </button>
          <div class="hangfire-stat hangfire-stat--workers">
            <span>Workers</span>
            <strong>{{ totalWorkers }}</strong>
          </div>
        </div>
        <nav class="hangfire-tabs" aria-label="Hangfire sections">
          <button type="button" [class.active]="hangfireTab === 'overview'" (click)="setHangfireTab('overview')">
            <span class="material-symbols-outlined">dashboard</span> Overview
          </button>
          <button type="button" [class.active]="hangfireTab === 'jobs'" (click)="setHangfireTab('jobs')">
            <span class="material-symbols-outlined">work_history</span> Jobs
          </button>
          <button type="button" [class.active]="hangfireTab === 'retries'" (click)="setHangfireTab('retries')">
            <span class="material-symbols-outlined">replay</span> Retries
          </button>
          <button type="button" [class.active]="hangfireTab === 'recurring'" (click)="setHangfireTab('recurring')">
            <span class="material-symbols-outlined">event_repeat</span> Recurring jobs
          </button>
          <button type="button" [class.active]="hangfireTab === 'servers'" (click)="setHangfireTab('servers')">
            <span class="material-symbols-outlined">dns</span> Servers & queues
          </button>
          <button type="button" disabled title="Batch processing requires Hangfire Pro">
            <span class="material-symbols-outlined">account_tree</span> Batches · Pro
          </button>
        </nav>
        <p class="hangfire-message">Batch processing is not available in the installed Hangfire edition; it requires Hangfire Pro.</p>
        <div class="hangfire-overview" *ngIf="hangfireTab === 'overview'">
          <div class="hangfire-section-heading">
            <div><h3>Overview</h3><span class="muted">Tổng quan xử lý nền và hiệu suất 24 giờ</span></div>
            <button type="button" class="hangfire-refresh" (click)="loadHangfireOverview()">
              <span class="material-symbols-outlined">refresh</span> Refresh
            </button>
          </div>
          <div class="hangfire-overview__summary">
            <div><span>Recurring jobs</span><strong>{{ overviewJobCounts.recurring }}</strong></div>
            <div><span>Queues</span><strong>{{ overviewJobCounts.queues }}</strong></div>
            <div><span>Servers</span><strong>{{ overviewJobCounts.servers }}</strong></div>
            <div><span>Workers</span><strong>{{ totalWorkers }}</strong></div>
          </div>
          <div class="hangfire-chart-grid">
            <section class="hangfire-chart-card">
              <h4>Hourly activity</h4>
              <div class="hangfire-chart-legend"><span class="success-key">Succeeded</span><span class="failed-key">Failed</span></div>
              <div class="hangfire-chart">
                <div class="hangfire-chart__column" *ngFor="let point of hourlyActivity">
                  <div class="hangfire-chart__bars">
                    <i class="hangfire-chart__bar--success" [style.height.%]="chartHeight(point.succeeded)"></i>
                    <i class="hangfire-chart__bar--failed" [style.height.%]="chartHeight(point.failed)"></i>
                  </div>
                  <time [title]="point.atUtc">{{ point.atUtc | date:'HH:mm' }}</time>
                </div>
              </div>
            </section>
            <section class="hangfire-chart-card">
              <h4>Daily history · 7 days</h4>
              <div class="hangfire-history-chart">
                <div class="hangfire-history-chart__row" *ngFor="let point of dailyActivity">
                  <time>{{ point.atUtc | date:'EEE dd' }}</time>
                    <div class="hangfire-history-chart__track" [attr.title]="point.succeeded + ' succeeded, ' + point.failed + ' failed'">
                      <i class="hangfire-chart__bar--success" [style.width.%]="historyChartWidth(point.succeeded)"></i>
                      <i class="hangfire-chart__bar--failed" [style.width.%]="historyChartWidth(point.failed)"></i>
                    </div>
                    <b>{{ point.succeeded + point.failed }}</b>
                </div>
              </div>
            </section>
          </div>
          <section class="hangfire-queues">
            <div class="hangfire-section-heading"><div><h3>Queues</h3><span class="muted">Fetched counts and next enqueued jobs</span></div></div>
            <div class="hangfire-table" *ngIf="hangfireQueues.length">
              <div class="hangfire-table__head hangfire-table__head--queues"><span>Queue</span><span>Length</span><span>Fetched</span><span>Next jobs</span></div>
              <article class="hangfire-row hangfire-row--queue" *ngFor="let queue of hangfireQueues">
                <button type="button" class="hangfire-queue-link" (click)="openQueue(queue.name)">{{ queue.name }}</button>
                <strong>{{ queue.length }}</strong>
                <span>{{ queue.fetched ?? 0 }}</span>
                <div class="hangfire-queue-next" *ngIf="queue.nextJobs.length; else noQueueJobs">
                  <button type="button" *ngFor="let job of queue.nextJobs" (click)="openQueueJob(queue.name, job.id)">
                    <code>{{ job.id | slice:0:12 }}…</code><span>{{ job.method }}</span>
                  </button>
                </div>
                <ng-template #noQueueJobs><span class="muted">No jobs queued.</span></ng-template>
              </article>
            </div>
            <p class="hangfire-message" *ngIf="!hangfireQueues.length">No queues reported by Hangfire storage.</p>
          </section>
        </div>
        <div class="hangfire-job-list" *ngIf="hangfireTab === 'jobs'">
          <div class="hangfire-section-heading">
            <div>
              <h3>{{ selectedHangfireStateLabel }}</h3>
              <span class="muted">{{ hangfireTotalCount }} jobs in this state</span>
            </div>
            <div class="hangfire-section-heading__actions">
              <label *ngIf="selectedHangfireState === 'enqueued'">Queue
                <select [value]="selectedQueue" (change)="selectQueue($event)">
                  <option *ngFor="let queue of hangfireQueues" [value]="queue.name">{{ queue.name }}</option>
                </select>
              </label>
              <label>Per page
                <select [value]="hangfirePageSizeValue" (change)="changeHangfirePageSize($event)">
                  <option [value]="10">10</option><option [value]="20">20</option>
                  <option [value]="50">50</option><option [value]="100">100</option>
                </select>
              </label>
              <button type="button" class="hangfire-refresh" (click)="loadHangfireJobs(selectedHangfireState)">
                <span class="material-symbols-outlined">refresh</span> Refresh
              </button>
            </div>
          </div>
          <p class="hangfire-message hangfire-message--error" *ngIf="hangfireJobsError">{{ hangfireJobsError }}</p>
          <p class="hangfire-message" *ngIf="hangfireJobsLoading">Đang tải jobs...</p>
          <div class="hangfire-table" *ngIf="!hangfireJobsLoading && hangfireJobs.length">
            <div class="hangfire-table__head">
              <span>Job</span><span>State</span><span>Created / queue</span><span>Actions</span>
            </div>
            <article class="hangfire-row" *ngFor="let job of hangfireJobs">
              <div class="hangfire-row__method">
                <strong>{{ job.method }}</strong>
                <span>{{ job.type }}<ng-container *ngIf="job.error"> · {{ job.error }}</ng-container></span>
              </div>
              <span class="hangfire-state" [attr.data-state]="job.state">{{ job.state }}</span>
              <div class="hangfire-times"><code>{{ job.id }}</code><span>{{ job.timestamp | date:'medium' }}</span><span *ngIf="job.queue">{{ job.queue }}</span><span *ngIf="job.duration != null">Duration: {{ job.duration }} ms</span></div>
              <div class="hangfire-row__actions">
                <button type="button" (click)="toggleHangfireJobDetails(job.id)">
                  {{ selectedHangfireJob?.id === job.id ? 'Ẩn chi tiết' : 'Chi tiết' }}
                </button>
                <button type="button" *ngIf="job.state === 'failed'" [disabled]="hangfireActionInProgress" (click)="requeueHangfireJob(job.id)">Requeue</button>
                <button type="button" class="hangfire-row__delete" [disabled]="hangfireActionInProgress" (click)="deleteHangfireJob(job.id)">Xóa</button>
              </div>
              <div class="hangfire-detail" *ngIf="selectedHangfireJob?.id === job.id">
                <p *ngIf="hangfireDetailsLoading">Đang tải job details...</p>
                <ng-container *ngIf="selectedHangfireJob && !hangfireDetailsLoading">
                  <div class="hangfire-detail__meta">
                    <span><b>Type</b>{{ selectedHangfireJob.type }}</span>
                    <span><b>Method</b>{{ selectedHangfireJob.method }}</span>
                    <span><b>State</b>{{ selectedHangfireJob.currentState }}</span>
                    <span><b>Created</b>{{ selectedHangfireJob.createdAt | date:'medium' }}</span>
                    <span><b>Expires</b>{{ selectedHangfireJob.expireAt | date:'medium' }}</span>
                    <span><b>Retry count</b>{{ selectedHangfireJob.retryCount || '0' }}</span>
                  </div>
                  <div class="hangfire-exception" *ngIf="selectedHangfireJob.exceptionMessage">
                    <h4>{{ selectedHangfireJob.exceptionType || 'Exception' }}</h4>
                    <p>{{ selectedHangfireJob.exceptionMessage }}</p>
                    <pre>{{ selectedHangfireJob.exceptionDetails }}</pre>
                  </div>
                  <div class="hangfire-arguments">
                    <h4>Parameters</h4>
                    <div *ngFor="let parameter of objectEntries(selectedHangfireJob.parameters)">
                      <code>{{ parameter[0] }}</code><span>{{ parameter[1] }}</span>
                    </div>
                  </div>
                  <div class="hangfire-arguments">
                    <h4>Arguments</h4>
                    <div *ngFor="let argument of selectedHangfireJob.arguments">
                      <code>{{ argument.name }}</code><span>{{ argument.value }}</span>
                    </div>
                  </div>
                  <div class="hangfire-history">
                    <h4>State history</h4>
                    <div *ngFor="let event of selectedHangfireJob.history">
                      <span class="hangfire-state" [attr.data-state]="event.state.toLowerCase()">{{ event.state }}</span>
                      <time>{{ event.at | date:'medium' }}</time>
                      <span>{{ event.reason || event.data['ExceptionMessage'] || event.data['EnqueueAt'] || '' }}</span>
                    </div>
                  </div>
                </ng-container>
                <p class="hangfire-message hangfire-message--error" *ngIf="hangfireDetailsError">{{ hangfireDetailsError }}</p>
              </div>
            </article>
          </div>
          <p class="hangfire-message" *ngIf="!hangfireJobsLoading && !hangfireJobsError && !hangfireJobs.length">Không có job trong trạng thái này.</p>
          <nav class="hangfire-pagination" aria-label="Phân trang Hangfire jobs" *ngIf="!hangfireJobsLoading && totalHangfirePages > 1">
            <button type="button" [disabled]="hangfirePage <= 1" (click)="goToHangfirePage(hangfirePage - 1)">Trước</button>
            <span>Trang {{ hangfirePage }} / {{ totalHangfirePages }} · {{ hangfireTotalCount }} jobs</span>
            <button type="button" [disabled]="hangfirePage >= totalHangfirePages" (click)="goToHangfirePage(hangfirePage + 1)">Sau</button>
          </nav>
        </div>
        <div class="hangfire-job-list" *ngIf="hangfireTab === 'retries'">
          <div class="hangfire-section-heading">
            <div><h3>Retries</h3><span class="muted">{{ retryTotalCount }} scheduled retry jobs</span></div>
            <div class="hangfire-section-heading__actions">
              <label>Per page
                <select [value]="retryPageSize" (change)="changeRetryPageSize($event)">
                  <option [value]="10">10</option><option [value]="20">20</option>
                  <option [value]="50">50</option><option [value]="100">100</option>
                </select>
              </label>
              <button type="button" class="hangfire-refresh" (click)="loadHangfireRetries()">
                <span class="material-symbols-outlined">refresh</span> Refresh
              </button>
            </div>
          </div>
          <p class="hangfire-message hangfire-message--error" *ngIf="retriesError">{{ retriesError }}</p>
          <p class="hangfire-message" *ngIf="retryScanLimited">
            Scanned {{ retryScannedCount }} of {{ retryTotalScheduledCount }} scheduled jobs (limit {{ retryScanLimit }}).
          </p>
          <p class="hangfire-message" *ngIf="retriesLoading">Loading retries...</p>
          <div class="hangfire-table" *ngIf="!retriesLoading && hangfireRetries.length">
            <div class="hangfire-table__head hangfire-table__head--retries"><span>Job ID</span><span>State</span><span>Job</span><span>Reason</span><span>Retry / scheduled</span><span>Actions</span></div>
            <article class="hangfire-row hangfire-row--retries" *ngFor="let job of hangfireRetries">
              <code>{{ job.id }}</code>
              <span class="hangfire-state" data-state="scheduled">Scheduled</span>
              <div class="hangfire-row__method"><strong>{{ job.type }}.{{ job.method }}</strong></div>
              <span class="hangfire-retry-reason">{{ job.error || 'Automatic retry scheduled' }}</span>
              <div class="hangfire-times"><b>Attempt {{ job.retryCount }}</b><span>{{ job.timestamp | date:'medium' }}</span></div>
              <div class="hangfire-row__actions">
                <button type="button" (click)="openScheduledJob(job.id)">Details</button>
                <button type="button" [disabled]="hangfireActionInProgress" (click)="requeueHangfireJob(job.id)">Requeue now</button>
                <button type="button" class="hangfire-row__delete" [disabled]="hangfireActionInProgress" (click)="deleteJobFromList(job.id)">Delete</button>
              </div>
            </article>
          </div>
          <p class="hangfire-message" *ngIf="!retriesLoading && !retriesError && !hangfireRetries.length">No scheduled retries.</p>
          <nav class="hangfire-pagination" *ngIf="!retriesLoading && retryTotalPages > 1" aria-label="Retry pagination">
            <button type="button" [disabled]="retryPage <= 1" (click)="goToRetryPage(retryPage - 1)">Prev</button>
            <span>Page {{ retryPage }} / {{ retryTotalPages }} · {{ retryTotalCount }} retries</span>
            <button type="button" [disabled]="retryPage >= retryTotalPages" (click)="goToRetryPage(retryPage + 1)">Next</button>
          </nav>
        </div>
        <div class="hangfire-recurring" *ngIf="hangfireTab === 'recurring'">
          <div class="hangfire-section-heading">
            <div><h3>Recurring jobs</h3><span class="muted">Lịch trình và lần chạy tiếp theo</span></div>
            <button type="button" class="hangfire-refresh" (click)="loadRecurringJobs()">
              <span class="material-symbols-outlined">refresh</span> Làm mới
            </button>
          </div>
          <p class="hangfire-message hangfire-message--error" *ngIf="recurringJobsError">{{ recurringJobsError }}</p>
          <div class="hangfire-table" *ngIf="!recurringJobsLoading && recurringJobs.length">
            <div class="hangfire-table__head hangfire-table__head--recurring">
              <span>Recurring job</span><span>Schedule / Queue</span><span>Next / Last execution</span><span></span>
            </div>
            <article class="hangfire-row hangfire-row--recurring" *ngFor="let job of recurringJobs">
              <div class="hangfire-row__method"><strong>{{ job.id }}</strong><span>{{ job.type }}.{{ job.method }}</span><span>Last job: {{ job.lastJobId || '—' }}</span></div>
              <div><code>{{ job.cron }}</code><span class="hangfire-queue">{{ job.queue }}</span></div>
              <div class="hangfire-times"><span>{{ job.timeZoneId }}</span><span>Next: {{ job.nextExecution | date:'medium' }}</span><span>Last: {{ job.lastExecution | date:'medium' }}</span><span>Created: {{ job.createdAt | date:'medium' }}</span></div>
              <div class="hangfire-row__actions">
                <button type="button" [disabled]="hangfireActionInProgress" (click)="triggerRecurringJob(job.id)">Trigger now</button>
                <button type="button" class="hangfire-row__delete" [disabled]="hangfireActionInProgress" (click)="deleteRecurringJob(job.id)">Delete</button>
              </div>
              <p class="hangfire-recurring__error" *ngIf="job.error">{{ job.error }}</p>
            </article>
          </div>
          <p class="hangfire-message" *ngIf="recurringJobsLoading">Đang tải lịch chạy...</p>
          <p class="hangfire-message" *ngIf="!recurringJobsLoading && !recurringJobsError && !recurringJobs.length">Không có recurring job.</p>
          <nav class="hangfire-pagination" aria-label="Phân trang recurring jobs" *ngIf="!recurringJobsLoading && recurringTotalPages > 1">
            <button type="button" [disabled]="recurringPage <= 1" (click)="goToRecurringPage(recurringPage - 1)">Trước</button>
            <span>Trang {{ recurringPage }} / {{ recurringTotalPages }} · {{ recurringTotalCount }} jobs</span>
            <button type="button" [disabled]="recurringPage >= recurringTotalPages" (click)="goToRecurringPage(recurringPage + 1)">Sau</button>
          </nav>
        </div>
        <div class="hangfire-servers" *ngIf="hangfireTab === 'servers'">
          <article class="hangfire-server-card" *ngFor="let server of hangfireServers">
            <div class="hangfire-server-card__heading">
              <span class="hangfire-live-dot"></span><strong>{{ server.name }}</strong>
              <span class="hangfire-state" data-state="processing">Online</span>
            </div>
            <div class="hangfire-server-card__metrics">
              <span><b>{{ server.workers }}</b> workers</span>
              <span><b>{{ server.queues.length }}</b> queues</span>
              <span>Started {{ server.startedAtUtc | date:'medium' }}</span>
              <span>Heartbeat {{ server.heartbeatUtc | date:'medium' }}</span>
            </div>
            <div class="hangfire-queue-list"><span *ngFor="let queue of server.queues">{{ queue }}</span></div>
          </article>
          <p class="hangfire-message" *ngIf="hangfireServers.length === 0">Chưa có Hangfire server hoạt động.</p>
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
