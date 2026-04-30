import React from 'react';
import '../styles/layout-editor.css';
import PdfModal from './PdfModal';

interface Element {
  x: number;
  y: number;
  text: string;
  imagePath: string;
  fontSize: number;
  bold: boolean;
  alignment: 'left' | 'center' | 'right';
  color: string;
  visible: boolean;
}

interface Card {
  id: string;
  x: number;
  y: number;
  w: number;
  h: number;
  content: {
    title: Element;
    subtitle: Element;
    price: Element;
    unit: Element;
    footer: Element;
  };
}

interface LayoutConfig {
  cards: Card[];
  pageMargin?: number;
  gridColumns?: number;
  gridRows?: number;
  gridGapMm?: number;
}

const defaultConfig: LayoutConfig = {
  cards: [
    {
      id: 'cebola',
      x: 10,
      y: 10,
      w: 92.5,
      h: 136,
      content: {
        title: { x: 5, y: 15, text: 'CEBOLA', fontSize: 16, bold: true, alignment: 'left', color: '#000', visible: true },
        subtitle: { x: 5, y: 30, text: 'Nacional', fontSize: 12, bold: false, alignment: 'left', color: '#666', visible: true },
        price: { x: 5, y: 60, text: '2,99', fontSize: 20, bold: true, alignment: 'left', color: '#000', visible: true },
        unit: { x: 70, y: 63, text: 'KG', fontSize: 10, bold: false, alignment: 'left', color: '#666', visible: true },
        footer: { x: 5, y: 115, text: 'Oferta válida enquanto durarem os estoques', fontSize: 8, bold: false, alignment: 'left', color: '#999', visible: true },
      },
    },
    {
      id: 'laranja',
      x: 107.5,
      y: 10,
      w: 92.5,
      h: 136,
      content: {
        title: { x: 5, y: 15, text: 'LARANJA', fontSize: 16, bold: true, alignment: 'left', color: '#000', visible: true },
        subtitle: { x: 5, y: 30, text: 'Pera', fontSize: 12, bold: false, alignment: 'left', color: '#666', visible: true },
        price: { x: 5, y: 60, text: '3,99', fontSize: 20, bold: true, alignment: 'left', color: '#000', visible: true },
        unit: { x: 70, y: 63, text: 'KG', fontSize: 10, bold: false, alignment: 'left', color: '#666', visible: true },
        footer: { x: 5, y: 115, text: '', fontSize: 8, bold: false, alignment: 'left', color: '#999', visible: false },
      },
    },
    {
      id: 'tomate',
      x: 10,
      y: 151,
      w: 92.5,
      h: 136,
      content: {
        title: { x: 5, y: 15, text: 'TOMATE', fontSize: 16, bold: true, alignment: 'left', color: '#000', visible: true },
        subtitle: { x: 5, y: 30, text: 'Salada', fontSize: 12, bold: false, alignment: 'left', color: '#666', visible: true },
        price: { x: 5, y: 60, text: '4,99', fontSize: 20, bold: true, alignment: 'left', color: '#000', visible: true },
        unit: { x: 70, y: 63, text: 'KG', fontSize: 10, bold: false, alignment: 'left', color: '#666', visible: true },
        footer: { x: 5, y: 115, text: '', fontSize: 8, bold: false, alignment: 'left', color: '#999', visible: false },
      },
    },
    {
      id: 'maracuja',
      x: 107.5,
      y: 151,
      w: 92.5,
      h: 136,
      content: {
        title: { x: 5, y: 15, text: 'MARACUJÁ', fontSize: 16, bold: true, alignment: 'left', color: '#000', visible: true },
        subtitle: { x: 5, y: 30, text: 'Azedo', fontSize: 12, bold: false, alignment: 'left', color: '#666', visible: true },
        price: { x: 5, y: 60, text: '6,99', fontSize: 20, bold: true, alignment: 'left', color: '#000', visible: true },
        unit: { x: 70, y: 63, text: 'KG', fontSize: 10, bold: false, alignment: 'left', color: '#666', visible: true },
        footer: { x: 5, y: 115, text: '', fontSize: 8, bold: false, alignment: 'left', color: '#999', visible: false },
      },
    },
  ],
};

