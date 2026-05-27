import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CreateAcademicYearModalComponent } from '../../features/institution/create-academic-year-modal/create-academic-year-modal.component';
import { AuthService } from '../../core/services/auth.service';
import { LocaleService } from '../../core/services/locale.service';
import { SidebarNavigationComponent } from '../sidebar-navigation/sidebar-navigation.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarNavigationComponent, TranslateModule, FormsModule, CreateAcademicYearModalComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent implements OnInit {
  private readonly auth = inject(AuthService);
  readonly locale = inject(LocaleService);

  readonly sidebarOpen = signal(true);
  readonly currentUser = this.auth.currentUser;

  ngOnInit(): void {
    if (this.auth.isAuthenticated() && !this.auth.currentUser()) {
      this.auth.loadCurrentUser().subscribe();
    }
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((open) => !open);
  }

  onLanguageChange(code: string): void {
    this.locale.setLanguage(code);
  }

  logout(): void {
    this.auth.logout().subscribe();
  }
}
