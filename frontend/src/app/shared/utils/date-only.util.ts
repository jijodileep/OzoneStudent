export const DATE_ONLY_PATTERN = /^\d{4}-\d{2}-\d{2}$/;

/** Parse YYYY-MM-DD as a local calendar date (no UTC shift). */
export function parseDateOnly(value: string | null | undefined): Date | null {
  if (!value?.trim()) {
    return null;
  }

  const match = DATE_ONLY_PATTERN.exec(value.trim());
  if (!match) {
    return null;
  }

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const date = new Date(year, month - 1, day);

  if (
    date.getFullYear() !== year ||
    date.getMonth() !== month - 1 ||
    date.getDate() !== day
  ) {
    return null;
  }

  return date;
}

/** Format a Date as YYYY-MM-DD using local calendar parts. */
export function formatDateOnly(date: Date | null | undefined): string {
  if (!date || Number.isNaN(date.getTime())) {
    return '';
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** Format YYYY-MM-DD for display (e.g. 19 May 2026). */
export function formatDateOnlyDisplay(
  value: string | null | undefined,
  locale = 'en',
): string {
  const date = parseDateOnly(value);
  if (!date) {
    return value?.trim() ?? '—';
  }

  return new Intl.DateTimeFormat(locale, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  }).format(date);
}
