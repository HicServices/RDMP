import { useEffect, useState } from 'react';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import './App.css';
import HomeContainer from './Components/home/HomeContainer';
import TabContainer from './Components/globals/TabContainer';
import LeftHandPanel from './Components/LeftHandPanel/LeftHandPanel';
import rootReducer from './stateManagment/reducers/rootReducer';
import CatalogueInfoPanel from './Components/Catalogue/CatalogueInfoPanel';
function App() {

    function setTab(value) {
        return {
            type: 'SELECT_TAB',
            value: value
        }
    }


    const store = createStore(rootReducer);
    let [selectedTab, setSelectedTab] = useState(0);
    return (

        <Provider store={store}>
            <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                <TabContainer onClick={value => { store.dispatch(setTab(value)); setSelectedTab(value) }} />
                <div style={{ display: 'flex', flexDirection: 'row', height: '100%' }}>
                    <LeftHandPanel selectedTab={selectedTab} />
                    <div style={{ display: 'flex', flexDirection: 'column', backgroundColor: 'white', width: '-webkit-fill-available', height: 'fill-available', borderRadius: '8px', margin: '4px' }}>

                        {selectedTab == 0 && < HomeContainer />}
                        {selectedTab == 1 && <CatalogueInfoPanel />}
                    </div>

                </div>
            </div>
        </Provider>
    );
}

export default App;