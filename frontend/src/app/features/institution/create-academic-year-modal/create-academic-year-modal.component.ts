import { Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AppModalComponent } from '../../../shared/components/modal/app-modal.component';
import { DatePickerComponent } from '../../../shared/components/date-picker/date-picker.component';
import { extractApiErrors, toErrorTranslationKeys } from '../../../core/utils/api-error.util';
import {
  AcademicYearDto,
  InstitutionApiService,
} from '../services/institution-api.service';
import { CreateAcademicYearModalService } from '../services/create-academic-year-modal.service';

const NAME_MAX_LENGTH = 100;

@Component({
  selector: 'app-create-academic-year-modal',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule, AppModalComponent, DatePickerComponent],
  templateUrl: './create-academic-year-modal.component.html',
})
export class CreateAcademicYearModalComponent {
  private readonly api = inject(InstitutionApiService);
  private readonly fb = inject(FormBuilder);
  readonly modal = inject(CreateAcademicYearModalService);

  readonly nameMaxLength = NAME_MAX_LENGTH;
  readonly saving = signal(false);
  readonly errorKeys = signal<string[]>([]);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(NAME_MAX_LENGTH)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    setAsCurrent: [true],
  });

  constructor() {
    effect(() => {
      if (this.modal.isOpen()) {
        this.resetForm();
      }
    });
  }

  dismiss(): void {
    if (!this.saving()) {
      this.modal.close(null);
    }
  }

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorKeys.set([]);

    this.api.createAcademicYear(this.form.getRawValue()).subscribe({
      next: (created: AcademicYearDto) => {
        this.saving.set(false);
        this.modal.close(created);
      },
      error: (err) => {
        this.saving.set(false);
        this.applyApiErrors(err);
      },
    });
  }

  nameErrorKey(): string | null {
    const control = this.form.controls.name;
    if (!control.touched || !control.errors) {
      return null;
    }

    if (control.errors['required']) {
      return 'errors.institution.academic_year.name_required';
    }

    if (control.errors['maxlength']) {
      return 'errors.institution.academic_year.name_too_long';
    }

    return null;
  }

  dateErrorKey(controlName: 'startDate' | 'endDate'): string | null {
    const control = this.form.controls[controlName];
    if (!control.touched || !control.errors?.['required']) {
      return null;
    }

    return controlName === 'startDate'
      ? 'errors.institution.academic_year.start_date_required'
      : 'errors.institution.academic_year.end_date_required';
  }

  private applyApiErrors(err: unknown): void {
    const errors = extractApiErrors(err);
    this.errorKeys.set(toErrorTranslationKeys(errors));

    for (const error of errors) {
      const field = error.field?.toLowerCase();
      if (field === 'name') {
        this.form.controls.name.setErrors({ api: true });
        this.form.controls.name.markAsTouched();
      } else if (field === 'startdate') {
        this.form.controls.startDate.setErrors({ api: true });
        this.form.controls.startDate.markAsTouched();
      } else if (field === 'enddate') {
        this.form.controls.endDate.setErrors({ api: true });
        this.form.controls.endDate.markAsTouched();
      }
    }
  }

  private resetForm(): void {
    this.form.reset({
      name: '',
      startDate: '',
      endDate: '',
      setAsCurrent: true,
    });
    this.errorKeys.set([]);
    this.saving.set(false);
  }
}
