import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-placeholder-page',
  standalone: true,
  imports: [TranslateModule],
  template: `
    <div class="page-header mb-4">
      <h1 class="h3 mb-0">{{ titleKey | translate }}</h1>
    </div>
    <div class="card border-0 shadow-sm">
      <div class="card-body text-muted">{{ 'common.comingSoon' | translate }}</div>
    </div>
  `,
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly titleKey = (this.route.snapshot.data['titleKey'] as string) ?? 'common.comingSoon';
}
