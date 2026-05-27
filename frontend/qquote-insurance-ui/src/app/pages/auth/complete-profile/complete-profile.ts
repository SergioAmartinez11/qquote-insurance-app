import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { AppFloatingConfigurator } from '../../../layout/component/app.floatingconfigurator';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [ButtonModule, InputTextModule, InputNumberModule, FormsModule, AppFloatingConfigurator],
  templateUrl: './complete-profile.component.html',
})
export class CompleteProfile {
  age: number | null = null;
  zipCode: string = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService,
  ) {}

  onSubmit(): void {
    if (!this.age || !this.zipCode) {
      this.toast.warn('Both fields are required.');
      return;
    }

    this.loading = true;
    this.authService.completeProfile({ age: this.age, zipCode: this.zipCode }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.toast.fromHttpError(err, 'Could not save your profile. Please try again.');
      },
    });
  }
}
