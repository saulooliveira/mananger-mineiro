import React from 'react';
import '../styles/settings.css';

interface PdfConfig {
  pageMargin: number;
  gridColumns: number;
  gridRows: number;
  gridGapMm: number;
  elements: {
    [key: string]: {
      fontSize: number;
      fontFamily: string;
    };
  };
}

const defaultConfig: PdfConfig = {
  pageMargin: 10,
  gridColumns: 2,
  gridRows: 2,
  gridGapMm: 5,
  elements: {
    title: { fontSize: 16, fontFamily: 'Arial' },
    description: { fontSize: 12, fontFamily: 'Arial' },
    price: { fontSize: 20, fontFamily: 'Arial Bold' },
    unit: { fontSize: 10, fontFamily: 'Arial' },
    footer: { fontSize: 8, fontFamily: 'Arial' },
  },
};

function SettingsScreen() {
  const [config, setConfig] = React.useState<PdfConfig>(defaultConfig);
  const [loading, setLoading] = React.useState(false);
  const [message, setMessage] = React.useState<{ type: 'success' | 'error'; text: string } | null>(null);

  React.useEffect(() => {
    loadConfig();
  }, []);

  const loadConfig = async () => {
    try {
      const response = await fetch('http://localhost:5274/api/pdf-config');
      if (response.ok) {
        const data = await response.json();
        setConfig(data);
      }
    } catch (err) {
      console.error('Falha ao carregar configuração:', err);
    }
  };

  const saveConfig = async () => {
    setLoading(true);
    setMessage(null);

    try {
      const response = await fetch('http://localhost:5274/api/pdf-config', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(config),
      });

      if (response.ok) {
        setMessage({ type: 'success', text: 'Configuração salva com sucesso!' });
      } else {
        setMessage({ type: 'error', text: 'Falha ao salvar configuração.' });
      }
    } catch (err) {
      setMessage({ type: 'error', text: 'Erro ao conectar com o servidor.' });
    } finally {
      setLoading(false);
    }
  };

  const updateElement = (elementKey: string, field: 'fontSize' | 'fontFamily', value: number | string) => {
    setConfig((prev) => ({
      ...prev,
      elements: {
        ...prev.elements,
        [elementKey]: {
          ...prev.elements[elementKey],
          [field]: value,
        },
      },
    }));
  };

  const fontFamilies = ['Arial', 'Arial Bold', 'Times New Roman', 'Courier New', 'Helvetica', 'Verdana'];

  return (
    <div className="settings-screen">
      <div className="settings-header">
        <h2>Configurações de PDF</h2>
      </div>

      <div className="settings-content">
        <div className="settings-section">
          <h3>Configuração da Página A4</h3>
          <div className="setting-group">
            <label>
              Margem da Página (mm):
              <input
                type="number"
                min="0"
                max="50"
                value={config.pageMargin}
                onChange={(e) => setConfig({ ...config, pageMargin: parseInt(e.target.value) })}
                className="setting-input"
              />
            </label>
          </div>
        </div>

        <div className="settings-section">
          <h3>Configuração da Grade</h3>
          <div className="setting-group">
            <label>
              Colunas:
              <input
                type="number"
                min="1"
                max="4"
                value={config.gridColumns}
                onChange={(e) => setConfig({ ...config, gridColumns: parseInt(e.target.value) })}
                className="setting-input"
              />
            </label>
          </div>
          <div className="setting-group">
            <label>
              Linhas:
              <input
                type="number"
                min="1"
                max="4"
                value={config.gridRows}
                onChange={(e) => setConfig({ ...config, gridRows: parseInt(e.target.value) })}
                className="setting-input"
              />
            </label>
          </div>
          <div className="setting-group">
            <label>
              Espaçamento (mm):
              <input
                type="number"
                min="0"
                max="20"
                value={config.gridGapMm}
                onChange={(e) => setConfig({ ...config, gridGapMm: parseInt(e.target.value) })}
                className="setting-input"
              />
            </label>
          </div>
        </div>

        <div className="settings-section">
          <h3>Fontes dos Elementos</h3>
          {Object.entries(config.elements).map(([elementKey, element]) => (
            <div key={elementKey} className="element-config">
              <h4 className="element-name">{elementKey}</h4>
              <div className="element-fields">
                <div className="setting-group">
                  <label>
                    Tamanho (pt):
                    <input
                      type="number"
                      min="6"
                      max="72"
                      value={element.fontSize}
                      onChange={(e) => updateElement(elementKey, 'fontSize', parseInt(e.target.value))}
                      className="setting-input"
                    />
                  </label>
                </div>
                <div className="setting-group">
                  <label>
                    Fonte:
                    <select
                      value={element.fontFamily}
                      onChange={(e) => updateElement(elementKey, 'fontFamily', e.target.value)}
                      className="setting-select"
                    >
                      {fontFamilies.map((font) => (
                        <option key={font} value={font}>
                          {font}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
              </div>
            </div>
          ))}
        </div>

        {message && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        <div className="settings-actions">
          <button
            type="button"
            className="save-button"
            onClick={saveConfig}
            disabled={loading}
          >
            {loading ? 'Salvando...' : 'Salvar Configuração'}
          </button>
          <button
            type="button"
            className="reset-button"
            onClick={() => setConfig(defaultConfig)}
            disabled={loading}
          >
            Restaurar Padrão
          </button>
        </div>
      </div>
    </div>
  );
}

export default SettingsScreen;
