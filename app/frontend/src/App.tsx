import React from 'react';
import Sidebar from './components/Sidebar';
import ProdutosScreen from './components/ProdutosScreen';
import LayoutEditor from './components/LayoutEditor';
import './App.css';

function App() {
  const [currentScreen, setCurrentScreen] = React.useState('produtos');

  return (
    <div className="app-container">
      <Sidebar currentScreen={currentScreen} setCurrentScreen={setCurrentScreen} />
      <div className="main-content">
        {currentScreen === 'produtos' && <ProdutosScreen />}
        {currentScreen === 'layout' && <LayoutEditor />}
      </div>
    </div>
  );
}

export default App;
