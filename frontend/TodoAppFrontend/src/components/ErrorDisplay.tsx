import { type ParsedError, getValidationHints } from '../utils/errorHandling';
import './ErrorDisplay.css';

interface ErrorDisplayProps {
  error: ParsedError | null;
  onDismiss?: () => void;
  className?: string;
}

export function ErrorDisplay({ error, onDismiss, className = '' }: ErrorDisplayProps) {
  if (!error) return null;

  const getErrorIcon = (type: ParsedError['type']) => {
    switch (type) {
      case 'validation':
        return '⚠️';
      case 'not_found':
        return '🔍';
      case 'conflict':
        return '⚡';
      case 'network':
        return '🌐';
      default:
        return '❌';
    }
  };

  const getErrorClass = (type: ParsedError['type']) => {
    switch (type) {
      case 'validation':
        return 'error-validation';
      case 'not_found':
        return 'error-not-found';
      case 'conflict':
        return 'error-conflict';
      case 'network':
        return 'error-network';
      default:
        return 'error-server';
    }
  };

  return (
    <div className={`error-display ${getErrorClass(error.type)} ${className}`}>
      <div className="error-content">
        <span className="error-icon">{getErrorIcon(error.type)}</span>
        <div className="error-details">
          <div className="error-message">{error.message}</div>
          
          {error.validationErrors && error.validationErrors.length > 0 && (
            <div className="validation-details">
              {error.validationErrors.map((validationError, index) => (
                <div key={index} className="validation-error">
                  <div className="validation-message">{validationError.message}</div>
                  {validationError.field && (
                    <div className="validation-hint">
                      {getValidationHints(validationError.field)}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
        
        {onDismiss && (
          <button 
            className="error-dismiss" 
            onClick={onDismiss}
            aria-label="Dismiss error"
          >
            ×
          </button>
        )}
      </div>
    </div>
  );
}