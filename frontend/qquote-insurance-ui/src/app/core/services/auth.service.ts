import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';

export interface LoginRequest  { email: string; password: string; }
export interface LoginResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
    login(credentials: LoginRequest): Observable<LoginResponse> {
        if (credentials.email && credentials.password) {
            return of({ token: 'mock-jwt-token' });
        }
        return throwError(() => new Error('Invalid credentials'));
    }

    logout(): void {
        localStorage.removeItem('token');
    }

    isAuthenticated(): boolean {
        return !!localStorage.getItem('token');
    }
}
