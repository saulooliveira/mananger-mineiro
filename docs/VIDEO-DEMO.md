# Video Demo - Gerador de Ofertas

Complete walkthrough: Frontend UI + Backend API in action

## Prerequisites

Backend running:
```bash
cd app/backend
dotnet run
# Listening on http://localhost:5274
```

Frontend running:
```bash
cd app/frontend
npm start
# Listening on http://localhost:3000
```

---

## Demo Script

### Scene 1: API Health Check

**Terminal Command:**
```bash
curl -s http://localhost:5274/health | jq .
```

**Output:**
```json
{
  "status": "healthy",
  "timestamp": "2026-04-29T18:35:00.000Z"
}
```

✅ Backend is running and responsive

---

### Scene 2: Fetch Products from API

**Terminal Command:**
```bash
curl -s http://localhost:5274/api/produtos | jq '.[0:2]'
```

**Output (first 2 products):**
```json
[
  {
    "id": 1,
    "codigo": "0001",
    "descricao": "Cerveja Pilsen 350ml 0001",
    "categoria": "Hortifruti",
    "valor": 13.77,
    "quantidadeImpresa": 4
  },
  {
    "id": 2,
    "codigo": "0002",
    "descricao": "Arroz Branco 5kg 0002",
    "categoria": "Higiene",
    "valor": 26.76,
    "quantidadeImpresa": 4
  }
]
```

✅ API returns 1000+ products from database

---

### Scene 3: Open Frontend - Produtos Screen

**Browser:** Navigate to `http://localhost:3000`

**Screen shows:**
- Sidebar with 4 options: Produtos, Impressão, Histórico, Layout
- Product table with 10 items per page
- Search bar at top
- Filters: by código, descrição, categoria
- Initial letter filter (Todos, A-Z)
- Pagination (page 1 of 100)
- Right panel: "0 selecionados para impressão"

**User Action:** Click on search bar and type "cerveja"

**API Call (in Network tab):**
```
GET http://localhost:5274/api/produtos?search=cerveja
```

**Result:** Table filters to show only "Cerveja" products

---

### Scene 4: Select 4 Products

**User Action:** Check 4 products (rows 1, 2, 3, 4)

**Frontend shows:**
- Checkboxes get checked
- Right panel updates: "4 selecionados para impressão"
- Message: "4 selecionados. 1 folha pronta." (4 selected, 1 sheet ready)
- Preview button becomes BLUE (enabled)
- Imprimir button becomes active
- Page 1 section shows selected items with "Remover" button for each

---

### Scene 5: Click Preview Button

**User Action:** Click blue "Preview" button

**Browser Network Tab shows:**
```
POST http://localhost:5274/api/print/preview
Content-Type: application/json

{
  "productIds": [1, 2, 3, 4],
  "editedPrices": {}
}
```

**Backend Processing:**
1. Loads layout-config.json
2. Queries products from database
3. Generates PDF with QuestPDF
4. Returns base64-encoded PDF

**Frontend shows:**
- Modal window appears
- PDF preview renders (A4 page with 4 products in 2×2 grid)
- Buttons in modal: Save PDF, Print, Close

---

### Scene 6: Click Imprimir (Print)

**User Action:** Click "Imprimir" button in modal

**Browser Network Tab shows:**
```
POST http://localhost:5274/api/print/confirm
Content-Type: application/json

{
  "productIds": [1, 2, 3, 4]
}
```

**Backend Processing:**
1. Increments `quantidade_impressa` for each product
2. Updates SQLite database
3. Returns success message

**Frontend shows:**
- Success notification appears
- Modal closes
- Back at Produtos screen
- Selected items cleared
- "Quantidade Impressa" column updated for products 1-4 (now showing +1 from before)

---

### Scene 7: Navigate to Layout Editor

**User Action:** Click "Layout" in sidebar

**Screen shows:**
- "Editor de Layout" title at top
- "Salvar Layout" button (blue, top right)
- "Imprimir" button (green, top right)
- A4 canvas (210×297mm) with preview of card layout
- Single test card visible with: TESTE / Teste / 1,99 / UN

