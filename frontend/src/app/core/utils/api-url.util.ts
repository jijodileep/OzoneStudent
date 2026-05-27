export function getApiPath(url: string): string {
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return new URL(url).pathname;
  }

  return url;
}

export function isApiRequest(url: string, apiBaseUrl: string): boolean {
  return getApiPath(url).startsWith(apiBaseUrl);
}
