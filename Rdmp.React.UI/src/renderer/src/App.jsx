import Versions from './components/Versions'
import electronLogo from './assets/electron.svg'
import React, { useEffect, useState } from 'react';
function App() {
  //const ipcHandle = () => window.electron.ipcRenderer.send('ping')
  const [catalogues, setCatalogues] = useState(0);
  async function populateCatalogues() {
    const response = await fetch('http://localhost:5067/Catalogues');
    const data = await response.json();
    setCatalogues(data.length);
  }

  useEffect(() => {
    populateCatalogues();
  }, []);

  return (
    <>
      <img alt="logo" className="logo" src={electronLogo} />
      <div className="creator">Powered by electron-vite</div>
      <div className="text">
        Build an Electron app with <span className="react">React</span> - Found {catalogues} catalogues
      </div>
      <p className="tip">
        Please try pressing <code>F12</code> to open the devTool
      </p>
      <div className="actions">
        <div className="action">
          <a href="https://electron-vite.org/" target="_blank" rel="noreferrer">
            Documentation
          </a>
        </div>
      </div>
    </>
  )
}

export default App

