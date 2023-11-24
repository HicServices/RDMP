/* eslint-disable react/prop-types */
/* eslint-disable no-undef */
import { useState,  } from 'react';
import {  connect} from 'react-redux'
import './App.css';
//import HomeContainer from './Components/home/HomeContainer';
import TabContainer from './Components/globals/TabContainer';
import LeftHandPanel from './Components/LeftHandPanel/LeftHandPanel';
//import CatalogueInfoPanel from './Components/Catalogue/CatalogueInfoPanel';
import WorkspaceContainer from './Components/workspace/WorkspaceContainer'
function Root(props) {
    let [selectedTab, setSelectedTab] = useState(0);

  return (
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          <TabContainer onClick={value => { props.st(value); setSelectedTab(value) }} />
          <div style={{ display: 'flex', flexDirection: 'row', height: '100%' }}>
              <LeftHandPanel selectedTab={selectedTab} />
              <div style={{ display: 'flex', flexDirection: 'column', backgroundColor: 'white', width: '-webkit-fill-available', height: 'fill-available', borderRadius: '8px', margin: '4px' }}>
                  <WorkspaceContainer />
              </div>

          </div>
      </div>
  );
}

function setTab(value) {
    return {
        type: 'SELECT_TAB',
        value: value
    }
}


const reduxFunctions = dispatch => {
    return {
        st: id => dispatch(setTab(id))
    }
}

export default connect(null, reduxFunctions)(Root);