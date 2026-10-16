import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, finalize, map, Observable, of, shareReplay, tap } from 'rxjs';

interface LoginResponse { token: string; email: string; }

@Injectable({ providedIn: 'root' })
export class Auth {
  private http = inject(HttpClient);
  private apiUrl = 'https://mybookspace.onrender.com/api/auth';
  private refreshInProgress$: Observable<string | null> | null = null;

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }, { withCredentials: true })
      .pipe(tap(response => this.saveLogin(response)));
  }

  register(email: string, password: string) {
    return this.http.post(`${this.apiUrl}/register`, { email, password });
  }

  refreshToken(): Observable<string | null> {
    if (!this.refreshInProgress$) {
      this.refreshInProgress$ = this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, {}, { withCredentials: true }).pipe(
        tap(response => this.saveLogin(response)),
        map(response => response.token),
        catchError(() => { this.clearLogin(); return of(null); }),
        finalize(() => { this.refreshInProgress$ = null; }),
        shareReplay(1)
      );
    }
    return this.refreshInProgress$;
  }

  getToken(): string | null { return localStorage.getItem('bookspace_token'); }
  isLoggedIn(): boolean { return !!this.getToken(); }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      next: () => this.clearLogin(),
      error: () => this.clearLogin()
    });
    // Clear local credentials immediately so the interface logs out at once.
    this.clearLogin();
  }

  private saveLogin(response: LoginResponse): void {
    localStorage.setItem('bookspace_token', response.token);
    localStorage.setItem('bookspace_email', response.email);
  }
  private clearLogin(): void {
    localStorage.removeItem('bookspace_token');
    localStorage.removeItem('bookspace_email');
  }
}
