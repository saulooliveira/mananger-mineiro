import React from 'react';
import '../styles/pdf-modal.css';

interface PdfModalProps {
  isOpen: boolean;
  pdfUrl: string | null;
  onClose: () => void;
  onSave: () => void;
  onPrint: () => void;
}

function PdfModal({ isOpen, pdfUrl, onClose, onSave, onPrint }: PdfModalProps) {
  if (!isOpen || !pdfUrl) {
    return null;
  }

  return (
    <div className="pdf-modal-overlay" onClick={onClose}>
      <div className="pdf-modal-container" onClick={(e) => e.stopPropagation()}>
        <div className="pdf-modal-header">
          <h2>Preview do PDF</h2>
          <button
            type="button"
            className="pdf-modal-close"
            onClick={onClose}
            aria-label="Fechar modal"
          >
            ✕
          </button>
        </div>

        <div className="pdf-modal-content">
          <iframe
            src={pdfUrl}
            title="Preview PDF"
            className="pdf-modal-iframe"
          />
        </div>

        <div className="pdf-modal-footer">
          <button
            type="button"
            className="modal-button save-button"
            onClick={onSave}
          >
            Salvar PDF
          </button>
          <button
            type="button"
            className="modal-button print-button"
            onClick={onPrint}
          >
            Imprimir
          </button>
          <button
            type="button"
            className="modal-button cancel-button"
            onClick={onClose}
          >
            Fechar
          </button>
        </div>
      </div>
    </div>
  );
}

export default PdfModal;
