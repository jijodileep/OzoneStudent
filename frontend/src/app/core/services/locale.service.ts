import { Injectable, computed, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Arabic } from 'flatpickr/dist/l10n/ar.js';
import { Hindi } from 'flatpickr/dist/l10n/hi.js';
import type { CustomLocale } from 'flatpickr/dist/types/locale';

export interface AppLanguage {
  code: string;
  label: string;
  rtl: boolean;
}

const STORAGE_KEY = 'ss_lang';

@Injectable({ providedIn: 'root' })
export class LocaleService {
  private readonly translate = inject(TranslateService);

  readonly languages: AppLanguage[] = [
    { code: 'en', label: 'English', rtl: false },
    { code: 'ar', label: 'العربية', rtl: true },
    { code: 'hi', label: 'हिन्दी', rtl: false },
  ];

  readonly currentLanguage = signal(this.languages[0]);
  readonly isRtl = computed(() => this.currentLanguage().rtl);

  initialize(): void {
    const stored = sessionStorage.getItem(STORAGE_KEY);
    const language = this.languages.find((item) => item.code === stored) ?? this.languages[0];
    this.applyLanguage(language.code, false);
  }

  setLanguage(code: string): void {
    this.applyLanguage(code, true);
  }

  flatpickrLocale(): CustomLocale | undefined {
    const code = this.currentLanguage().code;
    if (code === 'ar') {
      return Arabic;
    }

    if (code === 'hi') {
      return Hindi;
    }

    return undefined;
  }

  private applyLanguage(code: string, persist: boolean): void {
    const language = this.languages.find((item) => item.code === code) ?? this.languages[0];
    this.currentLanguage.set(language);
    this.translate.use(language.code);

    const html = document.documentElement;
    html.lang = language.code;
    html.dir = language.rtl ? 'rtl' : 'ltr';
    document.body.classList.toggle('rtl-layout', language.rtl);

    this.toggleBootstrapRtl(language.rtl);

    if (persist) {
      sessionStorage.setItem(STORAGE_KEY, language.code);
    }
  }

  private toggleBootstrapRtl(enabled: boolean): void {
    const link = document.getElementById('bootstrap-rtl') as HTMLLinkElement | null;
    if (link) {
      link.disabled = !enabled;
    }
  }
}
