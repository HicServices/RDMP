import { useState, useEffect } from 'react';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import './App.css';
//import HomeContainer from './Components/home/HomeContainer';
import TabContainer from './Components/globals/TabContainer';
import LeftHandPanel from './Components/LeftHandPanel/LeftHandPanel';
import rootReducer from './stateManagment/reducers/rootReducer';
//import CatalogueInfoPanel from './Components/Catalogue/CatalogueInfoPanel';
import Root from './Root'

function App() {

  
    let store = createStore(rootReducer);


    return (

        <Provider store={store}>
           <Root/>
        </Provider>
    );
}

export default App;