# CHANGELOG

## 2026-04-28 (continued)
- autor: Claude Code
- arquivo: app/frontend/src/styles/layout-editor.css
- ação: criação
- compilou: sim
- testado: não
- observação: Layout editor stylesheet. A4 page canvas styling (210x297mm), card and element positioning, config panel layout, print styles.

- autor: Claude Code
- arquivo: app/frontend/src/App.tsx, app/frontend/src/components/Sidebar.tsx
- ação: alteração
- compilou: sim
- testado: não
- observação: Integrated LayoutEditor component. Added layout screen routing in App.tsx, added Layout button to Sidebar navigation.

- autor: Claude Code
- arquivo: app/backend/Controllers/LayoutConfigController.cs
- ação: criação
- compilou: sim
- testado: sim
- observação: Backend endpoint for saving/loading layout configuration. GET/POST /api/layout-config with JSON file persistence.

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: alteração
- compilou: sim
- testado: sim
- observação: Refactored to use layout configuration instead of grid-based layout. New LayoutBasedDocument class loads layout-config.json and applies to PDF generation.

- autor: Claude Code
- arquivo: app/frontend/src/components/LayoutEditor.tsx
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added save button to persist layout configuration to backend. Integrated with /api/layout-config endpoint on port 5274. Real-time synchronization of editor state to backend.

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: alteração (correção)
- compilou: sim
- testado: sim
- observação: Fixed layout positioning in PDF generation. Now uses X,Y coordinates from layout editor for element placement. RenderElement applies left padding and vertical spacing based on template values. Products selected in Produtos screen now render with layout-editor configuration.

- autor: Claude Code
- arquivo: app/frontend/src/components/LayoutEditor.tsx
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added useEffect hook to load saved layout from backend on component mount. Calls GET /api/layout-config and populates editor with persisted configuration. Layout editor now displays last saved configuration when screen opens.

- autor: Claude Code
- arquivo: app/backend/Controllers/UploadController.cs
- ação: criação
- compilou: sim
- testado: não
- observação: Image upload endpoint. POST /api/upload/image saves files to /uploads directory with GUID filenames. Validates image formats (jpg, png, gif, bmp).

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: alteração
- compilou: sim
- testado: não
- observação: Added imagePath field to Element class. Updated Compose to set page.Margin(0) for full-page layout. Cards now 105x148.5mm with no margins for exact PDF preview match.

- autor: Claude Code
- arquivo: app/frontend/src/components/LayoutEditor.tsx, app/frontend/src/styles/layout-editor.css, app/frontend/src/App.tsx, app/frontend/src/components/Sidebar.tsx, app/frontend/src/components/SettingsScreen.tsx
- ação: alteração (merge SettingsScreen into LayoutEditor)
- compilou: sim
- testado: sim
- observação: Merged SettingsScreen functionality into LayoutEditor via tab interface. Added activeTab state switching between Cards and Config tabs. Moved pageMargin, gridColumns, gridRows, gridGapMm configuration into Config tab. Removed SettingsScreen component and routes. Unified all configuration in single layout editor screen. Verified: GET/POST /api/layout-config endpoints working, config persistence working, tabs toggle correctly between Cards and Config sections.

- autor: Claude Code
- arquivo: app/backend/Program.cs
- ação: alteração
- compilou: sim
- testado: não
- observação: Added static file serving for uploads directory. StaticFileOptions configured to serve from /uploads path. Enables access to uploaded images in PDF.

- autor: Claude Code
- arquivo: app/frontend/src/components/LayoutEditor.tsx
- ação: alteração
- compilou: sim
- testado: não
- observação: Added image upload field to element editor. File input with accept=image/* triggers handleImageUpload. Updates element.imagePath with response from backend upload endpoint.

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: alteração
- compilou: sim
- testado: não
- observação: Added RenderImage method to support image rendering in PDF. RenderElement now checks imagePath and renders image instead of text if path exists. Images served from /uploads directory with MaxWidth constraint.

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

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx, app/frontend/src/styles/produtos.css
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added pagination (10 items/page) and filters for código, descrição, categoria. Client-side filtering with clear filters button, pagination controls with previous/next navigation.

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx, app/frontend/src/styles/produtos.css
- ação: alteração
- compilou: sim
- testado: sim
- observação: Added alphabetical initial letter filter (A-Z + Todos button). Filters products by first character of description. Responsive button layout, integrates with existing filters.

- autor: Claude Code
- arquivo: app/frontend/src/components/ProdutosScreen.tsx
- ação: alteração
- compilou: sim
- testado: sim
- observação: Removed separate confirm print button. Print button now automatically confirms print and increments quantidade_impressa. Simplified workflow with single action.

- autor: Claude Code
- arquivo: app/frontend/src/components/SettingsScreen.tsx, app/frontend/src/styles/settings.css, app/frontend/src/App.tsx, app/backend/Controllers/PdfConfigController.cs
- ação: criação
- compilou: sim
- testado: sim
- observação: Added PDF configuration settings screen. Visual form builder for page margin, grid layout (columns/rows/gap), element fonts (size + family). Backend endpoint saves/loads config from json file. Five configurable elements: title, description, price, unit, footer.

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: alteração
- compilou: sim
- testado: sim
- observação: Integrated PDF config into PrintService. Loads pdf-config.json, applies page margin, grid layout, and font sizes to PDF generation. Dynamic items per page based on config.

- autor: Claude Code
- arquivo: app/backend/Services/PrintService.cs
- ação: correção
- compilou: sim
- testado: sim
- observação: Cleaned PDF layout: removed title (Preview de Ofertas), total count, borders, and lines. Minimalist design with only grid spacing.
