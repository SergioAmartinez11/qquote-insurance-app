import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { DividerModule } from 'primeng/divider';
import { AppFloatingConfigurator } from '../../../layout/component/app.floatingconfigurator';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    PasswordModule,
    FormsModule,
    RouterModule,
    RippleModule,
    DividerModule,
    AppFloatingConfigurator,
  ],
  templateUrl: './register.component.html',
})
export class Register {
  fullName: string = '';
  email: string = '';
  password: string = '';
  age: number | null = null;
  zipCode: string = '';
  loading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService,
  ) {}

  onRegister(): void {
    if (!this.fullName || !this.email || !this.password || !this.age || !this.zipCode) {
      this.toast.warn('All fields are required.');
      return;
    }

    this.loading = true;

    this.authService
      .register({
        fullName: this.fullName,
        email: this.email,
        password: this.password,
        age: this.age,
        zipCode: this.zipCode,
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.toast.success('Your account has been created.', 'Registered');
          this.router.navigate(['/']);
        },
        error: (err) => {
          this.loading = false;
          this.toast.fromHttpError(err, 'Registration failed. Please try again.');
        },
      });
  }
}
