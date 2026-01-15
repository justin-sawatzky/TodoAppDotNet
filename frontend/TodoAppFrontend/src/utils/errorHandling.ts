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
export function parseApiError(error: any): ParsedError {
  // Handle network errors (no response at all)
  if (!error) {
    return {
      type: 'network',
      message: 'Network error. Please check your connection and try again.'
    };
  }

  // Check if this is a simple error object with just a message
  if (error.message && !error.response && !error.error && !error.status) {
    // This appears to be the actual format we're getting
    const message = error.message;
    
    // Check if it's a validation error based on the message content
    if (message.includes('Validation failed:')) {
      return {
        type: 'validation',
        message: message,
        validationErrors: parseValidationMessage(message)
      };
    }
    
    // Check for other error types based on message content
    if (message.toLowerCase().includes('not found')) {
      return {
        type: 'not_found',
        message: message
      };
    }
    
    if (message.toLowerCase().includes('already exists') || message.toLowerCase().includes('conflict')) {
      return {
        type: 'conflict',
        message: message
      };
    }
    
    // Default to server error for unknown message-only errors
    return {
      type: 'server',
      message: message
    };
  }

  // openapi-fetch returns errors in the format: { error: {...}, response: Response }
  let status: number | undefined;
  let responseData: any;

  if (error.response && error.error) {
    // openapi-fetch format: { error: { message: "..." }, response: Response }
    status = error.response.status;
    responseData = error.error;
  } else if (error.error && error.error.status) {
    // Alternative format: { error: { status: 400, data: {...} } }
    status = error.error.status;
    responseData = error.error.data;
  } else if (error.status) {
    // Direct format: { status: 400, data: {...} }
    status = error.status;
    responseData = error.data;
  } else if (error.response) {
    // Format: { response: { status: 400, data: {...} } }
    status = error.response.status;
    responseData = error.response.data;
  } else {
    // Unknown error format, treat as network error
    return {
      type: 'network',
      message: 'Network error. Please check your connection and try again.'
    };
  }

  if (!status) {
    return {
      type: 'network',
      message: 'Network error. Please check your connection and try again.'
    };
  }

  switch (status) {
    case 400:
      // Validation error
      const validationMessage = responseData?.message || responseData?.Message || 'Validation failed';
      return {
        type: 'validation',
        message: validationMessage,
        validationErrors: parseValidationMessage(validationMessage)
      };

    case 404:
      return {
        type: 'not_found',
        message: responseData?.message || responseData?.Message || 'Resource not found'
      };

    case 409:
      return {
        type: 'conflict',
        message: responseData?.message || responseData?.Message || 'Resource already exists'
      };

    case 500:
    case 502:
    case 503:
      return {
        type: 'server',
        message: 'Server error. Please try again later.'
      };

    default:
      return {
        type: 'server',
        message: responseData?.message || responseData?.Message || 'An unexpected error occurred'
      };
  }
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
  const errors = errorsPart.split(', ').map(error => error.trim());
  
  return errors.map(error => {
    // Try to extract field name from common patterns
    const fieldMatch = error.match(/The field (\w+)/i) || error.match(/The (\w+) field/i);
    const field = fieldMatch ? fieldMatch[1].toLowerCase() : undefined;
    
    return {
      field,
      message: error
    };
  });
}

/**
 * Get user-friendly validation hints based on field names
 */
export function getValidationHints(field: string): string {
  const hints: Record<string, string> = {
    username: 'Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens.',
    email: 'Please enter a valid email address (5-254 characters).',
    name: 'List name must be 1-100 characters long.',
    description: 'Description can be up to 500 characters for lists or 1000 characters for tasks.',
    order: 'Order must be a number between 0 and 999,999.'
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