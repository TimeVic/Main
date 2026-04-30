export class TimevicApiError extends Error {
  readonly statusCode: number;
  readonly details: string;

  constructor(message: string, statusCode: number, details: string) {
    super(message);
    this.name = 'TimevicApiError';
    this.statusCode = statusCode;
    this.details = details;
  }
}

export function isAuthError(error: unknown): boolean {
  return (
    error instanceof TimevicApiError &&
    (error.statusCode === 401 || error.statusCode === 403)
  );
}

export function formatError(error: unknown): string {
  if (error instanceof TimevicApiError) {
    if (isAuthError(error)) {
      return 'TimeVic connection is invalid. Please reconnect in settings.';
    }
    return `TimeVic API error: ${error.message}`;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'An unexpected error occurred';
}
