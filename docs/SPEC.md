# SPEC — Gerador de Ofertas MVP

## Objetivo
App desktop local para selecionar produtos e imprimir cartazes A4 com 4 produtos por folha.

## Referência visual
Design inspirado no projeto `paperclipai/paperclip`:
- dashboard limpo
- sidebar esquerda
- cards brancos
- fundo cinza claro
- painel central produtivo
- painel lateral de seleção
- botões simples
- bordas arredondadas
- sombra leve
- tipografia moderna

Não copiar marca, logo ou textos do Paperclip. Usar apenas como referência de UX/layout.

## Stack
Frontend:
- Electron
- React
- TypeScript

Backend:
- .NET 8 API local

DB:
- SQLite

PDF:
- QuestPDF

## Escopo MVP
Inclui:
- listar produtos
- buscar produtos
- selecionar via checkbox
- validar múltiplo de 4
- preview PDF A4
- imprimir/salvar PDF
- atualizar quantidade impressa

Não inclui:
- login
- usuário
- cloud
- multiempresa
- permissões
- templates avançados

## Banco
Tabela obrigatória: `produtos`

Campos:
- `id` int PK autoincrement
- `codigo` text
- `yield` text
- `descricao` text
- `categoria` text
- `valor` real
- `quantidade_impressa` int default 0

```sql
CREATE TABLE produtos (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  codigo TEXT NOT NULL,
  yield TEXT,
  descricao TEXT NOT NULL,
  categoria TEXT,
  valor REAL NOT NULL,
  quantidade_impressa INTEGER NOT NULL DEFAULT 0
);
```

## Regra principal
Impressão só é válida se quantidade selecionada for múltiplo de 4.

Válido:
- 4 = 1 folha
- 8 = 2 folhas
- 12 = 3 folhas

Inválido:
- 1,2,3,5,6,7,9,10,11

```ts
const valido = selecionados.length > 0 && selecionados.length % 4 === 0;
```

## Fluxo
1. Abrir app
2. Carregar produtos SQLite
3. Buscar/filtrar
4. Selecionar produtos
5. Mostrar status seleção
6. Habilitar preview se válido
7. Gerar PDF
8. Visualizar PDF
9. Imprimir/salvar
10. Incrementar `quantidade_impressa`

## Layout UI
Estrutura desktop:

```txt
┌────────────┬────────────────────────────┬─────────────────┐
│ Sidebar    │ Produtos                   │ Selecionados    │
│            │ Busca                      │ Status          │
│ Dashboard  │ Lista                      │ Página 1        │
│ Produtos   │ Checkbox Código Desc Valor │ Itens 1..4      │
│ Impressão  │                            │ Preview         │
│ Histórico  │                            │                 │
│ Config     │                            │                 │
└────────────┴────────────────────────────┴─────────────────┘
```

## Tela Produtos
Colunas:
- checkbox
- id
- codigo
- descricao
- categoria
- valor
- quantidade_impressa

Ações:
- selecionar
- remover seleção
- limpar seleção
- editar preço antes de imprimir

## Painel Selecionados
Mostrar:
- quantidade selecionada
- folhas prontas
- produtos faltantes
- lista agrupada por página

Mensagens:
- `2 selecionados. Faltam 2.`
- `4 selecionados. 1 folha pronta.`
- `6 selecionados. Faltam 2.`
- `8 selecionados. 2 folhas prontas.`

## Preview PDF
Formato:
- A4 retrato
- 2 colunas
- 2 linhas
- 4 produtos por página

Cada bloco:
- descrição uppercase
- preço grande
- unidade/yield se aplicável
- categoria opcional
- logo/nome loja opcional

## Cálculo grid
```ts
const pagina = Math.floor(index / 4);
const pos = index % 4;
const linha = Math.floor(pos / 2);
const coluna = pos % 2;
```

## API mínima
`GET /produtos`

`GET /produtos?search=texto`

`POST /print/preview`
- body: ids produtos + valores editados
- retorno: PDF/base64 ou caminho arquivo

`POST /print/confirm`
- body: ids produtos
- ação: incrementa quantidade impressa

## Atualizar impressão
```sql
UPDATE produtos
SET quantidade_impressa = quantidade_impressa + 1
WHERE id IN (...);
```

## Estrutura sugerida
```txt
/app
  /frontend
    /src
      /components
      /pages
      /services
      /styles
  /backend
    /Controllers
    /Services
    /Models
    /Data
/docs
  SPEC.md
  CHANGELOG.md
  AGENTS.md
  CODEX_TASKS.md
```

## Regras para IA
Toda alteração deve atualizar `docs/CHANGELOG.md`.

Cada item no changelog deve conter:
- data
- autor: Codex, Claude Code ou humano
- arquivo alterado
- ação
- compilou: sim/não/N/A
- testado: sim/não/N/A

## Critério de pronto
MVP pronto quando:
- produtos aparecem
- busca funciona
- seleção múltiplo de 4 funciona
- preview PDF correto
- impressão/salvar PDF funciona
- quantidade_impressa incrementa
