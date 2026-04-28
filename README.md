# Gerador de Ofertas — Spec Pack

Projeto desktop Electron + React + .NET 8 para gerar ofertas de produtos e imprimir em PDF.

## Estrutura do repositório

- `app/backend` — API .NET 8 com SQLite e geração de PDF via QuestPDF
- `app/frontend` — Electron + React + TypeScript
- `docs` — especificação, regras de agente e registro de tarefas

## Requisitos

- .NET 8 SDK
- Node.js 18+ ou compatível
- npm

## Executar localmente

### Backend

1. Abra um terminal em `app/backend`
2. Restaurar dependências:
   ```powershell
   dotnet restore
   ```
3. Executar API:
   ```powershell
   dotnet run
   ```

A API roda em `http://localhost:5000` ou em outra porta configurada pelo .NET.

### Frontend

1. Abra um terminal em `app/frontend`
2. Instalar dependências:
   ```powershell
   npm install
   ```
3. Iniciar em modo dev:
   ```powershell
   npm run dev
   ```

O Electron deve abrir a aplicação desktop e conectar à API local.

### Executar toda a aplicação com script shell

Se você já gerou o pacote do Electron e existe um `.exe` em `app/frontend/dist`, o script PowerShell abrirá o executável automaticamente.

No Linux/macOS ou em um ambiente com Bash, use o script na raiz do projeto:

```bash
./run-full.sh
```

No Windows PowerShell, use o wrapper PowerShell:

```powershell
.\run-full.ps1
```

Se o executável não estiver disponível, o PowerShell fará fallback para o modo de desenvolvimento com `npm run dev`.

## Build

### Backend

```powershell
cd app/backend
dotnet build
```

### Frontend

```powershell
cd app/frontend
npm run build
```

## Endpoints principais

- `GET /api/produtos` — lista todos os produtos
- `GET /api/produtos?search=texto` — busca produtos
- `POST /api/print/preview` — gera preview PDF com IDs enviados
- `POST /api/print/confirm` — confirma impressão e incrementa `quantidadeImpressa`

## Documentação de tarefas

- `docs/CODEX_TASKS.md` — sequência de tarefas MVP
- `docs/CHANGELOG.md` — registro obrigatório de alterações
- `docs/SPEC.md` — especificação completa
- `docs/AGENTS.md` — regras para revisão por IA

## Observações

- O seed de teste gera 1000 produtos apenas em build Debug do backend
- Mantenha o `docs/CHANGELOG.md` atualizado após qualquer modificação no código
