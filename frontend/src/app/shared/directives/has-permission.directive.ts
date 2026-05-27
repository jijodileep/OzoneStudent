import {
  Directive,
  Input,
  TemplateRef,
  ViewContainerRef,
  effect,
  inject,
} from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { PermissionService } from '../../core/services/permission.service';

@Directive({
  selector: '[appHasPermission]',
  standalone: true,
})
export class HasPermissionDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly permissionService = inject(PermissionService);
  private readonly auth = inject(AuthService);

  private permissionCode = '';

  @Input({ alias: 'appHasPermission', required: true })
  set appHasPermission(code: string) {
    this.permissionCode = code;
    this.render();
  }

  constructor() {
    effect(() => {
      this.auth.currentUser();
      this.render();
    });
  }

  private render(): void {
    this.viewContainer.clear();

    if (!this.permissionCode || this.permissionService.hasPermission(this.permissionCode)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }
}
