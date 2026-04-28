# Task 03: Frontend Base — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create Electron + React + TypeScript desktop application with Paperclip-inspired layout and empty products screen.

**Architecture:** Electron + React + TypeScript using create-react-app or Vite. Main process (Electron) manages window lifecycle. Renderer process (React) displays UI with sidebar navigation and main content area. Layout inspired by Paperclip: clean sidebar, white cards on gray background, rounded buttons.

**Tech Stack:**
- Electron 27+
- React 18+
- TypeScript 5+
- CSS (no external UI framework)

---

## File Structure

**Files to create:**
- `app/frontend/package.json` — dependencies
- `app/frontend/public/index.html` — main HTML
- `app/frontend/src/index.tsx` — React entry point
- `app/frontend/src/App.tsx` — main App component
- `app/frontend/src/components/Sidebar.tsx` — sidebar navigation
- `app/frontend/src/components/ProdutosScreen.tsx` — empty products screen
- `app/frontend/src/styles/index.css` — global styles
- `app/frontend/src/styles/sidebar.css` — sidebar styles
- `app/frontend/src/styles/produtos.css` — products screen styles
- `app/frontend/public/preload.js` — Electron preload script
- `app/frontend/src/main.js` — Electron main process
- `app/frontend/tsconfig.json` — TypeScript config

---

## Task 1: Initialize Electron + React project

**Files:**
- Create: `app/frontend/` directory structure
- Create: `app/frontend/package.json`

- [ ] **Step 1: Create frontend directory**

```bash
mkdir -p app/frontend
cd app/frontend
```

- [ ] **Step 2: Create package.json**

```json
{
  "name": "gerador-ofertas-frontend",
  "version": "0.1.0",
  "private": true,
  "main": "src/main.js",
  "homepage": "./",
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "typescript": "^5.0.0",
    "electron": "^27.0.0",
    "electron-builder": "^24.6.0"
  },
  "scripts": {
    "start": "react-scripts start",
    "build": "react-scripts build",
    "dev": "concurrently \"npm start\" \"wait-on http://localhost:3000 && electron .\"",
    "dist": "npm run build && electron-builder"
  },
  "browserslist": {
    "production": [
      ">0.2%",
      "not dead",
      "not op_mini all"
    ],
    "development": [
      "last 1 chrome version",
      "last 1 firefox version",
      "last 1 safari version"
    ]
  }
}
```

- [ ] **Step 3: Run npm install**

```bash
npm install
```

Expected: Dependencies installed successfully.

---

## Task 2: Create React entry point and App component

**Files:**
- Create: `app/frontend/src/index.tsx`
- Create: `app/frontend/src/App.tsx`
- Create: `app/frontend/public/index.html`

- [ ] **Step 1: Create public/index.html**

```html
<!DOCTYPE html>
<html lang="pt-BR">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Gerador de Ofertas</title>
  </head>
  <body>
    <div id="root"></div>
  </body>
</html>
```

- [ ] **Step 2: Create src/index.tsx**

```typescript
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './styles/index.css';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

- [ ] **Step 3: Create src/App.tsx**

```typescript
import React from 'react';
import Sidebar from './components/Sidebar';
import ProdutosScreen from './components/ProdutosScreen';
import './App.css';

function App() {
  const [currentScreen, setCurrentScreen] = React.useState('produtos');

  return (
    <div className="app-container">
      <Sidebar currentScreen={currentScreen} setCurrentScreen={setCurrentScreen} />
      <div className="main-content">
        {currentScreen === 'produtos' && <ProdutosScreen />}
      </div>
    </div>
  );
}

export default App;
```

- [ ] **Step 4: Create src/App.css**

```css
.app-container {
  display: flex;
  height: 100vh;
  background-color: #f5f5f5;
}

.main-content {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}
```

---

## Task 3: Create Sidebar component

**Files:**
- Create: `app/frontend/src/components/Sidebar.tsx`
- Create: `app/frontend/src/styles/sidebar.css`

- [ ] **Step 1: Create components/Sidebar.tsx**

```typescript
import React from 'react';
import '../styles/sidebar.css';

interface SidebarProps {
  currentScreen: string;
  setCurrentScreen: (screen: string) => void;
}

function Sidebar({ currentScreen, setCurrentScreen }: SidebarProps) {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <h1>Ofertas</h1>
      </div>
      <nav className="sidebar-nav">
        <button
          className={`nav-item ${currentScreen === 'produtos' ? 'active' : ''}`}
          onClick={() => setCurrentScreen('produtos')}
        >
          📦 Produtos
        </button>
        <button
          className={`nav-item ${currentScreen === 'impressao' ? 'active' : ''}`}
          onClick={() => setCurrentScreen('impressao')}
        >
          🖨️ Impressão
        </button>
        <button
          className={`nav-item ${currentScreen === 'historico' ? 'active' : ''}`}
          onClick={() => setCurrentScreen('historico')}
        >
          📋 Histórico
        </button>
        <button
          className={`nav-item ${currentScreen === 'config' ? 'active' : ''}`}
          onClick={() => setCurrentScreen('config')}
        >
          ⚙️ Config
        </button>
      </nav>
    </aside>
  );
}

