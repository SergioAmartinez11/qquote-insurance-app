import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { QuoteResponse, QuoteService } from '../../core/services/quote.service';
import { PolicyResponse, PolicyService } from '../../core/services/policy.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    RouterModule,
    ButtonModule,
    SkeletonModule,
    TagModule,
    DividerModule,
    DecimalPipe,
    DatePipe,
  ],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard implements OnInit {
  private quoteService = inject(QuoteService);
  private policyService = inject(PolicyService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  loading = true;
  quotes: QuoteResponse[] = [];
  policies: PolicyResponse[] = [];

  get totalQuotes() {
    return this.quotes.length;
  }
  get activeQuotes() {
    return this.quotes.filter((q) => q.status === 'Active').length;
  }
  get activePolicies() {
    return this.policies.filter((p) => p.isActive).length;
  }
  get totalMonthlyPremium() {
    return this.policies.filter((p) => p.isActive).reduce((s, p) => s + p.monthlyPremium, 0);
  }
  get currency() {
    return this.policies.find((p) => p.isActive)?.currency ?? 'USD';
  }
  get recentQuotes() {
    return [...this.quotes]
      .sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt))
      .slice(0, 5);
  }
  get activePolicyList() {
    return this.policies.filter((p) => p.isActive).slice(0, 5);
  }

  ngOnInit(): void {
    forkJoin({
      quotes: this.quoteService.getMyQuotes(),
      policies: this.policyService.getMyPolicies(),
    }).subscribe({
      next: ({ quotes, policies }) => {
        this.quotes = quotes;
        this.policies = policies;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loading = false;
        this.toast.fromHttpError(err, 'Failed to load dashboard.');
        this.cdr.markForCheck();
      },
    });
  }

  riskSeverity(level: string): 'success' | 'warn' | 'danger' {
    if (level === 'Low') return 'success';
    if (level === 'Medium') return 'warn';
    return 'danger';
  }

  quoteStatusSeverity(status: string): 'success' | 'warn' | 'secondary' {
    if (status === 'Active') return 'success';
    if (status === 'Pending') return 'warn';
    return 'secondary';
  }

  daysRemaining(endDate: string): number {
    return Math.max(0, Math.ceil((new Date(endDate).getTime() - Date.now()) / 86_400_000));
  }
}
