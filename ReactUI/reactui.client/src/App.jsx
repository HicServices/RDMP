import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [catalogues, setCatalogues] = useState();

    useEffect(() => {
        populateCatalogues();
    }, []);

    const contents = catalogues === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <table className="table table-striped" aria-labelledby="tabelLabel">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th>ID</th>
                </tr>
            </thead>
            <tbody>
                {catalogues.map(catalogue =>
                    <tr key={catalogue.ID}>
                        <td>{catalogue.name}</td>
                        <td>{catalogue.description || 'no description set'}</td>
                        <td>{catalogue.id}</td>
                    </tr>
                )}
            </tbody>
        </table>;
    return (
        <div>
            <h1 id="tabelLabel">Catalogues</h1>
            <p>This component demonstrates fetching data from the server.</p>
            {contents}
        </div>
    );
    
    async function populateCatalogues() {
        const response = await fetch('catalogue');
        const data = await response.json();
        setCatalogues(data);
    }
}

export default App;