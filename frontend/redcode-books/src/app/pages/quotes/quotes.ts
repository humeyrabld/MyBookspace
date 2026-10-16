
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { QuotesService, Quote } from '../../services/quotes';
import { Auth } from '../../services/auth';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-quotes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './quotes.html',
  styleUrl: './quotes.scss'
})
export class Quotes implements OnInit {
  private quotesService = inject(QuotesService);
  private auth = inject(Auth);
  private router = inject(Router);
  theme = inject(ThemeService);

  quotes: Quote[] = [];
  loading = true;
  saving = false;
  error = '';
  showForm = false;
  editingId: number | null = null;

  form = {
    text: '',
    author: ''
  };

  ngOnInit(): void {
    this.loadQuotes();
  }

  loadQuotes(): void {
    this.loading = true;
    this.error = '';

    this.quotesService.getQuotes().subscribe({
      next: data => {
        this.quotes = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Kunde inte hämta citaten. Kontrollera att API:et körs och att du är inloggad.';
        this.loading = false;
      }
    });
  }

  openAddForm(): void {
    this.editingId = null;
    this.form = { text: '', author: '' };
    this.error = '';
    this.showForm = true;
  }

  openEditForm(quote: Quote): void {
    this.editingId = quote.id;
    this.form = {
      text: quote.text,
      author: quote.author
    };
    this.error = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  saveQuote(): void {
    if (!this.form.text.trim() || !this.form.author.trim() || this.saving) {
      return;
    }

    this.saving = true;
    this.error = '';

    const quoteData = {
      text: this.form.text.trim(),
      author: this.form.author.trim()
    };

    const request = this.editingId === null
      ? this.quotesService.addQuote(quoteData)
      : this.quotesService.updateQuote(this.editingId, {
          id: this.editingId,
          ...quoteData
        });

    request.subscribe({
      next: () => {
        this.saving = false;
        this.closeForm();
        this.loadQuotes();
      },
      error: () => {
        this.saving = false;
        this.error = 'Det gick inte att spara citatet.';
      }
    });
  }

  deleteQuote(quote: Quote): void {
    if (!confirm('Vill du ta bort det här citatet?')) {
      return;
    }

    this.quotesService.deleteQuote(quote.id).subscribe({
      next: () => this.loadQuotes(),
      error: () => {
        this.error = 'Det gick inte att ta bort citatet.';
      }
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