function LayoutEditor() {
  const [config, setConfig] = React.useState<LayoutConfig>(defaultConfig);
  const [selectedCardId, setSelectedCardId] = React.useState<string>('cebola');
  const [selectedElement, setSelectedElement] = React.useState<string | null>(null);
  const [dragging, setDragging] = React.useState<{ cardId: string; element: string; startX: number; startY: number } | null>(null);
  const [activeTab, setActiveTab] = React.useState<'cards' | 'config'>('cards');
  const [isModalOpen, setIsModalOpen] = React.useState(false);
  const [previewUrl, setPreviewUrl] = React.useState<string | null>(null);
  const [previewLoading, setPreviewLoading] = React.useState(false);
  const canvasRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  React.useEffect(() => {
    loadLayoutConfig();
  }, []);

  const loadLayoutConfig = async () => {
    try {
      const response = await fetch('http://localhost:5274/api/layout-config');
      if (response.ok) {
        const data = await response.json();
        if (data.cards && data.cards.length > 0) {
          setConfig(data);
        }
      }
    } catch (error) {
      console.error('Erro ao carregar layout:', error);
    }
  };

  const selectedCard = config.cards.find(c => c.id === selectedCardId);

  const handleElementMouseDown = (cardId: string, elementKey: string, e: React.MouseEvent) => {
    e.preventDefault();
    setSelectedCardId(cardId);
    setSelectedElement(elementKey);

    if (canvasRef.current) {
      const rect = canvasRef.current.getBoundingClientRect();
      setDragging({
        cardId,
        element: elementKey,
        startX: e.clientX - rect.left,
        startY: e.clientY - rect.top,
      });
    }
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!dragging || !canvasRef.current) return;

    const rect = canvasRef.current.getBoundingClientRect();
    const currentX = e.clientX - rect.left;
    const currentY = e.clientY - rect.top;

    const pxPerMm = 96 / 25.4;
    const deltaX = (currentX - dragging.startX) / pxPerMm;
    const deltaY = (currentY - dragging.startY) / pxPerMm;

    const card = config.cards.find(c => c.id === dragging.cardId)!;
    const element = card.content[dragging.element as keyof typeof card.content];

    const newX = Math.max(0, Math.min(card.w - 10, element.x + deltaX));
    const newY = Math.max(0, Math.min(card.h - 10, element.y + deltaY));

    setConfig(prev => ({
      cards: prev.cards.map(c =>
        c.id === dragging.cardId
          ? {
              ...c,
              content: {
                ...c.content,
                [dragging.element]: {
                  ...element,
                  x: newX,
                  y: newY,
                },
              },
            }
          : c
      ),
    }));

    setDragging({
      ...dragging,
      startX: currentX,
      startY: currentY,
    });
  };

  const handleMouseUp = () => {
    setDragging(null);
  };

  const updateElement = (cardId: string, elementKey: string, updates: Partial<Element>) => {
    setConfig(prev => ({
      cards: prev.cards.map(c =>
        c.id === cardId
          ? {
              ...c,
              content: {
                ...c.content,
                [elementKey]: {
                  ...c.content[elementKey as keyof typeof c.content],
                  ...updates,
                },
              },
            }
          : c
      ),
    }));
  };

  const handleSaveConfig = async () => {
    try {
      const response = await fetch('http://localhost:5274/api/layout-config', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(config),
      });
      if (response.ok) {
        alert('Layout salvo com sucesso!');
      } else {
        alert('Erro ao salvar layout');
      }
    } catch (error) {
      alert('Erro de conexão ao salvar layout');
      console.error(error);
    }
  };

  const handlePrintLayout = async () => {
    try {
      setPreviewLoading(true);

      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
        setPreviewUrl(null);
      }

      const testProductIds = [1, 2, 3, 4];

      const response = await fetch('http://localhost:5274/api/print/preview', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          productIds: testProductIds,
          editedPrices: {},
        }),
      });

      if (!response.ok) {
        throw new Error('Erro ao gerar PDF');
      }

      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      setPreviewUrl(url);
      setIsModalOpen(true);
    } catch (error) {
      alert('Erro ao gerar preview para impressão');
      console.error(error);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleSavePdf = () => {
    if (!previewUrl) return;

    const link = document.createElement('a');
    link.href = previewUrl;
    link.download = 'layout-preview.pdf';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handlePrintPdf = () => {
    if (!previewUrl) return;

    const printWindow = window.open(previewUrl, '_blank');
    if (printWindow) {
      setTimeout(() => {
        printWindow.print();
      }, 250);
    }
  };

  const elementKeys = ['title', 'subtitle', 'price', 'unit', 'footer'] as const;

  return (
    <>
    <div className="layout-editor">
      <div className="editor-header">
        <h2>Editor de Layout</h2>
        <div className="header-buttons">
          <button type="button" onClick={handleSaveConfig} className="save-button">
            💾 Salvar Layout
          </button>
          <button type="button" onClick={handlePrintLayout} className="print-button">
            🖨️ Imprimir
          </button>
        </div>
      </div>

      <div className="editor-container">
        <div className="canvas-section" ref={canvasRef} onMouseMove={handleMouseMove} onMouseUp={handleMouseUp} onMouseLeave={handleMouseUp}>
          <div className="a4-page">
            {config.cards.map(card => (
              <div
                key={card.id}
                className={`card ${selectedCardId === card.id ? 'selected' : ''}`}
                style={{
                  left: `${card.x}mm`,
                  top: `${card.y}mm`,
                  width: `${card.w}mm`,
                  height: `${card.h}mm`,
                }}
                onClick={() => setSelectedCardId(card.id)}
              >
                {elementKeys.map(elementKey => {
                  const element = card.content[elementKey];
                  if (!element.visible) return null;

                  return (
                    <div
                      key={elementKey}
                      className={`element ${selectedElement === elementKey && selectedCardId === card.id ? 'selected' : ''}`}
                      style={{
                        left: `${element.x}mm`,
                        top: `${element.y}mm`,
                        fontSize: `${element.fontSize}pt`,
                        fontWeight: element.bold ? 'bold' : 'normal',
                        textAlign: element.alignment,
                        color: element.color,
                        cursor: 'grab',
                      }}
                      onMouseDown={(e) => handleElementMouseDown(card.id, elementKey, e)}
                    >
                      {element.text}
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        </div>

        {selectedCard && (
          <div className="config-panel">
            <div className="config-tabs">
              <button
                className={`config-tab ${activeTab === 'cards' ? 'active' : ''}`}
                onClick={() => setActiveTab('cards')}
              >
                Cards
              </button>
              <button
                className={`config-tab ${activeTab === 'config' ? 'active' : ''}`}
                onClick={() => setActiveTab('config')}
              >
                ⚙️ Config
              </button>
            </div>

            {activeTab === 'cards' && (
              <>
                <h3>Configurar: {selectedCard.id}</h3>
                <div className="card-list">
              <h4>Cards</h4>
              {config.cards.map(card => (
                <button
                  key={card.id}
                  className={`card-button ${selectedCardId === card.id ? 'active' : ''}`}
                  onClick={() => setSelectedCardId(card.id)}
                >
                  {card.id}
                </button>
              ))}
            </div>

            <div className="element-editors">
              {elementKeys.map(elementKey => {
                const element = selectedCard.content[elementKey];

                return (
                  <div key={elementKey} className={`element-editor ${selectedElement === elementKey ? 'selected' : ''}`}>
                    <h4>{elementKey}</h4>

                    <label>
                      Texto:
                      <input
                        type="text"
                        value={element.text}
                        onChange={(e) => updateElement(selectedCardId, elementKey, { text: e.target.value })}
                      />
                    </label>

                    <label>
                      <input
                        type="checkbox"
                        checked={element.visible}
                        onChange={(e) => updateElement(selectedCardId, elementKey, { visible: e.target.checked })}
                      />
                      Visível
                    </label>

                    {element.visible && (
                      <>
                        <div className="position-editor">
                          <label>
                            X (mm):
                            <input
                              type="number"
                              value={element.x}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { x: parseFloat(e.target.value) })}
                            />
                          </label>
                          <label>
                            Y (mm):
                            <input
                              type="number"
                              value={element.y}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { y: parseFloat(e.target.value) })}
                            />
                          </label>
                        </div>

                        <div className="style-editor">
                          <label>
                            Tamanho (pt):
                            <input
                              type="number"
                              value={element.fontSize}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { fontSize: parseInt(e.target.value) })}
                            />
                          </label>

                          <label>
                            <input
                              type="checkbox"
                              checked={element.bold}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { bold: e.target.checked })}
                            />
                            Negrito
                          </label>

                          <label>
                            Alinhamento:
                            <select
                              value={element.alignment}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { alignment: e.target.value as any })}
                            >
                              <option value="left">Esquerda</option>
                              <option value="center">Centro</option>
                              <option value="right">Direita</option>
                            </select>
                          </label>

                          <label>
                            Cor:
                            <input
                              type="color"
                              value={element.color}
                              onChange={(e) => updateElement(selectedCardId, elementKey, { color: e.target.value })}
                            />
                          </label>

                          <label>
                            Imagem:
                            <input
                              type="file"
                              accept="image/*"
                              onChange={(e) => handleImageUpload(selectedCardId, elementKey, e.target.files?.[0], setConfig)}
                            />
                          </label>
                        </div>
                      </>
                    )}
                  </div>
                );
              })}
            </div>
              </>
            )}

            {activeTab === 'config' && (
              <>
                <h3>Configuração da Página</h3>
                <div className="config-section">
                  <label>
                    Margem da Página (mm):
                    <input
                      type="number"
                      value={config.pageMargin ?? 10}
                      onChange={(e) => setConfig({ ...config, pageMargin: parseInt(e.target.value) })}
                    />
                  </label>

                  <label>
                    Colunas do Grid:
                    <input
                      type="number"
                      value={config.gridColumns ?? 2}
                      onChange={(e) => setConfig({ ...config, gridColumns: parseInt(e.target.value) })}
                    />
                  </label>

                  <label>
                    Linhas do Grid:
                    <input
                      type="number"
                      value={config.gridRows ?? 2}
                      onChange={(e) => setConfig({ ...config, gridRows: parseInt(e.target.value) })}
                    />
                  </label>

                  <label>
                    Espaçamento do Grid (mm):
                    <input
                      type="number"
                      value={config.gridGapMm ?? 5}
                      onChange={(e) => setConfig({ ...config, gridGapMm: parseInt(e.target.value) })}
                    />
                  </label>
                </div>
              </>
            )}
          </div>
        )}
      </div>
    </div>

    <PdfModal
      isOpen={isModalOpen}
      pdfUrl={previewUrl}
      onClose={() => setIsModalOpen(false)}
      onSave={handleSavePdf}
      onPrint={handlePrintPdf}
    />
    </>
  );
}

async function handleImageUpload(cardId: string, elementKey: string, file: File | undefined, setConfig: React.Dispatch<React.SetStateAction<LayoutConfig>>) {
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await fetch('http://localhost:5274/api/upload/image', {
      method: 'POST',
      body: formData,
    });

    if (response.ok) {
      const data = await response.json();
      setConfig(prev => ({
        cards: prev.cards.map(c =>
          c.id === cardId
            ? {
                ...c,
                content: {
                  ...c.content,
                  [elementKey]: {
                    ...c.content[elementKey as keyof typeof c.content],
                    imagePath: data.path,
                  },
                },
              }
            : c
        ),
      }));
    } else {
      alert('Erro ao fazer upload da imagem');
    }
  } catch (error) {
    alert('Erro de conexão ao fazer upload');
    console.error(error);
  }
}

export default LayoutEditor;
