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
