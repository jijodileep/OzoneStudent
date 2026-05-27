import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { LocaleService } from '../../core/services/locale.service';
import { PermissionService } from '../../core/services/permission.service';
import { NAV_GROUPS, NavItem } from '../../shared/models/nav-item.model';

@Component({
  selector: 'app-sidebar-navigation',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, TranslateModule, FormsModule],
  templateUrl: './sidebar-navigation.component.html',
  styleUrl: './sidebar-navigation.component.scss',
})
export class SidebarNavigationComponent {
  private readonly permissionService = inject(PermissionService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly locale = inject(LocaleService);

  private readonly expandedGroups = signal<ReadonlySet<string>>(new Set());

  readonly searchQuery = signal('');

  readonly visibleGroups = computed(() =>
    NAV_GROUPS.map((group) => ({
      labelKey: group.labelKey,
      items: group.items.filter((item) => this.isItemVisible(item)),
    })).filter((group) => group.items.length > 0),
  );

  readonly isSearching = computed(() => this.searchQuery().trim().length > 0);

  readonly filteredGroups = computed(() => {
    this.locale.currentLanguage();

    const query = this.searchQuery().trim().toLowerCase();
    const groups = this.visibleGroups();

    if (!query) {
      return groups;
    }

    return groups
      .map((group) => {
        const groupLabel = this.translate.instant(group.labelKey).toLowerCase();
        const groupMatches = groupLabel.includes(query);

        if (groupMatches) {
          return group;
        }

        const items = group.items.filter((item) =>
          this.translate.instant(item.labelKey).toLowerCase().includes(query),
        );

        return items.length > 0 ? { ...group, items } : null;
      })
      .filter((group): group is NonNullable<typeof group> => group !== null);
  });

  constructor() {
    this.syncExpandedToRoute(this.router.url);

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe((event) => {
        this.syncExpandedToRoute(event.urlAfterRedirects);
      });

    effect(() => {
      this.visibleGroups();
      this.syncExpandedToRoute(this.router.url);
    });
  }

  isExpanded(labelKey: string): boolean {
    if (this.isSearching()) {
      return this.filteredGroups().some((group) => group.labelKey === labelKey);
    }

    return this.expandedGroups().has(labelKey);
  }

  toggleGroup(labelKey: string): void {
    if (this.isSearching()) {
      return;
    }

    this.expandedGroups.update((current) => {
      if (current.has(labelKey)) {
        return new Set();
      }

      return new Set([labelKey]);
    });
  }

  clearSearch(): void {
    this.searchQuery.set('');
  }

  private syncExpandedToRoute(url: string): void {
    if (this.isSearching()) {
      return;
    }

    const activeGroup = this.visibleGroups().find((group) =>
      group.items.some((item) => this.isRouteActive(url, item.route)),
    );

    if (activeGroup) {
      this.expandedGroups.set(new Set([activeGroup.labelKey]));
    }
  }

  private isRouteActive(url: string, route: string): boolean {
    return url === route || url.startsWith(`${route}/`);
  }

  private isItemVisible(item: NavItem): boolean {
    if (!item.permission) {
      return true;
    }

    const codes = Array.isArray(item.permission) ? item.permission : [item.permission];
    return this.permissionService.hasAnyPermission(codes);
  }
}
