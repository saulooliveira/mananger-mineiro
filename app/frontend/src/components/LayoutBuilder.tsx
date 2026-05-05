import React, { useState, useRef } from 'react';
import {
  LayoutData,
  LayoutElement,
  TextElement,
  ImageElement,
  QRCodeElement,
  BarcodeElement,
  FormulaElement,
  DEFAULT_LAYOUT,
  AVAILABLE_FIELDS,
} from '../types/LayoutTypes';
import '../styles/layout-builder.css';

const LayoutBuilder: React.FC = () => {
  const [layout, setLayout] = useState<LayoutData>(DEFAULT_LAYOUT);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [draggingId, setDraggingId] = useState<string | null>(null);
  const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
  const canvasRef = useRef<HTMLDivElement>(null);

  const selectedElement = layout.elements.find(e => e.id === selectedId);

  // Adicionar elemento
  const addElement = (type: LayoutElement['type']) => {
    const id = `${type}-${Date.now()}`;
    let newElement: LayoutElement;

    switch (type) {
      case 'text':
        newElement = {
          id,
          type: 'text',
          source: 'field',
          fieldName: 'descricao',
          previewText: 'Texto',
          xMm: 10,
          yMm: 20,
          widthMm: 50,
          heightMm: 8,
          fontSize: 14,
          bold: false,
          align: 'left',
          color: '#000000',
        } as TextElement;
        break;
      case 'image':
        newElement = {
          id,
          type: 'image',
          source: 'fixed',
          imagePath: '',
          xMm: 10,
          yMm: 10,
          widthMm: 30,
          heightMm: 30,
        } as ImageElement;
        break;
      case 'qrcode':
        newElement = {
          id,
          type: 'qrcode',
          source: 'fixed',
          value: 'https://example.com',
          xMm: 10,
          yMm: 10,
          widthMm: 25,
          heightMm: 25,
        } as QRCodeElement;
        break;
      case 'barcode':
        newElement = {
          id,
          type: 'barcode',
          barcodeType: 'ean13',
          source: 'field',
          fieldName: 'codigo_barras',
          xMm: 10,
          yMm: 10,
          widthMm: 40,
          heightMm: 10,
        } as BarcodeElement;
        break;
      case 'formula':
        newElement = {
          id,
          type: 'formula',
          formula: '"R$ " + preco',
          previewText: 'R$ 2,99',
          xMm: 10,
          yMm: 20,
          widthMm: 40,
          heightMm: 8,
          fontSize: 16,
          bold: true,
          align: 'left',
          color: '#000000',
        } as FormulaElement;
        break;
      default:
        return;
    }

    setLayout(prev => ({
      ...prev,
      elements: [...prev.elements, newElement],
    }));
    setSelectedId(id);
  };

  // Remover elemento
  const removeElement = (id: string) => {
    setLayout(prev => ({
      ...prev,
      elements: prev.elements.filter(e => e.id !== id),
    }));
    if (selectedId === id) setSelectedId(null);
  };

  // Atualizar elemento
  const updateElement = (id: string, updates: Partial<LayoutElement>) => {
    setLayout(prev => ({
      ...prev,
      elements: prev.elements.map(e =>
        e.id === id ? { ...e, ...updates } : e
      ),
    }));
  };

  // Mover elemento (mouse)
  const handleMouseDown = (id: string, e: React.MouseEvent) => {
    if (!canvasRef.current) return;
    const rect = canvasRef.current.getBoundingClientRect();
    const element = layout.elements.find(el => el.id === id);
    if (!element) return;

    setDraggingId(id);
    setDragOffset({
      x: e.clientX - rect.left - (element.xMm * 96) / 25.4,
      y: e.clientY - rect.top - (element.yMm * 96) / 25.4,
    });
    setSelectedId(id);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!draggingId || !canvasRef.current) return;
    const rect = canvasRef.current.getBoundingClientRect();
    const pxPerMm = 96 / 25.4;

    const newX = Math.max(0, (e.clientX - rect.left - dragOffset.x) / pxPerMm);
    const newY = Math.max(0, (e.clientY - rect.top - dragOffset.y) / pxPerMm);

    updateElement(draggingId, { xMm: newX, yMm: newY });
  };

  const handleMouseUp = () => {
    setDraggingId(null);
  };

  // Imprimir com layout
  const printLayout = async () => {
    try {
      // Teste com produtos 1-4
      const response = await fetch('http://localhost:5274/api/print/builder-preview', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          produtoIds: [1, 2, 3, 4],
          layout: layout,
        }),
      });

      if (!response.ok) {
        throw new Error('Erro ao gerar PDF');
      }

      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'layout-print.pdf';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      alert('Erro ao imprimir');
      console.error(error);
    }
  };

  // Salvar layout
  const saveLayout = async () => {
    try {
      const response = await fetch('http://localhost:5274/api/layout-builder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(layout),
      });
      if (response.ok) {
        alert('Layout salvo com sucesso!');
      } else {
        alert('Erro ao salvar layout');
      }
    } catch (error) {
      alert('Erro de conexão ao salvar');
      console.error(error);
    }
  };

  // Carregar layout
  const loadLayout = async () => {
    try {
      const response = await fetch('http://localhost:5274/api/layout-builder');
      if (response.ok) {
        const data = await response.json();
        setLayout(data);
      }
    } catch (error) {
      console.error('Erro ao carregar layout:', error);
    }
  };

  return (
    <div className="layout-builder">
      <div className="builder-sidebar">
        <div className="sidebar-section">
          <h3>Página</h3>
          <div className="form-group">
            <label>Colunas</label>
            <input
              type="number"
              min="1"
              max="4"
              value={layout.page.columns}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, columns: parseInt(e.target.value) },
                }))
              }
            />
          </div>
          <div className="form-group">
            <label>Linhas</label>
            <input
              type="number"
              min="1"
              max="4"
              value={layout.page.rows}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, rows: parseInt(e.target.value) },
                }))
              }
            />
          </div>
          <div className="form-group">
            <label>Largura Card (mm)</label>
            <input
              type="number"
              min="10"
              max="200"
              value={layout.page.cardWidthMm}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, cardWidthMm: parseFloat(e.target.value) },
                }))
              }
            />
          </div>
          <div className="form-group">
            <label>Altura Card (mm)</label>
            <input
              type="number"
              min="10"
              max="200"
              value={layout.page.cardHeightMm}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, cardHeightMm: parseFloat(e.target.value) },
                }))
              }
            />
          </div>
          <div className="form-group">
            <label>Margem (mm)</label>
            <input
              type="number"
              min="0"
              max="50"
              value={layout.page.marginMm}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, marginMm: parseInt(e.target.value) },
                }))
              }
            />
          </div>
          <div className="form-group">
            <label>Espaço (mm)</label>
            <input
              type="number"
              min="0"
              max="20"
              value={layout.page.gapMm}
              onChange={e =>
                setLayout(prev => ({
                  ...prev,
                  page: { ...prev.page, gapMm: parseInt(e.target.value) },
                }))
              }
            />
          </div>
        </div>

        <div className="sidebar-section">
          <h3>Adicionar Elemento</h3>
          <button className="btn-add" onClick={() => addElement('text')}>
            + Texto
          </button>
          <button className="btn-add" onClick={() => addElement('image')}>
            + Imagem
          </button>
          <button className="btn-add" onClick={() => addElement('qrcode')}>
            + QR Code
          </button>
          <button className="btn-add" onClick={() => addElement('barcode')}>
            + Código Barras
          </button>
          <button className="btn-add" onClick={() => addElement('formula')}>
            + Fórmula
          </button>
        </div>

        {selectedElement && (
          <div className="sidebar-section">
            <h3>Configurar Elemento</h3>

            <div className="form-group">
              <label>X (mm)</label>
              <input
                type="number"
                step="0.5"
                value={selectedElement.xMm}
                onChange={e =>
                  updateElement(selectedId!, { xMm: parseFloat(e.target.value) })
                }
              />
            </div>

            <div className="form-group">
              <label>Y (mm)</label>
              <input
                type="number"
                step="0.5"
                value={selectedElement.yMm}
                onChange={e =>
                  updateElement(selectedId!, { yMm: parseFloat(e.target.value) })
                }
              />
            </div>

            <div className="form-group">
              <label>Largura (mm)</label>
              <input
                type="number"
                step="0.5"
                value={selectedElement.widthMm}
                onChange={e =>
                  updateElement(selectedId!, { widthMm: parseFloat(e.target.value) })
                }
              />
            </div>

            <div className="form-group">
              <label>Altura (mm)</label>
              <input
                type="number"
                step="0.5"
                value={selectedElement.heightMm}
                onChange={e =>
                  updateElement(selectedId!, { heightMm: parseFloat(e.target.value) })
                }
              />
            </div>

            {(selectedElement.type === 'text' || selectedElement.type === 'formula') && (
              <>
                <div className="form-group">
                  <label>Tamanho Fonte</label>
                  <input
                    type="number"
                    min="8"
                    max="48"
                    value={(selectedElement as TextElement | FormulaElement).fontSize}
                    onChange={e =>
                      updateElement(selectedId!, { fontSize: parseInt(e.target.value) })
                    }
                  />
                </div>

                <div className="form-group">
                  <label>
                    <input
                      type="checkbox"
                      checked={(selectedElement as TextElement | FormulaElement).bold}
                      onChange={e =>
                        updateElement(selectedId!, { bold: e.target.checked })
                      }
                    />
                    Negrito
                  </label>
                </div>

                <div className="form-group">
                  <label>Alinhamento</label>
                  <select
                    value={(selectedElement as TextElement | FormulaElement).align}
                    onChange={e =>
                      updateElement(selectedId!, { align: e.target.value as 'left' | 'center' | 'right' })
                    }
                  >
                    <option value="left">Esquerda</option>
                    <option value="center">Centro</option>
                    <option value="right">Direita</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Cor</label>
                  <input
                    type="color"
                    value={(selectedElement as TextElement | FormulaElement).color}
                    onChange={e =>
                      updateElement(selectedId!, { color: e.target.value })
                    }
                  />
                </div>
              </>
            )}

            {selectedElement.type === 'text' && (
              <>
                <div className="form-group">
                  <label>Fonte</label>
                  <select
                    value={(selectedElement as TextElement).source}
                    onChange={e =>
                      updateElement(selectedId!, { source: e.target.value as 'fixed' | 'field' | 'formula' })
                    }
                  >
                    <option value="fixed">Texto Fixo</option>
                    <option value="field">Campo do Banco</option>
                    <option value="formula">Fórmula</option>
                  </select>
                </div>

                {(selectedElement as TextElement).source === 'field' && (
                  <div className="form-group">
                    <label>Campo</label>
                    <select
                      value={(selectedElement as TextElement).fieldName || ''}
                      onChange={e =>
                        updateElement(selectedId!, { fieldName: e.target.value })
                      }
                    >
                      <option value="">Selecionar campo</option>
                      {AVAILABLE_FIELDS.map(f => (
                        <option key={f.value} value={f.value}>{f.label}</option>
                      ))}
                    </select>
                  </div>
                )}

                {(selectedElement as TextElement).source === 'fixed' && (
                  <div className="form-group">
                    <label>Texto</label>
                    <input
                      type="text"
                      value={(selectedElement as TextElement).previewText}
                      onChange={e =>
                        updateElement(selectedId!, { previewText: e.target.value })
                      }
                    />
                  </div>
                )}
              </>
            )}

            {selectedElement.type === 'formula' && (
              <div className="form-group">
                <label>Fórmula</label>
                <textarea
                  value={(selectedElement as FormulaElement).formula}
                  onChange={e =>
                    updateElement(selectedId!, { formula: e.target.value })
                  }
                  placeholder='"R$ " + preco'
                />
              </div>
            )}

            {selectedElement.type === 'barcode' && (
              <div className="form-group">
                <label>Tipo</label>
                <select
                  value={(selectedElement as BarcodeElement).barcodeType}
                  onChange={e =>
                    updateElement(selectedId!, { barcodeType: e.target.value as 'ean13' | 'code128' })
                  }
                >
                  <option value="ean13">EAN-13</option>
                  <option value="code128">CODE-128</option>
                </select>
              </div>
            )}

            <button
              className="btn-remove"
              onClick={() => removeElement(selectedId!)}
            >
              🗑️ Remover Elemento
            </button>
          </div>
        )}

        <div className="sidebar-section">
          <button className="btn-save" onClick={saveLayout}>
            💾 Salvar Layout
          </button>
          <button className="btn-load" onClick={loadLayout}>
            📂 Carregar Layout
          </button>
          <button className="btn-print" onClick={printLayout}>
            🖨️ Imprimir (Teste)
          </button>
        </div>
      </div>

      <div className="builder-canvas">
        <div
          className="canvas-content"
          ref={canvasRef}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
        >
          <div
            className="a4-page"
            style={{
              padding: `${layout.page.marginMm}mm`,
              display: 'grid',
              gridTemplateColumns: `repeat(${layout.page.columns}, 1fr)`,
              gap: `${layout.page.gapMm}mm`,
            }}
          >
            {Array.from({ length: layout.page.rows * layout.page.columns }).map((_, i) => (
              <div
                key={i}
                className="card-slot"
                style={{
                  width: `${layout.page.cardWidthMm}mm`,
                  height: `${layout.page.cardHeightMm}mm`,
                  border: '2px solid #ddd',
                  position: 'relative',
                  backgroundColor: '#fff',
                }}
              >
                {layout.elements.map(el => (
                  <div
                    key={el.id}
                    className={`element ${selectedId === el.id ? 'selected' : ''}`}
                    style={{
                      position: 'absolute',
                      left: `${el.xMm}mm`,
                      top: `${el.yMm}mm`,
                      width: `${el.widthMm}mm`,
                      height: `${el.heightMm}mm`,
                      cursor: draggingId === el.id ? 'grabbing' : 'grab',
                      border: selectedId === el.id ? '2px solid #2196F3' : '1px dashed #999',
                      padding: '2px',
                      fontSize: (el as any).fontSize ? `${(el as any).fontSize}px` : '12px',
                      fontWeight: (el as any).bold ? 'bold' : 'normal',
                      textAlign: (el as any).align || 'left',
                      color: (el as any).color || '#000',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      overflow: 'hidden',
                    }}
                    onMouseDown={(e) => handleMouseDown(el.id, e)}
                    onClick={() => setSelectedId(el.id)}
                  >
                    {el.type === 'text' && (
                      <span>{(el as TextElement).previewText}</span>
                    )}
                    {el.type === 'formula' && (
                      <span>{(el as FormulaElement).previewText}</span>
                    )}
                    {el.type === 'image' && (
                      <span>📷</span>
                    )}
                    {el.type === 'qrcode' && (
                      <span>QR</span>
                    )}
                    {el.type === 'barcode' && (
                      <span>|||</span>
                    )}
                  </div>
                ))}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default LayoutBuilder;
