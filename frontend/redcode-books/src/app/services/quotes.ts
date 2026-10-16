
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Quote {
  id: number;
  text: string;
  author: string;
}

@Injectable({
  providedIn: 'root'
})
export class QuotesService {
  private http = inject(HttpClient);
  private apiUrl = 'https://mybookspace.onrender.com/api/quotes';

  getQuotes() {
    return this.http.get<Quote[]>(this.apiUrl);
  }

  addQuote(quote: Omit<Quote, 'id'>) {
    return this.http.post<Quote>(this.apiUrl, quote);
  }

  updateQuote(id: number, quote: Quote) {
    return this.http.put(`${this.apiUrl}/${id}`, quote);
  }

  deleteQuote(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}

