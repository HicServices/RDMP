import { useEffect, useState } from 'react';
import './App.css';
import HomeContainer from './home/HomeContainer';
import TabContainer from './globals/TabContainer';
import LeftHandPanel from './LeftHandPanel/LeftHandPanel';

function App() {
    //const [catalogues, setCatalogues] = useState();

    const tabs = {
        home: 0
    };

    const [selectedTab, setSelectedTab] = useState(0);

    useEffect(() => {
        //populateCatalogues();
    }, []);
    //console.log(catalogues);
    //const contents = catalogues === undefined
    //    ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
    //    : <table className="table table-striped" aria-labelledby="tabelLabel">
    //        <thead>
    //            <tr>
    //                <th>Name</th>
    //                <th>Description</th>
    //                <th>ID</th>
    //            </tr>
    //        </thead>
    //        <tbody>
    //            {catalogues.map(catalogue =>
    //                <tr key={catalogue.ID}>
    //                    <td>{catalogue.name}</td>
    //                    <td>{catalogue.description || 'no description set'}</td>
    //                    <td>{catalogue.id}</td>
    //                </tr>
    //            )}
    //        </tbody>
    //    </table>;
    return (
        
        <div style={{ display: 'flex', flexDirection:'column', height:'100%' }}>
            <TabContainer onClick={setSelectedTab} />
            <div style={{ display: 'flex', flexDirection: 'row', height: '100%' }}>
                <LeftHandPanel selectedTab={selectedTab} />
                <div style={{ display: 'flex', flexDirection: 'column', backgroundColor: 'white', width: '-webkit-fill-available', height: 'fill-available', borderRadius: '8px', margin: '4px' }}>

                {selectedTab == 0 && < HomeContainer />}
                </div>

            </div>
            {/*<h1 id="tabelLabel">Catalogues</h1>*/}
            {/*<p>This component demonstrates fetching data from the server.</p>*/}
            {/*{contents}*/}
        </div>
    );
    
    //async function populateCatalogues() {
    //    const response = await fetch('catalogues');
    //    const data = await response.json();
    //    setCatalogues(data);
    //}
}

export default App;