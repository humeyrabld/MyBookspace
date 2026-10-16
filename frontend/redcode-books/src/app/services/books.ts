
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Book {
  id: number;
  title: string;
  author: string;
  publishedDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class BooksService {
  private http = inject(HttpClient);
  private apiUrl = 'https://mybookspace.onrender.com/api/books';

  getBooks() {
    return this.http.get<Book[]>(this.apiUrl);
  }

  addBook(book: Omit<Book, 'id'>) {
    return this.http.post<Book>(this.apiUrl, book);
  }

  updateBook(id: number, book: Book) {
    return this.http.put(`${this.apiUrl}/${id}`, book);
  }

  deleteBook(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}

