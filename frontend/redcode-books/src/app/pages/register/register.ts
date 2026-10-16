
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private auth = inject(Auth);
  private router = inject(Router);

  email = '';
  password = '';
  confirmPassword = '';

  loading = false;
  error = '';
  success = '';

  register(): void {
    this.error = '';
    this.success = '';

    if (!this.email.trim() || !this.password) {
      this.error = 'Fyll i e-post och lösenord.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.error = 'Lösenorden matchar inte.';
      return;
    }

    if (this.password.length < 6) {
      this.error = 'Lösenordet måste vara minst 6 tecken.';
      return;
    }

    this.loading = true;

    this.auth.register(this.email.trim(), this.password).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Ditt konto har skapats! Du skickas till inloggningen.';

        setTimeout(() => {
          this.router.navigateByUrl('/login');
        }, 1200);
      },
      error: (err) => {
        this.loading = false;

        if (err.status === 400) {
          this.error = 'Kontrollera uppgifterna. E-postadressen kan redan vara registrerad eller lösenordet uppfyller inte kraven.';
        } else {
          this.error = 'Det gick inte att skapa kontot. Försök igen.';
        }
      }
    });
  }
}

