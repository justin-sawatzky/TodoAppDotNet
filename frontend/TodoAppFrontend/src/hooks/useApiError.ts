import { useState, useCallback } from 'react';
import { parseApiError, type ParsedError } from '../utils/errorHandling';

export function useApiError() {
  const [error, setError] = useState<ParsedError | null>(null);

  const handleError = useCallback((apiError: unknown, operation?: string) => {
    console.error(`API Error${operation ? ` during ${operation}` : ''}:`, apiError);
    const parsedError = parseApiError(apiError);
    setError(parsedError);
    return parsedError;
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const handleApiCall = useCallback(
    async <T>(
      apiCall: () => Promise<{ data?: T; error?: unknown }>,
      operation?: string
    ): Promise<{ data?: T; error?: ParsedError }> => {
      try {
        const result = await apiCall();

        if (result.error) {
          const parsedError = handleError(result.error, operation);
          return { error: parsedError };
        }

        // Clear any previous errors on success
        clearError();
        return { data: result.data };
      } catch (err) {
        // This catch block handles cases where the API call itself throws
        // (which shouldn't happen with openapi-fetch, but just in case)
        const parsedError = handleError(err, operation);
        return { error: parsedError };
      }
    },
    [handleError, clearError]
  );

  return {
    error,
    handleError,
    clearError,
    handleApiCall,
  };
}
