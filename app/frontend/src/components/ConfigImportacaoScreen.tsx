import React, { useState, useEffect, useRef } from 'react';
import '../styles/config-importacao.css';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5274/api';

interface DbfConfig {
  filePath: string;
  codigoColumn: string;
  descricaoColumn: string;
  categoriaColumn: string;
  valorColumn: string;
  yieldColumn: string;
}

interface ImportResult {
  inserted: number;
  updated: number;
}

interface Message {
  type: 'success' | 'error';
  text: string;
}

const ConfigImportacaoScreen: React.FC = () => {
  const [config, setConfig] = useState<DbfConfig>({
    filePath: '',
    codigoColumn: '',
    descricaoColumn: '',
    categoriaColumn: '',
    valorColumn: '',
    yieldColumn: '',
  });

  const [dbfColumns, setDbfColumns] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const [message, setMessage] = useState<Message | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    loadConfig();
  }, []);

  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => {
        setMessage(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [message]);

  const loadConfig = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/dbf-config`);
      if (response.ok) {
        const data = await response.json();
        setConfig(data);
      }
    } catch (error) {
      console.error('Error loading config:', error);
    }
  };

  const handleFileSelected = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      let fullPath = (file as any).path;

      // Se não tiver path (Electron property), usa o name como fallback
      // Mas alerta o usuário que precisa do caminho completo
      if (!fullPath) {
        fullPath = file.name;
        console.warn('Aviso: arquivo selecionado sem caminho completo. Use path completo.');
      }

      console.log('Caminho do arquivo selecionado:', fullPath);

      setConfig(prev => ({
        ...prev,
        filePath: fullPath,
      }));
      setDbfColumns([]);
    }
  };

  const handleSelectFile = () => {
    fileInputRef.current?.click();
  };

  const handleLoadFields = async () => {
    if (!config.filePath) {
      setMessage({
        type: 'error',
        text: 'Selecione um arquivo DBF primeiro',
      });
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(
        `${API_BASE_URL}/dbf-config/fields?path=${encodeURIComponent(config.filePath)}`
      );
      if (response.ok) {
        const fields: string[] = await response.json();
        setDbfColumns(fields);
        setMessage({
          type: 'success',
          text: `${fields.length} colunas carregadas do arquivo`,
        });
      } else {
        const error = await response.json();
        setMessage({
          type: 'error',
          text: error.error || 'Erro ao carregar campos',
        });
      }
    } catch (error) {
      setMessage({
        type: 'error',
        text: 'Erro ao carregar campos do arquivo',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSaveConfig = async () => {
    if (!config.codigoColumn || !config.descricaoColumn || !config.valorColumn) {
      setMessage({
        type: 'error',
        text: 'Código, Descrição e Valor são obrigatórios',
      });
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/dbf-config`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(config),
      });

      if (response.ok) {
        setMessage({
          type: 'success',
          text: 'Configuração salva com sucesso',
        });
      } else {
        const error = await response.json();
        setMessage({
          type: 'error',
          text: error.error || 'Erro ao salvar configuração',
        });
      }
    } catch (error) {
      setMessage({
        type: 'error',
        text: 'Erro ao salvar configuração',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleImport = async () => {
    if (!config.filePath) {
      setMessage({
        type: 'error',
        text: 'Configure o arquivo DBF primeiro',
      });
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/dbf-config/import`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      });

      if (response.ok) {
        const result: ImportResult = await response.json();
        setImportResult(result);
        setMessage({
          type: 'success',
          text: `Importação concluída: ${result.inserted} inseridos, ${result.updated} atualizados`,
        });
      } else {
        const error = await response.json();
        setMessage({
          type: 'error',
          text: error.error || 'Erro ao importar',
        });
      }
    } catch (error) {
      setMessage({
        type: 'error',
        text: 'Erro ao importar produtos',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleColumnChange = (field: keyof DbfConfig, value: string) => {
    setConfig(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const isValidConfig = config.codigoColumn && config.descricaoColumn && config.valorColumn;

  return (
    <div className="config-importacao-container">
      <div className="config-section">
        <h3>📂 Arquivo DBF</h3>
        <div className="file-select-row">
          <input
            type="text"
            value={config.filePath}
            onChange={e => setConfig(prev => ({ ...prev, filePath: e.target.value }))}
            placeholder="Selecione ou digite caminho completo do arquivo .dbf"
          />
          <button onClick={handleSelectFile} disabled={loading}>
            Selecionar
          </button>
          <button onClick={handleLoadFields} disabled={loading || !config.filePath}>
            {loading ? (
              <>
                <span className="spinner"></span>
                <span>Carregando...</span>
              </>
            ) : (
              'Carregar Campos'
            )}
          </button>
        </div>
        <input
          ref={fileInputRef}
          type="file"
          accept=".dbf"
          onChange={handleFileSelected}
          className="file-input-hidden"
        />
        {dbfColumns.length === 0 && config.filePath && (
          <div className="empty-columns-message">
            Clique em "Carregar Campos" para descobrir as colunas disponíveis
          </div>
        )}
      </div>

      <div className="config-section">
        <h3>🔗 Mapeamento de Colunas</h3>
        {dbfColumns.length > 0 ? (
          <div className="mapping-grid">
            <div className="mapping-field required">
              <label>Código</label>
              <select
                value={config.codigoColumn}
                onChange={e => handleColumnChange('codigoColumn', e.target.value)}
                disabled={loading}
              >
                <option value="">-- selecione --</option>
                {dbfColumns.map(col => (
                  <option key={col} value={col}>
                    {col}
                  </option>
                ))}
              </select>
            </div>

            <div className="mapping-field required">
              <label>Descrição</label>
              <select
                value={config.descricaoColumn}
                onChange={e => handleColumnChange('descricaoColumn', e.target.value)}
                disabled={loading}
              >
                <option value="">-- selecione --</option>
                {dbfColumns.map(col => (
                  <option key={col} value={col}>
                    {col}
                  </option>
                ))}
              </select>
            </div>

            <div className="mapping-field">
              <label>Categoria</label>
              <select
                value={config.categoriaColumn}
                onChange={e => handleColumnChange('categoriaColumn', e.target.value)}
                disabled={loading}
              >
                <option value="">-- (ignorar) --</option>
                {dbfColumns.map(col => (
                  <option key={col} value={col}>
                    {col}
                  </option>
                ))}
              </select>
            </div>

            <div className="mapping-field required">
              <label>Valor</label>
              <select
                value={config.valorColumn}
                onChange={e => handleColumnChange('valorColumn', e.target.value)}
                disabled={loading}
              >
                <option value="">-- selecione --</option>
                {dbfColumns.map(col => (
                  <option key={col} value={col}>
                    {col}
                  </option>
                ))}
              </select>
            </div>

            <div className="mapping-field">
              <label>Yield / Unidade</label>
              <select
                value={config.yieldColumn}
                onChange={e => handleColumnChange('yieldColumn', e.target.value)}
                disabled={loading}
              >
                <option value="">-- (ignorar) --</option>
                {dbfColumns.map(col => (
                  <option key={col} value={col}>
                    {col}
                  </option>
                ))}
              </select>
            </div>
          </div>
        ) : (
          <div className="empty-columns-message">
            Nenhuma coluna carregada. Selecione um arquivo DBF e clique em "Carregar Campos".
          </div>
        )}
      </div>

      <div className="config-section">
        <h3>⚙️ Ações</h3>
        <div className="button-group">
          <button
            className="btn-save"
            onClick={handleSaveConfig}
            disabled={loading || !config.filePath}
          >
            💾 Salvar Configuração
          </button>
          <button
            className="btn-import"
            onClick={handleImport}
            disabled={loading || !isValidConfig}
          >
            {loading ? (
              <>
                <span className="spinner"></span>
                <span className="loading-text">Importando...</span>
              </>
            ) : (
              '📥 Importar Agora'
            )}
          </button>
        </div>

        {importResult && (
          <div className="import-result">
            ✓ Importados: <strong>{importResult.inserted}</strong>, Atualizados:{' '}
            <strong>{importResult.updated}</strong>
          </div>
        )}
      </div>

      {message && (
        <div className={`message-toast ${message.type}`}>
          {message.type === 'success' ? '✓' : '✕'} {message.text}
        </div>
      )}
    </div>
  );
};

export default ConfigImportacaoScreen;
