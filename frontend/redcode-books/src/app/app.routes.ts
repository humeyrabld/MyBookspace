import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Books } from './pages/books/books';
import { Quotes } from './pages/quotes/quotes';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', component: Books, canActivate: [authGuard] },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'quotes', component: Quotes, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
