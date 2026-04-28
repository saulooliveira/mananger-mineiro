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
