# API & Frontend Demo

Complete workflow demonstration: Frontend UI → API calls → PDF generation

## Visual Demo

**GIF Recording:** `demo-workflow.gif` (17 frames)
- Products selection (4 items required)
- Layout Editor with Cards/Config tabs
- Element positioning and configuration
- Real-time API synchronization

---

## API Endpoints

Backend runs on `http://localhost:5274`

### 1. Get All Products

```bash
curl -X GET http://localhost:5274/api/produtos
```

**Response (200 OK):**
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
  ...
]
```

### 2. Search Products

```bash
curl -X GET "http://localhost:5274/api/produtos?search=cerveja"
```

**Response:** Filtered product list

### 3. Load Layout Configuration

```bash
curl -X GET http://localhost:5274/api/layout-config
```

**Response (200 OK):**
```json
{
  "cards": [
    {
      "id": "cebola",
      "x": 10,
      "y": 10,
      "w": 92.5,
      "h": 136,
      "content": {
        "title": {
          "x": 5,
          "y": 15,
          "text": "CEBOLA",
          "fontSize": 16,
          "bold": true,
          "alignment": "left",
          "color": "#000",
          "visible": true
        },
        "subtitle": {...},
        "price": {...},
        "unit": {...},
        "footer": {...}
      }
    }
  ],
  "pageMargin": 0,
  "gridColumns": 2,
  "gridRows": 2,
  "gridGapMm": 5
}
```

### 4. Save Layout Configuration

```bash
curl -X POST http://localhost:5274/api/layout-config \
  -H "Content-Type: application/json" \
  -d @layout-config.json
```

**Request Body:** Same structure as GET response

**Response (200 OK):**
```json
{
  "message": "Layout configurado com sucesso."
}
```

### 5. Generate PDF Preview

```bash
curl -X POST http://localhost:5274/api/print/preview \
  -H "Content-Type: application/json" \
  -d '{
    "productIds": [1, 2, 3, 4],
    "editedPrices": {}
  }'
```

**Response:** Base64-encoded PDF (binary data)

### 6. Confirm Print (Increment Quantities)

```bash
curl -X POST http://localhost:5274/api/print/confirm \
  -H "Content-Type: application/json" \
  -d '{
    "productIds": [1, 2, 3, 4]
  }'
```

**Response (200 OK):**
```json
{
  "message": "Impressão confirmada para 4 produtos."
}
```

### 7. Upload Image

```bash
curl -X POST http://localhost:5274/api/upload/image \
  -F "file=@image.jpg"
```

**Response (200 OK):**
```json
{
  "success": true,
  "filename": "a1b2c3d4-e5f6-4g7h-8i9j-k0l1m2n3o4p5.jpeg",
  "path": "/uploads/a1b2c3d4-e5f6-4g7h-8i9j-k0l1m2n3o4p5.jpeg"
}
```

### 8. Health Check

```bash
curl -X GET http://localhost:5274/health
```

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2026-04-29T18:30:00.000Z"
}
```

---

## Frontend Workflow

### 1. Produtos Screen
- **Location:** Sidebar → Produtos
- **Purpose:** Select products for printing
- **Validation:** Requires multiple of 4 items (4, 8, 12, 16, etc.)

**Steps:**
1. Search products (optional)
2. Filter by code, description, category, or initial letter
3. Check 4+ products
4. Click "Preview" to see PDF
5. Click "Imprimir" to print and confirm

**API Calls Made:**
- `GET /api/produtos` (on page load)
- `GET /api/produtos?search=X` (on search)
- `POST /api/print/preview` (on Preview click)
- `POST /api/print/confirm` (on Imprimir click)

### 2. Layout Editor Screen
- **Location:** Sidebar → Layout
- **Purpose:** Configure card positions and styles

**Cards Tab:**
- Edit individual product cards
- Adjust element positions (X, Y in mm)
- Configure text, font size, bold, alignment, color
- Upload images
- Drag elements on canvas to reposition

**Config Tab:**
- Page margin (default 0)
- Grid columns (default 2)
- Grid rows (default 2)
- Grid spacing in mm (default 5)

**API Calls Made:**
- `GET /api/layout-config` (on page load)
- `POST /api/layout-config` (on Salvar Layout click)
- `POST /api/upload/image` (on image upload)

### 3. PDF Preview Modal
- Shows generated PDF in modal window
- Buttons: Save PDF, Print, Close
- Uses layout configuration from backend

---

## Data Flow

```
Frontend (React/Electron)
    ↓
    → GET /api/produtos (fetch products)
    ↓
User selects 4+ products
    ↓
    → POST /api/print/preview (generate PDF)
    ↓
Backend (C#/.NET)
    ├─ Load layout-config.json
    ├─ Render PDF with QuestPDF
    ├─ Apply card positions, styles, images
    └─ Return base64 PDF
    ↓
Frontend displays PDF preview in modal
    ↓
User clicks Imprimir (Print)
    ↓
    → POST /api/print/confirm (confirm print)
    ↓
Backend increments quantidade_impressa
    ↓
Frontend shows success message
```

---

## Layout Configuration Example

**File:** `/app/data/layout-config.json`

```json
{
  "cards": [
    {
      "id": "cebola",
      "x": 10,
      "y": 10,
      "w": 92.5,
      "h": 136,
      "content": {
        "title": {
          "x": 5,
          "y": 15,
          "text": "CEBOLA",
          "fontSize": 16,
          "bold": true,
          "alignment": "left",
          "color": "#000",
          "visible": true
        }
      }
    }
  ],
  "pageMargin": 0,
  "gridColumns": 2,
  "gridRows": 2,
  "gridGapMm": 5
}
```

---

## Testing with Postman

Import requests:

```bash
# 1. Get products
GET http://localhost:5274/api/produtos

# 2. Load layout
GET http://localhost:5274/api/layout-config

# 3. Generate PDF
POST http://localhost:5274/api/print/preview
Header: Content-Type: application/json
Body: {
  "productIds": [1, 2, 3, 4],
  "editedPrices": {}
}

# 4. Confirm print
POST http://localhost:5274/api/print/confirm
Header: Content-Type: application/json
Body: {
  "productIds": [1, 2, 3, 4]
}
```

---

## Performance Metrics

- **Backend startup:** ~2-3 seconds
- **Product fetch:** ~100ms
- **PDF generation:** ~500-800ms
- **Frontend UI:** ~2-3 second total load

---

## Database

**File:** `/app/data/produtos.db` (SQLite)

**Table:** `Produtos`

```sql
CREATE TABLE Produtos (
  id INTEGER PRIMARY KEY,
  codigo TEXT NOT NULL,
  yield TEXT,
  descricao TEXT NOT NULL,
  categoria TEXT,
  valor DECIMAL(10,2),
  quantidade_impressa INTEGER DEFAULT 0
);
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| API not responding | Check backend running: `curl http://localhost:5274/health` |
| Product list empty | Seed database: Run backend with `--seed` flag |
| PDF generation fails | Check QuestPDF license (included in build) |
| Images not loading | Check `/uploads` folder exists and has write permissions |
| Layout not saving | Verify `/app/data` folder exists and is writable |

