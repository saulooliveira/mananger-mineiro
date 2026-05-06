import React from 'react';
import Sidebar from './components/Sidebar';
import ProdutosScreen from './components/ProdutosScreen';
import LayoutBuilder from './components/LayoutBuilder';
import HistoricoScreen from './components/HistoricoScreen';
import './App.css';

function App() {
  const [currentScreen, setCurrentScreen] = React.useState('produtos');

  return (
    <div className="app-container">
      <Sidebar currentScreen={currentScreen} setCurrentScreen={setCurrentScreen} />
      <div className="main-content">
        {currentScreen === 'produtos' && <ProdutosScreen />}
        {currentScreen === 'layout' && <LayoutBuilder />}
        {currentScreen === 'historico' && <HistoricoScreen />}
      </div>
    </div>
  );
}

export default App;
