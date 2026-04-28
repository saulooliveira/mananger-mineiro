import React from 'react';
import '../styles/produtos.css';
import { Produto, getProdutos } from '../services/produtosApi';

function ProdutosScreen() {
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [produtos, setProdutos] = React.useState<Produto[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(search);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [search]);

  React.useEffect(() => {
    const controller = new AbortController();

    const loadProdutos = async () => {
      setLoading(true);
      setError(null);

      try {
        const data = await getProdutos(debouncedSearch, controller.signal);
        setProdutos(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') {
          return;
        }

        setError('Não foi possível carregar os produtos. Verifique se a API está rodando.');
      } finally {
        setLoading(false);
      }
    };

    void loadProdutos();

    return () => {
      controller.abort();
    };
  }, [debouncedSearch]);

  const formatarValor = (valor: number) => {
    return valor.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    });
  };

  const getQuantidadeImpressa = (produto: Produto) => {
    return produto.quantidadeImpressa ?? produto.quantidadeImpresa ?? 0;
  };

  return (
    <div className="produtos-screen">
      <div className="screen-header">
        <h2>Produtos</h2>
        <input
          type="text"
          placeholder="Buscar produtos..."
          className="search-input"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
      </div>
      <div className="produtos-content">
        {loading && (
          <div className="status-card loading-state">
            <p>Carregando produtos...</p>
          </div>
        )}

        {!loading && error && (
          <div className="status-card error-state">
            <p>{error}</p>
          </div>
        )}

        {!loading && !error && produtos.length === 0 && (
          <div className="status-card empty-state">
            <p>Nenhum produto encontrado.</p>
          </div>
        )}

        {!loading && !error && produtos.length > 0 && (
          <div className="table-wrapper">
            <table className="produtos-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Código</th>
                  <th>Descrição</th>
                  <th>Categoria</th>
                  <th>Valor</th>
                  <th>Quantidade Impressa</th>
                </tr>
              </thead>
              <tbody>
                {produtos.map((produto) => (
                  <tr key={produto.id}>
                    <td>{produto.id}</td>
                    <td>{produto.codigo}</td>
                    <td>{produto.descricao}</td>
                    <td>{produto.categoria || '-'}</td>
                    <td>{formatarValor(produto.valor)}</td>
                    <td>{getQuantidadeImpressa(produto)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default ProdutosScreen;
