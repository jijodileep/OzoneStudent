import {
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  afterNextRender,
  effect,
  forwardRef,
  inject,
  viewChild,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import flatpickr from 'flatpickr';
import type { Instance as FlatpickrInstance } from 'flatpickr/dist/types/instance';
import { LocaleService } from '../../../core/services/locale.service';
import { formatDateOnly, parseDateOnly } from '../../utils/date-only.util';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatePickerComponent),
      multi: true,
    },
  ],
})
export class DatePickerComponent implements ControlValueAccessor, OnChanges, OnDestroy {
  private readonly localeService = inject(LocaleService);
  private readonly hostRef = viewChild.required<ElementRef<HTMLElement>>('host');
  private readonly inputRef = viewChild.required<ElementRef<HTMLInputElement>>('input');

  @Input() inputId = '';
  @Input() placeholder = '';
  @Input() invalid = false;
  @Input() minDate: string | null = null;
  @Input() maxDate: string | null = null;
  /** Inline calendar (recommended inside modals). */
  @Input() inline = false;

  private picker?: FlatpickrInstance;
  private pendingValue: string | null = null;
  private localeCode = '';
  private initialized = false;
  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;
  disabled = false;

  constructor() {
    afterNextRender(() => this.initPicker(this.pendingValue));

    effect(() => {
      const code = this.localeService.currentLanguage().code;
      if (!this.picker) {
        this.localeCode = code;
        return;
      }

      if (code !== this.localeCode) {
        this.localeCode = code;
        this.picker.set('locale', this.localeService.flatpickrLocale() ?? 'default');
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.picker) {
      return;
    }

    if (changes['minDate'] || changes['maxDate']) {
      this.applyBounds();
    }

    if (changes['inline'] && !changes['inline'].firstChange) {
      this.rebuildPicker(this.pendingValue);
    }
  }

  ngOnDestroy(): void {
    this.destroyPicker();
  }

  writeValue(value: string | null): void {
    this.pendingValue = value?.trim() ? value.trim() : null;
    if (this.picker) {
      this.applyValue(this.pendingValue);
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (!this.picker) {
      return;
    }

    this.picker.set('clickOpens', !isDisabled);
    this.picker.input.disabled = isDisabled;
  }

  open(): void {
    if (!this.disabled && !this.inline) {
      this.picker?.open();
    }
  }

  private initPicker(initialValue: string | null): void {
    if (this.initialized) {
      return;
    }

    this.initialized = true;
    const input = this.inputRef().nativeElement;
    this.localeCode = this.localeService.currentLanguage().code;

    this.picker = flatpickr(input, {
      dateFormat: 'Y-m-d',
      locale: this.localeService.flatpickrLocale(),
      allowInput: false,
      clickOpens: !this.inline,
      disableMobile: true,
      static: this.inline,
      appendTo: this.inline ? this.hostRef().nativeElement : document.body,
      position: 'auto',
      onChange: (selectedDates: Date[]) => {
        const nextValue = formatDateOnly(selectedDates[0] ?? null);
        this.pendingValue = nextValue || null;
        this.onChange(nextValue);
        this.onTouched();
      },
      onClose: () => this.onTouched(),
      onOpen: (_dates, _str, instance) => {
        instance.calendarContainer.style.zIndex = '1200';
      },
    });

    this.applyBounds();
    this.applyValue(initialValue);

    if (this.disabled) {
      this.setDisabledState(true);
    }
  }

  private rebuildPicker(value: string | null): void {
    this.destroyPicker();
    this.initPicker(value);
  }

  private destroyPicker(): void {
    this.picker?.destroy();
    this.picker = undefined;
    this.initialized = false;
  }

  private applyValue(value: string | null): void {
    if (!this.picker) {
      return;
    }

    const parsed = parseDateOnly(value);
    if (parsed) {
      this.picker.setDate(parsed, false);
      return;
    }

    this.picker.clear(false);
  }

  private applyBounds(): void {
    if (!this.picker) {
      return;
    }

    this.picker.set('minDate', parseDateOnly(this.minDate) ?? undefined);
    this.picker.set('maxDate', parseDateOnly(this.maxDate) ?? undefined);
  }
}
