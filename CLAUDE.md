# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Gerador de Ofertas MVP** — Desktop app to select products and print A4 posters (4 products/sheet).

Stack:
- **Frontend**: Electron + React + TypeScript
- **Backend**: .NET 8 API (local only)
- **Database**: SQLite
- **PDF**: QuestPDF

## Architecture

Three-layer desktop architecture:

```
┌─────────────────────────────────────────────────┐
│ Electron IPC Bridge                             │
│ (frontend ↔ backend HTTP/API)                   │
└─────────────────────────────────────────────────┘
         ↑                         ↑
    ┌────────────┐           ┌──────────────┐
    │ Frontend   │           │ Backend      │
    │ React/TS   │           │ .NET 8 API   │
    │ /app/front │           │ /app/backend │
    └────────────┘           └──────────────┘
         ↑                         ↑
    ┌────────────────────────────────────────┐
    │ SQLite Database                        │
    │ /app/data/produtos.db                  │
    └────────────────────────────────────────┘
```

### Frontend (`/app/frontend`)
- Electron main process (app initialization, window management)
- React components: products table, selection panel, PDF preview
- State: selected items (must be multiple of 4)
- HTTP calls to local backend (http://localhost:5274)

### Backend (`/app/backend`)
- .NET 8 API with SQLite
- Endpoints: GET /produtos, GET /produtos?search, POST /print/preview, POST /print/confirm
- Services: product queries, PDF generation via QuestPDF
- Database: table `produtos` with fields: id, codigo, yield, descricao, categoria, valor, quantidade_impressa

### PDF Generation Flow
1. Frontend sends selected product IDs + edited prices to `POST /print/preview`
2. Backend uses QuestPDF to render A4 (2×2 grid, 4 products/page)
3. Backend returns PDF (base64 or file path)
4. Frontend displays preview
5. On confirm, frontend calls `POST /print/confirm`
6. Backend increments `quantidade_impressa` for each product

## Build & Development Commands

### Backend (.NET 8)
```bash
# From /app/backend
dotnet build
dotnet run
dotnet test
dotnet publish -c Release
```

### Frontend (Electron)
```bash
# From /app/frontend
npm install
npm start        # dev with hot reload
npm run build    # production build
npm test         # run tests if added
npm run lint     # if linter configured
```

### Full Stack (local development)
```bash
# Terminal 1: Backend
cd app/backend && dotnet run
# Backend will listen on http://localhost:5274

# Terminal 2: Frontend (Electron dev server)
cd app/frontend && npm start
```

### Build Single Portable EXE (Distribution)
```bash
# From project root (requires .NET 8 + Node.js 18+)
.\build.ps1 -Configuration release

# Output: ./dist/Gerador de Ofertas.exe (~150-200 MB)
# Self-contained (no .NET installation required)
# Single file, portable, ready to distribute

# See docs/BUILD.md for detailed documentation
```

## Key Integration Points

**Frontend → Backend HTTP calls:**
- `GET http://localhost:5274/produtos` — fetch all products
- `GET http://localhost:5274/produtos?search=texto` — search products
- `POST http://localhost:5274/print/preview` — generate PDF preview
- `POST http://localhost:5274/print/confirm` — confirm print, increment quantities

**Selection validation:**
Only 4, 8, 12, 16... items allowed. Check in frontend:
```ts
const valid = selected.length > 0 && selected.length % 4 === 0;
```

**PDF A4 Layout (2×2 grid):**
```ts
const pagina = Math.floor(index / 4);
const pos = index % 4;
const linha = Math.floor(pos / 2);
const coluna = pos % 2;
```

## Design Reference

Use `paperclipai/paperclip` as visual reference:
- Sidebar (left navigation)
- White cards on light gray background
- Clean, minimal buttons with rounded corners
- Light drop shadow
- Modern typography

Do NOT copy Paperclip logo, brand, or text — reference only.

Layout structure:
```
┌──────────┬─────────────────┬─────────────┐
│ Sidebar  │ Main: Products  │ Panel:      │
│ Nav      │ Table + Search  │ Selected    │
│          │                 │ Preview     │
└──────────┴─────────────────┴─────────────┘
```

## Mandatory Rules (from AGENTS.md)

1. **Changelog requirement**: After ANY code change, update `docs/CHANGELOG.md`

   Format (required):
   ```md
   ## YYYY-MM-DD
   - autor: Claude Code | Codex | Humano
   - arquivo: path/to/file
   - ação: criação | alteração | correção | remoção
   - compilou: sim | não | N/A
   - testado: sim | não | N/A
   - observação: short text (optional)
   ```

2. **Minimal scope**: Implement only what's asked. No large refactors without necessity. Simplest code wins.

3. **Prohibited in MVP**: login, users, cloud sync, permissions, advanced template editor.

4. **Development priority**:
   1. SQLite database setup
   2. API /produtos endpoints
   3. UI product list
   4. Selection validation (multiple of 4)
   5. PDF A4 grid layout
   6. PDF preview
   7. Print/save
   8. Update quantidade_impressa

## Documentation Structure

- `docs/SPEC.md` — complete MVP specification (frozen, reference only)
- `docs/AGENTS.md` — rules for AI agents (read before implementing)
- `docs/CODEX_TASKS.md` — 8 sequential tasks (follow order)
- `docs/CHANGELOG.md` — change log (update after every change, required)

## Testing & Validation

MVP is ready when:
- Products appear in table
- Search works
- Selection enforces multiple of 4
- PDF preview renders correctly (2×2 grid, multiple pages)
- Print/save produces PDF file
- `quantidade_impressa` increments after print

## Common Issues

- **Backend not responding**: Check `localhost:5274/health` or check .NET 8 installation
- **Selection invalid**: Verify selection count % 4 === 0 before allowing preview
- **PDF blank**: Check QuestPDF setup in backend, verify product data passed to PDF service
- **Electron IPC blocked**: Ensure frontend waits for backend to start before making HTTP calls
