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
