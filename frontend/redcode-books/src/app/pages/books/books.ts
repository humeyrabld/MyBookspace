import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { BooksService, Book } from '../../services/books';
import { Auth } from '../../services/auth';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './books.html',
  styleUrl: './books.scss'
})
export class Books implements OnInit {
  private booksService = inject(BooksService);
  private auth = inject(Auth);
  readonly theme = inject(ThemeService);
  private router = inject(Router);

  books: Book[] = [];
  searchTerm = '';
  loading = true;
  saving = false;
  error = '';
  showForm = false;
  editingId: number | null = null;

  form = {
    title: '',
    author: '',
    publishedDate: ''
  };

  ngOnInit(): void {
    this.loadBooks();
  }

  get filteredBooks(): Book[] {
    const search = this.searchTerm.toLowerCase().trim();

    return this.books.filter(book =>
      book.title.toLowerCase().includes(search) ||
      book.author.toLowerCase().includes(search)
    );
  }

  loadBooks(): void {
    this.loading = true;
    this.error = '';

    this.booksService.getBooks().subscribe({
      next: data => {
        this.books = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Kunde inte hämta böckerna. Kontrollera inloggningen och att API:et körs.';
        this.loading = false;
      }
    });
  }

  openAddForm(): void {
    this.editingId = null;
    this.form = { title: '', author: '', publishedDate: '' };
    this.error = '';
    this.showForm = true;
  }

  openEditForm(book: Book): void {
    this.editingId = book.id;
    this.form = {
      title: book.title,
      author: book.author,
      publishedDate: book.publishedDate.substring(0, 10)
    };
    this.error = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  saveBook(): void {
    if (!this.form.title.trim() ||
        !this.form.author.trim() ||
        !this.form.publishedDate ||
        this.saving) {
      return;
    }

    this.saving = true;
    this.error = '';

    const bookData = {
      title: this.form.title.trim(),
      author: this.form.author.trim(),
      publishedDate: new Date(
        `${this.form.publishedDate}T00:00:00Z`
      ).toISOString()
    };

    const request = this.editingId === null
      ? this.booksService.addBook(bookData)
      : this.booksService.updateBook(this.editingId, {
          id: this.editingId,
          ...bookData
        });

    request.subscribe({
      next: () => {
        this.saving = false;
        this.closeForm();
        this.loadBooks();
      },
      error: () => {
        this.saving = false;
        this.error = 'Det gick inte att spara boken. Försök igen.';
      }
    });
  }

  deleteBook(book: Book): void {
    if (!confirm(`Vill du ta bort "${book.title}"?`)) {
      return;
    }

    this.error = '';

    this.booksService.deleteBook(book.id).subscribe({
      next: () => this.loadBooks(),
      error: () => {
        this.error = 'Det gick inte att ta bort boken.';
      }
    });
  }

  toggleTheme(): void {
    this.theme.toggle();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
