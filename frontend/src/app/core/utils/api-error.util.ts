import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  code: string;
  message: string;
  field?: string | null;
  propertyName?: string | null;
}

export interface ApiErrorResponse {
  errors: ApiError[];
}

export function extractApiErrors(error: unknown): ApiError[] {
  if (!(error instanceof HttpErrorResponse)) {
    return [];
  }

  const body = error.error;
  if (!body) {
    return [];
  }

  if (Array.isArray(body)) {
    return body.filter(isApiError);
  }

  if (typeof body === 'object' && Array.isArray((body as ApiErrorResponse).errors)) {
    return (body as ApiErrorResponse).errors.filter(isApiError);
  }

  return [];
}

export function toErrorTranslationKeys(errors: ApiError[]): string[] {
  const keys = errors
    .map((item) => item.code?.trim())
    .filter((code): code is string => !!code)
    .map((code) => `errors.${code}`);

  return keys.length > 0 ? [...new Set(keys)] : ['errors.unknown'];
}

function isApiError(value: unknown): value is ApiError {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof (value as ApiError).code === 'string'
  );
}
