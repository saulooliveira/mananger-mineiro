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
