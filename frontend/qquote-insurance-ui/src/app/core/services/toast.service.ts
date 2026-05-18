import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({ providedIn: 'root' })
export class ToastService {
  constructor(private messageService: MessageService) {}

  success(detail: string, summary = 'Success') {
    this.messageService.add({ severity: 'success', summary, detail, life: 4000 });
  }

  error(detail: string, summary = 'Error') {
    this.messageService.add({ severity: 'error', summary, detail, life: 5000 });
  }

  warn(detail: string, summary = 'Warning') {
    this.messageService.add({ severity: 'warn', summary, detail, life: 4000 });
  }

  info(detail: string, summary = 'Info') {
    this.messageService.add({ severity: 'info', summary, detail, life: 4000 });
  }

  fromHttpError(err: unknown, fallback = 'An unexpected error occurred.') {
    const detail = (err as any)?.error?.message ?? fallback;
    this.error(detail);
  }
}
