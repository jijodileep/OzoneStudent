import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EnvironmentConfigService {
  readonly apiBaseUrl = environment.apiBaseUrl;
  readonly defaultTenantSlug = environment.defaultTenantSlug;
  readonly production = environment.production;
}
