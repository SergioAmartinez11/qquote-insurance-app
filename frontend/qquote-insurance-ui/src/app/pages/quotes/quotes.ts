import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { PolicyService } from '../../core/services/policy.service';
import { CreateQuoteRequest, QuoteResponse, QuoteService } from '../../core/services/quote.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-quotes',
  standalone: true,
  imports: [
    FormsModule,
    RouterModule,
    ButtonModule,
    TableModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TagModule,
    DividerModule,
    DecimalPipe,
    DatePipe,
  ],
  templateUrl: './quotes.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Quotes implements OnInit {
  private quoteService = inject(QuoteService);
  private policyService = inject(PolicyService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  quotes: QuoteResponse[] = [];
  loading = false;
  creating = false;
  convertingIds = new Set<string>();

  showCreateDialog = false;
  showDetailDialog = false;
  selectedQuote: QuoteResponse | null = null;

  form: CreateQuoteRequest = this.emptyForm();

  coverageOptions = [
    { label: 'Basic — $50 base rate', value: 'Basic' },
    { label: 'Comprehensive — $120 base rate', value: 'Comprehensive' },
    { label: 'Limited — $30 base rate', value: 'Limited' },
  ];

  currencyOptions = [
    { label: 'USD — US Dollar', value: 'USD' },
    { label: 'EUR — Euro', value: 'EUR' },
    { label: 'MXN — Mexican Peso', value: 'MXN' },
    { label: 'GBP — British Pound', value: 'GBP' },
  ];

  ngOnInit(): void {
    this.loadQuotes();
  }

  loadQuotes(): void {
    this.loading = true;
    this.quoteService.getMyQuotes().subscribe({
      next: (quotes) => {
        this.quotes = quotes;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loading = false;
        this.toast.fromHttpError(err, 'Failed to load your quotes.');
        this.cdr.markForCheck();
      },
    });
  }

  openCreateDialog(): void {
    this.form = this.emptyForm();
    this.showCreateDialog = true;
  }

  submitCreate(): void {
    if (!this.form.vehicleMake.trim() || !this.form.vehicleModel.trim() || !this.form.vehicleYear) {
      this.toast.warn('Vehicle make, model, and year are required.');
      return;
    }

    this.creating = true;
    this.quoteService.create(this.form).subscribe({
      next: (quote) => {
        this.quotes = [quote, ...this.quotes];
        this.creating = false;
        this.showCreateDialog = false;
        this.toast.success(
          `${quote.currency} ${quote.monthlyPremium.toFixed(2)}/mo — Risk: ${quote.riskLevel}`,
          'Quote Created',
        );
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.creating = false;
        this.toast.fromHttpError(err, 'Failed to create quote.');
        this.cdr.markForCheck();
      },
    });
  }

  convertToPolicy(quote: QuoteResponse): void {
    this.convertingIds.add(quote.id);
    this.cdr.markForCheck();

    this.policyService.convertFromQuote(quote.id).subscribe({
      next: () => {
        this.convertingIds.delete(quote.id);
        quote.status = 'Converted';
        this.toast.success('Your quote has been converted to a policy.', 'Policy Created');
        this.cdr.markForCheck();
        this.router.navigate(['/policies']);
      },
      error: (err) => {
        this.convertingIds.delete(quote.id);
        this.toast.fromHttpError(err, 'Failed to convert quote to policy.');
        this.cdr.markForCheck();
      },
    });
  }

  viewDetail(quote: QuoteResponse): void {
    this.selectedQuote = quote;
    this.showDetailDialog = true;
  }

  riskSeverity(level: string): 'success' | 'warn' | 'danger' | 'info' {
    if (level === 'Low') return 'success';
    if (level === 'High') return 'danger';
    return 'warn';
  }

  statusSeverity(status: string): 'success' | 'secondary' | 'info' {
    if (status === 'Active') return 'success';
    if (status === 'Expired') return 'secondary';
    return 'info';
  }

  private emptyForm(): CreateQuoteRequest {
    return {
      vehicleMake: '',
      vehicleModel: '',
      vehicleYear: new Date().getFullYear(),
      coverageType: 'Basic',
      currency: 'USD',
    };
  }
}
