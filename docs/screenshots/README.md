# Screenshots - Gerador de Ofertas

Application interface screenshots showing key features and workflows.

## Screenshots

### 1. Produtos Screen (`01-produtos-screen.png`)
Product listing and selection interface.

**Features shown:**
- Sidebar navigation (Produtos, Impressão, Histórico, Layout)
- Product table with pagination
- Search bar and filters (código, descrição, categoria)
- Initial letter filter (Todos, A-Z)
- Checkboxes for product selection (multiple of 4 required)
- Product columns: ID, Código, Descrição, Categoria, Valor, Quantidade Impressa
- Pagination controls (page 1 of 100, 10 items per page)

### 2. Layout Editor Full View (`02-layout-full.png`)
Layout editor main interface without sidebar panel.

**Features shown:**
- Editor header with "Editor de Layout" title
- Save Layout button (blue, top right)
- Imprimir button (green, top right)
- A4 page canvas (210×297mm) showing single product card
- Card displays: TESTE (title), Teste (subtitle), 1,99 (price), UN (unit)
- Canvas is scrollable and zoomable

### 3. Layout Editor with Sidebar (`03-layout-with-sidebar.png`)
Wider viewport showing full layout with sidebar panel (when no card selected).

**Features shown:**
- Full sidebar on right side (width 320px)
- Config panel ready for interaction
- A4 canvas centered in viewport
- Single card with TESTE / Teste / 1,99 / UN

### 4. Layout Editor - Cards Tab (`04-layout-cards-tab-open.png`)
Layout editor with right sidebar showing Cards configuration tab.

**Features shown:**
- Cards tab selected (underlined in blue)
- Config tab available (shows gear icon)
- Card selection: "Configurar: test-card"
- Cards list showing "test-card" (selected, blue background)
- Element editors for:
  - **Title:** Text input (TESTE), visibility toggle, X/Y position, font size, bold toggle, alignment dropdown, color picker, image upload
  - **Subtitle:** Text input (Teste), visibility toggle, position/size/color controls
  - (Additional elements: price, unit, footer - scrollable panel)

### 5. Layout Editor - Config Tab (`05-layout-config-tab.png`)
Layout editor with right sidebar showing Config tab for page-level settings.

**Features shown:**
- Config tab selected (underlined in blue)
- Cards tab available
- "Configuração da Página" heading
- Settings controls:
  - **Margem da Página (mm):** Input field showing value 5
  - **Colunas do Grid:** Input field showing value 1
  - **Linhas do Grid:** Input field showing value 1
  - **Espaçamento do Grid (mm):** Input field showing value 0

## Key Interface Elements

### Sidebar Navigation
- Produtos (📦) - Product selection and printing
- Impressão (🖨️) - Print history
- Histórico (📋) - Document history
- Layout (🎨) - Layout editor (selected, light blue background)

### Layout Editor Tabs
- **Cards Tab:** Edit individual card elements (position, text, styling, images)
- **Config Tab:** Edit page-level configuration (margin, grid layout, spacing)

### Button Actions
- **Salvar Layout:** Save layout configuration to backend (POST /api/layout-config)
- **Imprimir:** Print preview to printer or PDF

## Responsive Design
- Application optimized for 1920×1080 viewport
- Sidebar scales with window width
- Canvas centered and scrollable
- Right panel (320px) collapses at smaller widths

## Data Workflow
1. Select products in Produtos screen (multiples of 4)
2. Click Layout to configure card layout and positioning
3. Use Cards tab to adjust individual element positions, sizes, colors
4. Use Config tab to adjust page margins and grid layout
5. Click Salvar Layout to persist changes
6. Next PDF generation will use new layout configuration

## Technology Stack
- React 18 (UI framework)
- TypeScript (type safety)
- Electron (desktop framework)
- CSS Grid & Flexbox (layout)
- Material Design inspired components

