import React from 'react';
import '../styles/historico.css';

interface PrintRecord {
  id: number;
  dataHora: string;
  produtoIds: string;
  numeroFolhas: number;
  quantidadeProdutos: number;
}

function HistoricoScreen() {
  const [historico, setHistorico] = React.useState<PrintRecord[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    fetchHistorico();
  }, []);

  const fetchHistorico = async () => {
    try {
      setLoading(true);
      const response = await fetch('http://localhost:5274/api/history');
      if (!response.ok) {
        throw new Error(`Erro ao carregar histórico (${response.status})`);
      }
      const data = await response.json();
      setHistorico(data);
      setError(null);
    } catch (err) {
      setError('Falha ao carregar histórico de impressões');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const formatData = (dataHora: string) => {
    const date = new Date(dataHora);
    return date.toLocaleString('pt-BR');
  };

  if (loading) {
    return <div className="historico-container">Carregando histórico...</div>;
  }

  return (
    <div className="historico-container">
      <h1>Histórico de Impressões</h1>

      {error && <div className="error-message">{error}</div>}

      {historico.length === 0 ? (
        <div className="empty-state">
          <p>Nenhuma impressão registrada ainda</p>
        </div>
      ) : (
        <table className="historico-table">
          <thead>
            <tr>
              <th>Data/Hora</th>
              <th>Produtos</th>
              <th>Quantidade</th>
              <th>Folhas</th>
            </tr>
          </thead>
          <tbody>
            {historico.map((record) => (
              <tr key={record.id}>
                <td>{formatData(record.dataHora)}</td>
                <td>{record.produtoIds}</td>
                <td>{record.quantidadeProdutos}</td>
                <td>{record.numeroFolhas}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <button className="btn-refresh" onClick={fetchHistorico}>
        🔄 Atualizar
      </button>
    </div>
  );
}

export default HistoricoScreen;