**API Call (automatic on page load):**
```
GET http://localhost:5274/api/layout-config
```

**Response:** Current layout configuration loaded and rendered

---

### Scene 8: Click Card to Show Configuration Panel

**User Action:** Click on card in canvas

**Right sidebar appears with:**

**Cards Tab (active):**
- "Configurar: test-card" heading
- Card list: test-card (selected, blue)
- Element editors for: Title, Subtitle, Price, Unit, Footer
- Each element shows:
  - Text input
  - Visibility toggle
  - Position (X mm, Y mm)
  - Font size (pt)
  - Bold toggle
  - Alignment dropdown (Esquerda, Centro, Direita)
  - Color picker
  - Image upload button ("Escolher arquivo")

---

### Scene 9: Edit Element Properties

**User Action:** Edit Title element

**Changes shown in real-time:**
- Type new text in "Texto:" field → Updates in canvas
- Change X value (5 → 20) → Element shifts right
- Change font size (16 → 24) → Text grows
- Toggle bold → Text becomes bold
- Change alignment (left → center) → Text recenters

**No API calls yet (local state changes)**

---

### Scene 10: Click Config Tab

**User Action:** Click "⚙️ Config" tab

**Shows:**
- "Configuração da Página" heading
- Settings form:
  - Margem da Página (mm): 0 (set to 0 for full-page layout)
  - Colunas do Grid: 2
  - Linhas do Grid: 2
  - Espaçamento do Grid (mm): 5

---

### Scene 11: Save Layout Configuration

**User Action:** Click "Salvar Layout" button

**Browser Network Tab shows:**
```
POST http://localhost:5274/api/layout-config
Content-Type: application/json

{
  "cards": [
    {
      "id": "test-card",
      "x": 20,
      "y": 10,
      "w": 90,
      "h": 130,
      "content": {
        "title": {
          "x": 20,
          "y": 15,
          "text": "[edited text]",
          "fontSize": 24,
          "bold": true,
          "alignment": "center",
          "color": "#000",
          "visible": true
        },
        ...
      }
    }
  ],
  "pageMargin": 0,
  "gridColumns": 2,
  "gridRows": 2,
  "gridGapMm": 5
}
```

**Backend:**
1. Saves layout-config.json to disk
2. Returns success message: "Layout configurado com sucesso."

**Frontend:**
- Success notification shows
- Configuration saved and persisted

---

### Scene 12: Return to Produtos and Print with New Layout

**User Action:** Click "Produtos" in sidebar

**Select 4 products again, click Preview**

**Backend Processing:**
1. Loads saved layout-config.json
2. Uses new card positions and styles
3. Generates PDF with updated layout
4. Returns PDF preview

**Frontend shows:**
- PDF preview reflects layout changes
- Card positions, font sizes, alignments match the layout editor

✅ Layout changes apply to PDF generation

---

### Scene 13: Upload Image to Card Element

**Back in Layout Editor → Cards Tab**

**User Action:**
1. Scroll down to find element with image upload
2. Click "Escolher arquivo" button
3. Select image file (e.g., product logo)
4. Browser shows file dialog

**Browser Network Tab shows:**
```
POST http://localhost:5274/api/upload/image
Content-Type: multipart/form-data

[binary image data]
```

**Response (200 OK):**
```json
{
  "success": true,
  "filename": "a1b2c3d4-e5f6-4g7h-8i9j-k0l1m2n3o4p5.jpeg",
  "path": "/uploads/a1b2c3d4-e5f6-4g7h-8i9j-k0l1m2n3o4p5.jpeg"
}
```

**Frontend:**
- Image path stored in element configuration
- Next PDF generation will include image

---

### Scene 14: API Performance Demo

**Terminal - Measure Response Times**

