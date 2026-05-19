import { AfterViewInit, Component, ElementRef, NgZone, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { DividerModule } from 'primeng/divider';
import { AppFloatingConfigurator } from '../../../layout/component/app.floatingconfigurator';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ButtonModule,
    CheckboxModule,
    InputTextModule,
    PasswordModule,
    FormsModule,
    RouterModule,
    RippleModule,
    DividerModule,
    AppFloatingConfigurator,
  ],
  templateUrl: './login.component.html',
})
export class Login implements AfterViewInit {
  @ViewChild('googleBtnContainer') googleBtnContainer!: ElementRef<HTMLDivElement>;

  email = '';
  password = '';
  checked = false;
  loading = false;
  googleLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService,
    private zone: NgZone,
  ) {}

  ngAfterViewInit(): void {
    this.initGoogleSignIn(0);
  }

  private initGoogleSignIn(attempt: number): void {
    const g = (window as any).google;
    if (!g?.accounts?.id) {
      if (attempt < 20) {
        setTimeout(() => this.initGoogleSignIn(attempt + 1), 150);
      }
      return;
    }

    g.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (response: { credential: string }) => {
        this.zone.run(() => this.handleGoogleCredential(response.credential));
      },
    });

    g.accounts.id.renderButton(this.googleBtnContainer.nativeElement, {
      type: 'standard',
      theme: 'outline',
      size: 'large',
      text: 'signin_with',
      logo_alignment: 'left',
      width: this.googleBtnContainer.nativeElement.offsetWidth || 340,
    });
  }

  private handleGoogleCredential(credential: string): void {
    this.googleLoading = true;
    this.authService.googleSignIn(credential).subscribe({
      next: () => {
        this.googleLoading = false;
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.googleLoading = false;
        this.toast.fromHttpError(err, 'Google sign-in failed. Please try again.');
      },
    });
  }

  onLogin(): void {
    if (!this.email || !this.password) {
      this.toast.warn('Email and password are required.');
      return;
    }

    this.loading = true;

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        this.toast.success('Welcome back!', 'Signed in');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.toast.fromHttpError(err, 'Invalid credentials. Please try again.');
      },
    });
  }
}
