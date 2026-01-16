import { useEffect, useRef } from 'react';
import './ConfirmModal.css';

interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
  variant?: 'danger' | 'default';
}

export function ConfirmModal({
  isOpen,
  title,
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  onConfirm,
  onCancel,
  variant = 'default',
}: ConfirmModalProps) {
  const confirmButtonRef = useRef<HTMLButtonElement>(null);
  const modalRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isOpen) {
      // Focus the cancel button when modal opens (safer default)
      confirmButtonRef.current?.focus();

      // Trap focus within modal
      const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          onCancel();
        }
      };

      document.addEventListener('keydown', handleKeyDown);
      return () => document.removeEventListener('keydown', handleKeyDown);
    }
  }, [isOpen, onCancel]);

  if (!isOpen) return null;

  return (
    <div
      className="modal-overlay"
      onClick={onCancel}
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
      aria-describedby="modal-message"
    >
      <div className="modal-content" onClick={(e) => e.stopPropagation()} ref={modalRef}>
        <h2 id="modal-title" className="modal-title">
          {title}
        </h2>
        <p id="modal-message" className="modal-message">
          {message}
        </p>
        <div className="modal-actions">
          <button type="button" className="btn-modal-cancel" onClick={onCancel}>
            {cancelLabel}
          </button>
          <button
            type="button"
            className={`btn-modal-confirm ${variant === 'danger' ? 'btn-danger' : ''}`}
            onClick={onConfirm}
            ref={confirmButtonRef}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
