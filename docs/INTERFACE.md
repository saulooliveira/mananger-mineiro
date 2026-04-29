# Gerador de Ofertas - Interface Documentation

## Overview

Desktop application for generating A4 product offer posters using Electron + React + TypeScript frontend with .NET 8 backend API.

---

## Application Screens

### 1. Produtos Screen

**Location:** Sidebar → Produtos

**Features:**
- Product listing with pagination (10 items/page)
- Search bar for quick product lookup
- Filter by product code (Filtrar por código)
- Filter by description (Filtrar por descrição)
- Filter by category (Filtrar por categoria)
- Initial letter filter (Todos, A-Z)
- Checkboxes for product selection (multiple of 4 required)
- Display columns: ID, Código, Descrição, Categoria, Valor, Quantidade Impressa
- Right panel showing:
  - Number of selected items
  - Validation message ("0 selecionados. Faltam 4.")
  - Preview button (disabled when no valid selection)
  - Salvar PDF button
  - Imprimir button (prints PDF directly)
  - Limpar seleção button (clear selection)

**Functionality:**
- Select products in multiples of 4
- Preview PDF before printing
- Save PDF to disk
- Print directly to printer
- Pagination controls (Anterior, Próxima)
- Shows "Mostrando 1 a 10 de 1000" for current view

---

### 2. Layout Editor Screen

**Location:** Sidebar → Layout

**Two-Tab Interface:**

#### Tab 1: Cards
**Purpose:** Configure individual product cards

**Card List:**
- List of all available cards (cebola, laranja, tomate, maracujá)
- Click to select/edit a card
- Selected card highlighted in blue

**Element Editors:**
For each element (title, subtitle, price, unit, footer), you can configure:

- **Texto:** Text content (CEBOLA, Nacional, 2,99, KG, etc.)
- **Visível:** Toggle visibility of element
- **Position** (when visible):
  - X (mm): Horizontal position on card
  - Y (mm): Vertical position on card
- **Style** (when visible):
  - Tamanho (pt): Font size in points
  - Negrito: Bold toggle
  - Alinhamento: left/center/right alignment
  - Cor: Color picker
  - Imagem: File upload for image (replaces text element)

**Drag-and-Drop Canvas:**
- Visual A4 page (210×297mm) with 2×2 card grid
- Click and drag element text to reposition
- Blue outline shows selected card
- Real-time coordinate updates

#### Tab 2: Config
**Purpose:** Configure page and grid layout

**Settings:**
- **Margem da Página (mm):** Page margin (default 10, set to 0 for full-page layout)
- **Colunas do Grid:** Number of columns (default 2)
- **Linhas do Grid:** Number of rows (default 2)
- **Espaçamento do Grid (mm):** Gap between cards (default 5)

**Actions:**
- **Salvar Layout:** Save current configuration to backend
- **Imprimir:** Print preview to printer or PDF

---

## Data Flow

### Product Selection to PDF

```
Produtos Screen
    ↓ (select 4+ items)
    ↓
Preview Button
    ↓ (click)
    ↓
POST /api/print/preview
    ↓
Backend: Uses LayoutConfig
    ↓
Returns PDF (base64)
    ↓
Modal displays preview
    ↓ (click Imprimir)
    ↓
Browser prints PDF
    ↓
POST /api/print/confirm (increments quantidade_impressa)
```

### Layout Configuration

```
Layout Editor → Cards Tab
    ↓ (edit card positions/styles)
    ↓
Layout Editor → Config Tab
    ↓ (adjust margins/grid)
    ↓
Salvar Layout Button
    ↓
POST /api/layout-config
    ↓
Backend: Saves to layout-config.json
    ↓
Next PDF generation uses new layout
```

---

## Key Features

### Selection Validation
- Enforces "multiple of 4" rule (4, 8, 12, 16, etc.)
- Shows validation message: "X selecionados. Faltam Y."
- Preview and print buttons disabled until valid

### Pagination & Filtering
- Products paginated at 10 per page
- Multiple simultaneous filters (code, description, category, initial letter)
- Search updates results in real-time
- "Mostando 1 a 10 de 1000" shows current range

### Visual Layout Editor
- A4 page (210×297mm) displayed at actual scale
- Drag-drop positioning with mm precision
- Absolute X,Y coordinates for each element
- Config persists via JSON file
- Load on editor open via GET /api/layout-config

### PDF Generation
- 2×2 card grid layout (4 products per A4 page)
- Uses LayoutConfig from backend (pageMargin, gridColumns, gridRows, gridGapMm)
- Supports text and image elements
- Images uploaded via file picker, saved to /uploads, referenced in PDF

### Merge of Settings & Layout Editor
- Single "Layout" screen combines both configuration and visual editor
- Tab interface (Cards ↔ Config) replaces separate SettingsScreen
- Unified configuration management

---

## API Endpoints (Backend: localhost:5274)

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | /api/produtos | List all products |
| GET | /api/produtos?search=X | Search products |
| GET | /api/layout-config | Load layout configuration |
| POST | /api/layout-config | Save layout configuration |
| POST | /api/print/preview | Generate PDF preview |
| POST | /api/print/confirm | Confirm print, update quantities |
| POST | /api/upload/image | Upload image file |
| GET | /health | Health check |

---

## Technology Stack

**Frontend:**
- Electron (desktop framework)
- React 18 (UI library)
- TypeScript (type safety)
- CSS (layout-editor.css, produtos.css, sidebar.css)

**Backend:**
- .NET 8 (API framework)
- Entity Framework Core (ORM)
- SQLite (database)
- QuestPDF (PDF generation)

**Data Persistence:**
- layout-config.json (layout configuration)
- uploads/ (user-uploaded images)
- produtos.db (SQLite database)

---

## User Workflows

### Print an Offer Sheet
1. Click **Produtos** in sidebar
2. Select 4 products (checkboxes)
3. Click **Preview** to see PDF
4. Click **Imprimir** to print (confirms print automatically)
5. Check **Quantidade Impressa** updates

### Customize Layout
1. Click **Layout** in sidebar
2. **Cards Tab:** Drag elements on canvas to reposition
3. **Cards Tab:** Edit text, font size, color, alignment per element
4. **Config Tab:** Adjust page margin, grid layout, spacing
5. Click **Salvar Layout** to persist changes
6. Next print uses new layout

### Add Image to Card Element
1. Go to **Layout** → **Cards Tab**
2. Select card and element
3. Click **Imagem: Escolher arquivo**
4. Upload image file (jpg, png, gif, bmp)
5. Image replaces text in PDF
6. Click **Salvar Layout** to persist

---

## Status Indicators

- **Sidebar:** Active screen highlighted in blue
- **Cards Tab/Config Tab:** Active tab underlined in blue
- **Product Row:** Selected rows have unchecked checkboxes
- **Card in Editor:** Selected card has blue border
- **Element in Editor:** Selected element has blue highlight

---

