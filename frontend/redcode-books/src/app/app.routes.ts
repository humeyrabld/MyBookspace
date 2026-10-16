
import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Books } from './pages/books/books';
import { Quotes } from './pages/quotes/quotes';

export const routes: Routes = [
  { path: '', component: Books },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'quotes', component: Quotes },
  { path: '**', redirectTo: '' }
];

