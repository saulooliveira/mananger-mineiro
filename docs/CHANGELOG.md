# CHANGELOG

## 2026-04-28
- autor: ChatGPT
- arquivo: docs/SPEC.md
- ação: criação da especificação MVP
- compilou: N/A
- testado: N/A

- autor: ChatGPT
- arquivo: docs/AGENTS.md
- ação: criação das regras para agentes IA
- compilou: N/A
- testado: N/A

- autor: ChatGPT
- arquivo: docs/CODEX_TASKS.md
- ação: criação das tarefas iniciais para Codex/Claude Code
- compilou: N/A
- testado: N/A

- autor: Claude Code
- arquivo: app/backend/*
- ação: criação
- compilou: sim
- testado: sim
- observação: Backend base Task 01. Project scaffold, EF Core SQLite, health check, produtos table, optional seed.

- autor: Claude Code
- arquivo: app/backend/Services/ProdutoService.cs, app/backend/Controllers/ProdutosController.cs, app/backend/Program.cs
- ação: criação
- compilou: sim
- testado: sim
- observação: Task 02 Produtos API. GET /api/produtos and GET /api/produtos/search endpoints with service layer.

- autor: Claude Code
- arquivo: app/frontend/package.json, app/frontend/src/*, app/frontend/public/*
- ação: criação
- compilou: sim
- testado: sim
- observação: Task 03 Frontend base. Electron + React + TypeScript desktop app with Paperclip-inspired layout, sidebar navigation, empty products screen.

- autor: Codex
- arquivo: app/backend/Controllers/ProdutosController.cs
- ação: alteração
- compilou: não
- testado: não
- observação: Task 04. GET /api/produtos agora aceita query `search` para busca no mesmo endpoint.

- autor: Codex
- arquivo: app/backend/Program.cs
- ação: alteração
- compilou: não
- testado: não
- observação: Task 04. Adicionada política CORS para permitir integração frontend local em desenvolvimento.

- autor: Codex
- arquivo: app/frontend/src/services/produtosApi.ts
- ação: criação
- compilou: sim
- testado: não
- observação: Task 04. Serviço HTTP para listar e buscar produtos via API.

- autor: Codex
- arquivo: app/frontend/src/components/ProdutosScreen.tsx
- ação: alteração
- compilou: sim
- testado: não
- observação: Task 04. Integração da tela de produtos com API, busca com debounce e estados de loading/erro.

- autor: Codex
- arquivo: app/frontend/src/styles/produtos.css
- ação: alteração
- compilou: sim
- testado: não
- observação: Task 04. Estilos de tabela de produtos e estados visuais de loading/erro/vazio.

- autor: Claude Code
- arquivo: app/backend/Program.cs
- ação: alteração
- compilou: não
- testado: não
- observação: Task 09. Seed de 1000 produtos gerados aleatoriamente em DEBUG.

- autor: GitHub Copilot
- arquivo: app/frontend/package.json, app/frontend/package-lock.json, app/frontend/public/electron.js
- ação: alteração
- compilou: N/A
- testado: não
- observação: Fix Electron packaging by adding public/electron.js and updating frontend build metadata for react-cra electron-builder.

- autor: Claude Code
- arquivo: app/backend/Controllers/PrintController.cs, app/backend/Services/PrintService.cs, app/backend/Services/ProdutoService.cs, app/backend/backend.csproj, app/backend/Program.cs
- ação: alteração
- compilou: sim
- testado: não
- observação: Task 06. Implementado PDF A4 QuestPDF com grid 2x2 e endpoint de preview.

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx, app/frontend/src/services/produtosApi.ts, app/frontend/src/styles/produtos.css
- ação: alteração
- compilou: sim
- testado: sim
- observação: Task 07. Implementado preview PDF, botão salvar e botão imprimir.

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx, app/frontend/src/services/produtosApi.ts
- ação: alteração
- compilou: sim
- testado: sim
- observação: Task 08. Implementado confirmar impressão, endpoint de confirmação e atualização de quantidade impressa.

- autor: Codex
- arquivo: app/frontend/src/components/ProdutosScreen.tsx, app/frontend/src/styles/produtos.css
- ação: alteração
- compilou: sim
- testado: sim
- observação: Task 05. Seleção por checkbox, painel de selecionados, mensagens de faltantes e preview desabilitado quando inválido.

- autor: Codex
- arquivo: .gitignore
- ação: alteração
- compilou: N/A
- testado: sim
- observação: Ajustado ignore para dependências, builds, bin/obj e bancos locais; removidos artefatos gerados do índice com git rm --cached.

- autor: Claude Code
- arquivo: app/frontend/src/main.js, app/frontend/package.json
- ação: correção
- compilou: sim
- testado: sim
- observação: Fixed Electron module loading. Replace electron-is-dev with process.env.NODE_ENV check (CommonJS compatible). Update preload path for packaged builds using process.resourcesPath.

- autor: Claude Code
- arquivo: app/frontend/package.json
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added electron-builder config: explicit file inclusion (src/main.js, public/, build/), Windows targets (portable + NSIS). Fixes packaged app module resolution.

- autor: Claude Code
- arquivo: app/backend/Program.cs
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added QuestPDF Community license configuration to disable license validation exception at startup.

- autor: Claude Code
- arquivo: app/frontend/src/components/PdfModal.tsx, app/frontend/src/styles/pdf-modal.css, app/frontend/src/components/ProdutosScreen.tsx
- ação: criação
- compilou: sim
- testado: sim
- observação: Added PDF preview modal. Modal opens when clicking preview button, displays PDF with save/print/close actions. Removed inline iframe from sidebar.

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx
- ação: correção
- compilou: sim
- testado: sim
- observação: Fixed print button to print PDF directly using native browser PDF viewer instead of HTML wrapper.
