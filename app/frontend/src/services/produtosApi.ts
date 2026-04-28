export interface Produto {
  id: number;
  codigo: string;
  yield?: string | null;
  descricao: string;
  categoria?: string | null;
  valor: number;
  quantidadeImpresa?: number;
  quantidadeImpressa?: number;
}

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5274/api';

export async function getProdutos(search?: string, signal?: AbortSignal): Promise<Produto[]> {
  const query = search?.trim() ?? '';
  const params = new URLSearchParams();

  if (query.length > 0) {
    params.set('search', query);
  }

  const url = `${API_BASE_URL}/produtos${params.toString() ? `?${params.toString()}` : ''}`;
  const response = await fetch(url, { signal });

  if (!response.ok) {
    throw new Error(`Erro ao carregar produtos (${response.status})`);
  }

  return (await response.json()) as Produto[];
}

export async function previewPdf(produtoIds: number[], signal?: AbortSignal): Promise<Blob> {
  const response = await fetch(`${API_BASE_URL}/print/preview`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ ProdutoIds: produtoIds }),
    signal
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Erro ao gerar preview (${response.status})`);
  }

  return await response.blob();
}

export async function confirmPrint(produtoIds: number[]): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/print/confirm`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ ProdutoIds: produtoIds })
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Erro ao confirmar impressão (${response.status})`);
  }
}
