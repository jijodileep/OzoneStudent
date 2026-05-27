import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { AcademicYearDto } from './institution-api.service';

@Injectable({ providedIn: 'root' })
export class CreateAcademicYearModalService {
  readonly isOpen = signal(false);

  private pending?: Subject<AcademicYearDto | null>;

  /** Open the create dialog. Emits the created year, or null if cancelled. */
  open(): Observable<AcademicYearDto | null> {
    this.isOpen.set(true);
    this.pending = new Subject<AcademicYearDto | null>();
    return this.pending.asObservable();
  }

  close(created: AcademicYearDto | null = null): void {
    this.isOpen.set(false);
    this.pending?.next(created);
    this.pending?.complete();
    this.pending = undefined;
  }
}
