import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { API_CONFIG } from '../../core/config/api.config';
import { PagedResult } from '../../core/models/paged-result.model';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

interface Account { userId: string; username: string; email: string; status: string; createdAt: string; roles: string[]; }
interface Role { roleId: string; name: string; description?: string; }
interface Permission { permissionId: string; code: string; name: string; description?: string; }

@Component({
  selector: 'app-admin-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent],
  templateUrl: './admin-accounts.component.html',
  styleUrl: './admin-crud.component.scss'
})
export class AdminAccountsComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly base = API_CONFIG.baseUrl;
  accounts: Account[] = []; roles: Role[] = []; permissions: Permission[] = []; rolePermissions: Permission[] = [];
  selected?: Account; selectedRole = ''; selectedPermissionRole = ''; selectedPermission = ''; roleFilter = ''; error = ''; saving = false;
  page = 1; readonly pageSize = 10; totalCount = 0; totalPages = 0; loading = false;
  readonly roleFilters = ['', 'User', 'Admin', 'Delivery'];

  ngOnInit() { this.loadAccounts(); this.http.get<Role[]>(`${this.base}/api/auth/admin/roles`).subscribe({ next: r => this.roles = r }); this.http.get<Permission[]>(`${this.base}/api/auth/admin/permissions`).subscribe({ next: p => this.permissions = p }); }
  loadAccounts() {
    this.loading = true;
    this.error = '';
    const params: Record<string, string> = { page: String(this.page), pageSize: String(this.pageSize) };
    if (this.roleFilter) params['role'] = this.roleFilter;
    this.http.get<PagedResult<Account>>(`${this.base}/api/auth/admin/accounts`, { params }).subscribe({
      next: result => {
        this.accounts = result.items;
        this.page = result.page;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        if (this.selected) {
          this.selected = this.accounts.find(account => account.userId === this.selected?.userId) ?? this.selected;
        }
        this.loading = false;
      },
      error: () => { this.error = 'Không thể tải tài khoản.'; this.loading = false; }
    });
  }
  setRoleFilter(role: string) { this.roleFilter = role; this.page = 1; this.loadAccounts(); }
  goToPage(page: number) {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.loadAccounts();
  }
  selectAccount(account: Account) {
    this.selected = account;
    this.selectedRole = '';
    this.selectedPermissionRole = '';
    this.selectedPermission = '';
    this.rolePermissions = [];
  }
  availableRoles(): Role[] {
    return this.roles.filter(role => !this.selected?.roles.includes(role.name));
  }
  removeRoleByName(roleName: string) {
    const role = this.roles.find(item => item.name === roleName);
    if (role) this.removeRole(role);
  }
  assignRole() {
    if (!this.selected || !this.selectedRole || this.saving) return;
    this.saving = true; this.error = '';
    this.http.post(`${this.base}/api/auth/users/${this.selected.userId}/roles/${this.selectedRole}`, {}).subscribe({
      next: () => { this.selectedRole = ''; this.saving = false; this.loadAccounts(); },
      error: error => { this.saving = false; this.error = error.error?.detail ?? 'Không thể gán vai trò.'; }
    });
  }
  removeRole(role: Role) {
    if (!this.selected || this.saving) return;
    this.saving = true; this.error = '';
    this.http.delete(`${this.base}/api/auth/users/${this.selected.userId}/roles/${role.roleId}`).subscribe({
      next: () => { this.saving = false; this.loadAccounts(); },
      error: error => { this.saving = false; this.error = error.error?.detail ?? 'Không thể gỡ vai trò.'; }
    });
  }
  assignPermission() {
    if (!this.selectedRole || !this.selectedPermission) return;
    this.http.put(`${this.base}/api/auth/admin/roles/${this.selectedRole}/permissions/${this.selectedPermission}`, {}).subscribe({ next: () => this.loadPermissions(), error: () => this.error = 'Không thể gán permission.' });
  }
  loadPermissions() {
    this.rolePermissions = [];
    if (!this.selectedPermissionRole) return;
    this.http.get<Permission[]>(`${this.base}/api/auth/admin/roles/${this.selectedPermissionRole}/permissions`).subscribe({
      next: p => this.rolePermissions = p,
      error: error => this.error = error.error?.detail ?? 'Không thể tải quyền của vai trò.'
    });
  }
}