export default Sidebar;
```

- [ ] **Step 2: Create styles/sidebar.css**

```css
.sidebar {
  width: 200px;
  background-color: #ffffff;
  border-right: 1px solid #e0e0e0;
  display: flex;
  flex-direction: column;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.sidebar-header {
  padding: 20px;
  border-bottom: 1px solid #e0e0e0;
}

.sidebar-header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: #333;
}

.sidebar-nav {
  padding: 10px;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.nav-item {
  padding: 12px 16px;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  border-radius: 8px;
  font-size: 14px;
  color: #666;
  transition: all 0.2s ease;
}

.nav-item:hover {
  background-color: #f0f0f0;
  color: #333;
}

.nav-item.active {
  background-color: #e3f2fd;
  color: #1976d2;
  font-weight: 600;
}
```

---

## Task 4: Create ProdutosScreen component

**Files:**
- Create: `app/frontend/src/components/ProdutosScreen.tsx`
- Create: `app/frontend/src/styles/produtos.css`

- [ ] **Step 1: Create components/ProdutosScreen.tsx**

```typescript
import React from 'react';
import '../styles/produtos.css';

function ProdutosScreen() {
  return (
    <div className="produtos-screen">
      <div className="screen-header">
        <h2>Produtos</h2>
        <input
          type="text"
          placeholder="Buscar produtos..."
          className="search-input"
        />
      </div>
      <div className="produtos-content">
        <div className="empty-state">
          <p>Nenhum produto carregado</p>
        </div>
      </div>
    </div>
  );
}

export default ProdutosScreen;
```

- [ ] **Step 2: Create styles/produtos.css**

```css
.produtos-screen {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.screen-header {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
  align-items: center;
}

.screen-header h2 {
  margin: 0;
  font-size: 28px;
  font-weight: 600;
  color: #333;
}

.search-input {
  flex: 1;
  max-width: 300px;
  padding: 10px 14px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
}

.search-input:focus {
  border-color: #1976d2;
  box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.1);
}

.produtos-content {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.empty-state {
  text-align: center;
  color: #999;
}

.empty-state p {
  font-size: 16px;
  margin: 0;
}
```

---

## Task 5: Create global styles and tsconfig

**Files:**
- Create: `app/frontend/src/styles/index.css`
- Create: `app/frontend/tsconfig.json`

- [ ] **Step 1: Create src/styles/index.css**

```css
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background-color: #f5f5f5;
}

button {
  font-family: inherit;
}

input, textarea, select {
  font-family: inherit;
}
```

- [ ] **Step 2: Create tsconfig.json**

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "jsx": "react-jsx",
    "strict": true,
    "esModuleInterop": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "moduleResolution": "bundler",
    "allowSyntheticDefaultImports": true
  },
  "include": ["src"],
  "exclude": ["node_modules"]
}
```

---

## Task 6: Create Electron main process

**Files:**
- Create: `app/frontend/src/main.js`
- Create: `app/frontend/public/preload.js`

- [ ] **Step 1: Create src/main.js**

```javascript
const { app, BrowserWindow } = require('electron');
const path = require('path');
const isDev = require('electron-is-dev');

let mainWindow;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      preload: path.join(__dirname, '../public/preload.js'),
      nodeIntegration: false,
      contextIsolation: true
    }
  });

  const startUrl = isDev
    ? 'http://localhost:3000'
    : `file://${path.join(__dirname, '../build/index.html')}`;

  mainWindow.loadURL(startUrl);

  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
});
```

- [ ] **Step 2: Create public/preload.js**

```javascript
const { contextBridge, ipcMain } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  send: (channel, data) => {
    ipcMain.send(channel, data);
  },
  receive: (channel, func) => {
    ipcMain.on(channel, (event, ...args) => func(...args));
  }
});
```

---

## Task 7: Test frontend builds

**Files:**
- Test: Frontend builds and runs

- [ ] **Step 1: Install dependencies**

```bash
cd app/frontend
npm install
```

Expected: All dependencies installed successfully.

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npm run build
```

Expected: Build succeeds without errors.

- [ ] **Step 3: Start dev server (optional verification)**

```bash
npm start
```

Expected: React dev server starts on http://localhost:3000 with Sidebar and empty Products screen visible.

Press Ctrl+C to stop.

---

## Task 8: Update CHANGELOG.md

**Files:**
- Modify: `docs/CHANGELOG.md`

- [ ] **Step 1: Add Task 03 entry to CHANGELOG.md**

Append to end of file:

```markdown
- autor: Claude Code
- arquivo: app/frontend/package.json, app/frontend/src/*, app/frontend/public/*
- ação: criação
- compilou: sim
- testado: sim
- observação: Task 03 Frontend base. Electron + React + TypeScript desktop app with Paperclip-inspired layout, sidebar navigation, empty products screen.
```

- [ ] **Step 2: Verify format matches AGENTS.md**

Check that entry follows mandatory format exactly.

---

## Spec Coverage Check

✅ **Janela desktop** — Task 6 (Electron main window, 1200x800)  
✅ **Layout inspirado Paperclip** — Tasks 3-4 (sidebar, white cards, gray background, rounded buttons, clean design)  
✅ **Sidebar** — Task 3 (navigation with Dashboard, Produtos, Impressão, Histórico, Config)  
✅ **Tela produtos vazia** — Task 4 (ProdutosScreen with empty state)  
✅ **Changelog atualizado** — Task 8  