```bash
# Measure PDF generation time
time curl -X POST http://localhost:5274/api/print/preview \
  -H "Content-Type: application/json" \
  -d '{"productIds":[1,2,3,4],"editedPrices":{}}' \
  -o /tmp/output.pdf

# Output: real 0m0.687s (approximately 687ms)
```

**Network Tab shows:**
- Product fetch: ~50-100ms
- Layout load: ~10-20ms
- PDF generation: ~500-800ms
- Total request: ~600-850ms

---

### Scene 15: Database State Changes

**Terminal - Check Updated Database**

```bash
# Before print
sqlite3 app/data/produtos.db "SELECT id, descricao, quantidade_impressa FROM Produtos LIMIT 4;"

# Output:
# 1|Cerveja Pilsen 350ml 0001|4
# 2|Arroz Branco 5kg 0002|4
# 3|Cerveja Pilsen 350ml 0003|4
# 4|Frango Resfriado kg 0004|5
```

**After clicking Imprimir:**

```bash
sqlite3 app/data/produtos.db "SELECT id, descricao, quantidade_impressa FROM Produtos LIMIT 4;"

# Output (quantidade_impressa incremented by 1):
# 1|Cerveja Pilsen 350ml 0001|5
# 2|Arroz Branco 5kg 0002|5
# 3|Cerveja Pilsen 350ml 0003|5
# 4|Frango Resfriado kg 0004|6
```

✅ Database updates on print confirmation

---

## Complete Workflow Summary

```
1. Frontend loads products from API
   ↓ GET /api/produtos
   ↓
2. User selects 4 products and clicks Preview
   ↓ POST /api/print/preview
   ↓
3. Backend generates PDF with layout config
   ↓ GET /api/layout-config
   ↓
4. Frontend displays PDF preview modal
   ↓
5. User clicks Imprimir (Print)
   ↓ POST /api/print/confirm
   ↓
6. Backend increments quantidade_impressa
   ↓ UPDATE Produtos (SQLite)
   ↓
7. Frontend confirms print success
   ↓
8. User navigates to Layout Editor
   ↓ GET /api/layout-config
   ↓
9. User edits layout and clicks Salvar
   ↓ POST /api/layout-config
   ↓
10. Backend saves layout-config.json
    ↓
11. Next PDF generation uses new layout
```

---

## Key Features Demonstrated

✅ **Frontend:**
- React component state management
- Real-time filtering and search
- Modal dialogs
- Tab navigation
- Drag-and-drop element positioning
- Form validation (4-item minimum)

✅ **Backend:**
- RESTful API endpoints
- Database queries (EF Core + SQLite)
- PDF generation (QuestPDF)
- File uploads
- JSON configuration persistence
- Response time under 1 second

✅ **Integration:**
- HTTP communication
- Frontend-backend synchronization
- Real-time layout updates
- Configuration persistence
- Database transaction handling

---

## Files & Directories

```
app/
├── backend/
│   ├── Controllers/ (API endpoints)
│   ├── Services/ (Business logic)
│   ├── Data/ (Database context)
│   └── Properties/ (Configuration)
├── frontend/
│   ├── src/
│   │   ├── components/ (React components)
│   │   ├── services/ (API client)
│   │   └── styles/ (CSS)
│   └── public/ (Static assets)
└── data/
    ├── produtos.db (SQLite database)
    └── layout-config.json (Layout configuration)
```

---

## Debug Tips

**Backend Debug Mode:**
```bash
cd app/backend
dotnet run --environment Development

# Logs will show:
# - Database queries (EF Core logging)
# - API request details
# - PDF generation steps
# - File upload operations
```

**Frontend Debug Mode:**
```bash
cd app/frontend
npm start

# React DevTools available in browser
# Network tab shows API calls
# Console logs show component lifecycle
```

**Database Inspection:**
```bash
sqlite3 app/data/produtos.db

# List tables
.tables

# View produtos table
SELECT * FROM Produtos LIMIT 10;

# Check quantity changes
SELECT id, descricao, quantidade_impressa FROM Produtos ORDER BY quantidade_impressa DESC LIMIT 5;
```

