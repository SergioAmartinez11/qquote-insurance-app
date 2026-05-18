import { DatePipe, NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ClaimResponse, ClaimService, CreateClaimRequest } from '../../core/services/claim.service';
import { PolicyResponse, PolicyService } from '../../core/services/policy.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-claims',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    TableModule,
    DialogModule,
    SelectModule,
    TagModule,
    DividerModule,
    DatePickerModule,
    TextareaModule,
    DatePipe,
    NgClass,
  ],
  templateUrl: './claims.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Claims implements OnInit {
  private claimService = inject(ClaimService);
  private policyService = inject(PolicyService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  policies: PolicyResponse[] = [];
  policyOptions: { label: string; value: string }[] = [];
  selectedPolicyId: string | null = null;

  claims: ClaimResponse[] = [];
  loadingPolicies = false;
  loadingClaims = false;
  creating = false;

  showCreateDialog = false;
  selectedClaim: ClaimResponse | null = null;
  showDetailDialog = false;

  today = new Date();

  form: { incidentDate: Date | null; description: string } = this.emptyForm();

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.loadingPolicies = true;
    this.policyService.getMyPolicies().subscribe({
      next: (policies) => {
        this.policies = policies;
        this.policyOptions = policies.map((p) => ({
          label: `${p.vehicleYear} ${p.vehicleMake} ${p.vehicleModel} — ${p.coverageType}${p.isActive ? '' : ' (Cancelled)'}`,
          value: p.id,
        }));
        this.loadingPolicies = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loadingPolicies = false;
        this.toast.fromHttpError(err, 'Failed to load policies.');
        this.cdr.markForCheck();
      },
    });
  }

  onPolicySelect(): void {
    if (!this.selectedPolicyId) return;
    this.loadClaims(this.selectedPolicyId);
  }

  loadClaims(policyId: string): void {
    this.loadingClaims = true;
    this.claims = [];
    this.claimService.getByPolicy(policyId).subscribe({
      next: (claims) => {
        this.claims = claims;
        this.loadingClaims = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loadingClaims = false;
        this.toast.fromHttpError(err, 'Failed to load claims.');
        this.cdr.markForCheck();
      },
    });
  }

  openCreateDialog(): void {
    this.form = this.emptyForm();
    this.showCreateDialog = true;
  }

  submitClaim(): void {
    if (!this.selectedPolicyId) {
      this.toast.warn('Please select a policy first.');
      return;
    }
    if (!this.form.incidentDate) {
      this.toast.warn('Please select the incident date.');
      return;
    }
    if (!this.form.description.trim()) {
      this.toast.warn('Please describe the incident.');
      return;
    }

    const request: CreateClaimRequest = {
      policyId: this.selectedPolicyId,
      incidentDate: this.form.incidentDate.toISOString(),
      description: this.form.description.trim(),
    };

    this.creating = true;
    this.claimService.file(request).subscribe({
      next: (claim) => {
        this.claims = [claim, ...this.claims];
        this.creating = false;
        this.showCreateDialog = false;
        this.cdr.detectChanges();
        this.toast.success('Your claim has been filed and is under review.', 'Claim Filed');
      },
      error: (err) => {
        this.creating = false;
        this.toast.fromHttpError(err, 'Failed to file claim.');
        this.cdr.detectChanges();
      },
    });
  }

  viewDetail(claim: ClaimResponse): void {
    this.selectedClaim = claim;
    this.showDetailDialog = true;
  }

  statusSeverity(status: string): 'warn' | 'info' | 'success' | 'secondary' {
    if (status === 'Reported') return 'warn';
    if (status === 'UnderReview') return 'info';
    if (status === 'Resolved') return 'success';
    return 'secondary';
  }

  statusLabel(status: string): string {
    if (status === 'UnderReview') return 'Under Review';
    return status;
  }

  selectedPolicy(): PolicyResponse | undefined {
    return this.policies.find((p) => p.id === this.selectedPolicyId);
  }

  isStepReached(currentStatus: string, step: string): boolean {
    const order = ['Reported', 'UnderReview', 'Resolved'];
    return order.indexOf(currentStatus) >= order.indexOf(step);
  }

  stepIcon(step: string): string {
    if (step === 'Reported') return 'pi pi-flag';
    if (step === 'UnderReview') return 'pi pi-search';
    return 'pi pi-check';
  }

  private emptyForm() {
    return { incidentDate: null as Date | null, description: '' };
  }
}
