import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private lightMode = false;

  constructor() {
    this.lightMode =
      localStorage.getItem('bookspace_theme') === 'light';

    this.applyTheme();
  }

  isLight(): boolean {
    return this.lightMode;
  }

  toggle(): void {
    this.lightMode = !this.lightMode;
    this.applyTheme();
  }

  private applyTheme(): void {
    const theme = this.lightMode ? 'light' : 'dark';

    document.documentElement.setAttribute(
      'data-theme',
      theme
    );

    localStorage.setItem('bookspace_theme', theme);
  }
}
