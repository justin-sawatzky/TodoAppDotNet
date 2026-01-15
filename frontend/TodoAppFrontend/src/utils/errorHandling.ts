// Utility functions for handling API errors and validation

export interface ValidationError {
  field?: string;
  message: string;
}

export interface ParsedError {
  type: 'validation' | 'not_found' | 'conflict' | 'server' | 'network';
  message: string;
  validationErrors?: ValidationError[];
}

/**
 * Parse error response from the API and extract validation details
 */
export function parseApiError(error: unknown): ParsedError {
  // Handle network errors (no response at all)
  if (!error) {
    return {
      type: 'network',
      message: 'Network error. Please check your connection and try again.',
    };
  }

  // openapi-fetch returns errors in the format: { message: "..." }
  // Just extract the message and pass it through to the user
  if (typeof error === 'object' && error !== null && 'message' in error) {
    const message = String(error.message);

    // Determine error type based on HTTP status or message content for styling purposes
    let type: ParsedError['type'] = 'server';

    if (message.includes('Validation failed:')) {
      type = 'validation';
    } else if (message.toLowerCase().includes('not found')) {
      type = 'not_found';
    } else if (
      message.toLowerCase().includes('already exists') ||
      message.toLowerCase().includes('conflict')
    ) {
      type = 'conflict';
    } else if (message.toLowerCase().includes('network')) {
      type = 'network';
    }

    return {
      type,
      message,
      // Only parse validation errors for better display
      validationErrors: type === 'validation' ? parseValidationMessage(message) : undefined,
    };
  }

  // Fallback for unknown error formats
  return {
    type: 'server',
    message: 'An unexpected error occurred. Please try again.',
  };
}

/**
 * Parse validation message and extract individual field errors
 */
function parseValidationMessage(message: string): ValidationError[] {
  if (!message.includes('Validation failed:')) {
    return [{ message }];
  }

  // Extract the part after "Validation failed: "
  const errorsPart = message.replace('Validation failed: ', '');

  // Split by comma and clean up each error
  const errors = errorsPart.split(', ').map((error) => error.trim());

  return errors.map((error) => {
    // Try to extract field name from common patterns
    const fieldMatch = error.match(/The field (\w+)/i) || error.match(/The (\w+) field/i);
    const field = fieldMatch ? fieldMatch[1].toLowerCase() : undefined;

    return {
      field,
      message: error,
    };
  });
}

/**
 * Get user-friendly validation hints based on field names
 */
export function getValidationHints(field: string): string {
  const hints: Record<string, string> = {
    username:
      'Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens.',
    email: 'Please enter a valid email address (5-254 characters).',
    name: 'List name must be 1-100 characters long.',
    description: 'Description can be up to 500 characters for lists or 1000 characters for tasks.',
    order: 'Order must be a number between 0 and 999,999.',
  };

  return hints[field.toLowerCase()] || '';
}

/**
 * Format validation errors for display
 */
export function formatValidationErrors(errors: ValidationError[]): string {
  if (errors.length === 1) {
    return errors[0].message;
  }

  return errors.map((error, index) => `${index + 1}. ${error.message}`).join('\n');
}
