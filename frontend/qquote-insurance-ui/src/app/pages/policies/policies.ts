import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { PolicyResponse, PolicyService } from '../../core/services/policy.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-policies',
  standalone: true,
  imports: [
    RouterModule,
    ButtonModule,
    TableModule,
    DialogModule,
    TagModule,
    DividerModule,
    DecimalPipe,
    DatePipe,
  ],
  templateUrl: './policies.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Policies implements OnInit {
  private policyService = inject(PolicyService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  policies: PolicyResponse[] = [];
  loading = false;

  showDetailDialog = false;
  selectedPolicy: PolicyResponse | null = null;

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.loading = true;
    this.policyService.getMyPolicies().subscribe({
      next: (policies) => {
        this.policies = policies;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loading = false;
        this.toast.fromHttpError(err, 'Failed to load your policies.');
        this.cdr.markForCheck();
      },
    });
  }

  viewDetail(policy: PolicyResponse): void {
    this.selectedPolicy = policy;
    this.showDetailDialog = true;
  }

  statusSeverity(isActive: boolean): 'success' | 'secondary' {
    return isActive ? 'success' : 'secondary';
  }

  statusLabel(isActive: boolean): string {
    return isActive ? 'Active' : 'Cancelled';
  }

  daysRemaining(endDate: string): number {
    return Math.max(0, Math.ceil((new Date(endDate).getTime() - Date.now()) / 86_400_000));
  }
}
