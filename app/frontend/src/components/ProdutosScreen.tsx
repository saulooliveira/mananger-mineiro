import React from 'react';
import '../styles/produtos.css';
import { Produto, getProdutos, previewPdf, confirmPrint } from '../services/produtosApi';

function ProdutosScreen() {
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [produtos, setProdutos] = React.useState<Produto[]>([]);
  const [selecionados, setSelecionados] = React.useState<Produto[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = React.useState<string | null>(null);
  const [previewLoading, setPreviewLoading] = React.useState(false);
  const [previewError, setPreviewError] = React.useState<string | null>(null);
  const [confirmLoading, setConfirmLoading] = React.useState(false);
  const [confirmError, setConfirmError] = React.useState<string | null>(null);
  const [confirmSuccess, setConfirmSuccess] = React.useState<string | null>(null);

  React.useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  React.useEffect(() => {
    setPreviewUrl(null);
    setPreviewError(null);
    setConfirmError(null);
    setConfirmSuccess(null);
  }, [selecionados]);

  const handlePreview = async () => {
    if (!selecaoValida) {
      return;
    }

    setPreviewLoading(true);
    setPreviewError(null);

    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }

    try {
      const blob = await previewPdf(selecionados.map((produto) => produto.id));
      const url = URL.createObjectURL(blob);
      setPreviewUrl(url);
    } catch (err) {
      setPreviewError('Falha ao gerar o preview. Verifique a API e tente novamente.');
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleSave = () => {
    if (!selecaoValida || !previewUrl) {
      return;
    }

    const link = document.createElement('a');
    link.href = previewUrl;
    link.download = 'preview-ofertas.pdf';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handlePrint = () => {
    if (!selecaoValida || !previewUrl) {
      return;
    }

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      return;
    }

    printWindow.document.write(`<!DOCTYPE html><html><head><title>Imprimir Ofertas</title></head><body style="margin:0"><iframe src="${previewUrl}" style="border:none;width:100%;height:100vh;" onload="window.focus();window.print();"></iframe></body></html>`);
    printWindow.document.close();
  };

  const handleConfirmPrint = async () => {
    if (!selecaoValida || !previewUrl) {
      return;
    }

    setConfirmLoading(true);
    setConfirmError(null);
    setConfirmSuccess(null);

    try {
      await confirmPrint(selecionados.map((produto) => produto.id));
      setConfirmSuccess('Impressão confirmada. Quantidade impressa atualizada.');
      const refreshed = await getProdutos(debouncedSearch);
      setProdutos(refreshed);
    } catch (err) {
      setConfirmError('Falha ao confirmar impressão. Tente novamente.');
    } finally {
      setConfirmLoading(false);
    }
  };

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

  const selecionadosIds = React.useMemo(() => {
    return new Set(selecionados.map((produto) => produto.id));
  }, [selecionados]);

  const totalSelecionados = selecionados.length;
  const faltantes = totalSelecionados === 0 ? 4 : (4 - (totalSelecionados % 4)) % 4;
  const folhasProntas = Math.floor(totalSelecionados / 4);
  const selecaoValida = totalSelecionados > 0 && totalSelecionados % 4 === 0;

  const paginasSelecionadas = React.useMemo(() => {
    const paginas: Produto[][] = [];

    selecionados.forEach((produto, index) => {
      const pagina = Math.floor(index / 4);
      if (!paginas[pagina]) {
        paginas[pagina] = [];
      }

      paginas[pagina].push(produto);
    });

    return paginas;
  }, [selecionados]);

  const toggleProduto = (produto: Produto) => {
    setSelecionados((atuais) => {
      if (atuais.some((item) => item.id === produto.id)) {
        return atuais.filter((item) => item.id !== produto.id);
      }

      return [...atuais, produto];
    });
  };

  const removerSelecionado = (produtoId: number) => {
    setSelecionados((atuais) => atuais.filter((produto) => produto.id !== produtoId));
  };

  const limparSelecao = () => {
    setSelecionados([]);
  };

  const getStatusSelecao = () => {
    if (selecaoValida) {
      const folhasLabel = folhasProntas === 1 ? 'folha pronta' : 'folhas prontas';
      return `${totalSelecionados} selecionados. ${folhasProntas} ${folhasLabel}.`;
    }

    return `${totalSelecionados} selecionados. Faltam ${faltantes}.`;
  };

  return (
    <div className="produtos-screen">
      <div className="screen-header">
        <div>
          <h2>Produtos</h2>
          <p>{totalSelecionados} selecionados para impressão</p>
        </div>
        <div className="search-area">
          <input
            type="text"
            placeholder="Buscar produtos..."
            className="search-input"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </div>
      </div>

      <div className="produtos-layout">
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
                    <th className="checkbox-column">Selecionar</th>
                    <th>ID</th>
                    <th>Código</th>
                    <th>Descrição</th>
                    <th>Categoria</th>
                    <th>Valor</th>
                    <th>Quantidade Impressa</th>
                  </tr>
                </thead>
                <tbody>
                  {produtos.map((produto) => {
                    const selecionado = selecionadosIds.has(produto.id);

                    return (
                      <tr key={produto.id} className={selecionado ? 'selected-row' : ''}>
                        <td className="checkbox-column">
                          <input
                            type="checkbox"
                            checked={selecionado}
                            onChange={() => toggleProduto(produto)}
                            aria-label={`Selecionar ${produto.descricao}`}
                          />
                        </td>
                        <td>{produto.id}</td>
                        <td>{produto.codigo}</td>
                        <td>{produto.descricao}</td>
                        <td>{produto.categoria || '-'}</td>
                        <td>{formatarValor(produto.valor)}</td>
                        <td>{getQuantidadeImpressa(produto)}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <aside className="selecionados-panel">
          <div className="selection-summary">
            <div>
              <span>Selecionados</span>
              <strong>{totalSelecionados}</strong>
            </div>
            <div>
              <span>Folhas prontas</span>
              <strong>{folhasProntas}</strong>
            </div>
          </div>

          <div className={`selection-message ${selecaoValida ? 'valid' : 'invalid'}`}>
            {getStatusSelecao()}
          </div>

          <div className="preview-actions">
            <button
              type="button"
              className="preview-button"
              disabled={!selecaoValida || previewLoading}
              onClick={handlePreview}
            >
              {previewLoading ? 'Gerando preview...' : 'Preview'}
            </button>

            <button
              type="button"
              className="save-button"
              onClick={handleSave}
              disabled={!selecaoValida || !previewUrl}
            >
              Salvar PDF
            </button>

            <button
              type="button"
              className="print-button"
              onClick={handlePrint}
              disabled={!selecaoValida || !previewUrl}
            >
              Imprimir
            </button>

            <button
              type="button"
              className="confirm-button"
              onClick={handleConfirmPrint}
              disabled={!selecaoValida || !previewUrl || confirmLoading}
            >
              {confirmLoading ? 'Confirmando...' : 'Confirmar impressão'}
            </button>
          </div>

          <button
            type="button"
            className="clear-selection-button"
            onClick={limparSelecao}
            disabled={totalSelecionados === 0}
          >
            Limpar seleção
          </button>

          {previewError && <div className="preview-error">{previewError}</div>}
          {confirmError && <div className="preview-error">{confirmError}</div>}
          {confirmSuccess && <div className="preview-success">{confirmSuccess}</div>}
          {previewUrl && (
            <div className="preview-frame-wrapper">
              <iframe
                src={previewUrl}
                title="Preview de Ofertas"
                className="preview-frame"
              />
            </div>
          )}

          <div className="selected-pages">
            {paginasSelecionadas.length === 0 && (
              <p className="selected-empty">Nenhum produto selecionado.</p>
            )}

            {paginasSelecionadas.map((pagina, index) => (
              <div className="selected-page" key={`pagina-${index + 1}`}>
                <h3>Página {index + 1}</h3>
                <ul>
                  {pagina.map((produto) => (
                    <li key={produto.id}>
                      <div>
                        <strong>{produto.descricao}</strong>
                        <span>{produto.codigo} · {formatarValor(produto.valor)}</span>
                      </div>
                      <button
                        type="button"
                        onClick={() => removerSelecionado(produto.id)}
                        aria-label={`Remover ${produto.descricao}`}
                      >
                        Remover
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </aside>
      </div>
    </div>
  );
}

export default ProdutosScreen;
