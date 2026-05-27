import { Directive, Input, TemplateRef } from '@angular/core';

@Directive({
  selector: '[appDataTableCell]',
  standalone: true,
})
export class DataTableCellDirective {
  @Input({ alias: 'appDataTableCell', required: true })
  columnId!: string;

  constructor(public readonly template: TemplateRef<{ row: unknown }>) {}
}
