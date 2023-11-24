/* eslint-disable react/jsx-key */
/* eslint-disable react/prop-types */
import React, { useEffect, useState } from 'react';
import { connect, useSelector } from 'react-redux';
import Box from '@mui/material/Box';
import Tab from '@mui/material/Tab';
import TabContext from '@mui/lab/TabContext';
import TabList from '@mui/lab/TabList';
import TabPanel from '@mui/lab/TabPanel';
import HomeContainer from '../home/HomeContainer'
import CatalogueInfoPanel from '../catalogue/CatalogueInfoPanel'
function WorkspaceContainer(props) {

    const tabs= useSelector(state => state.workspace.openTabs)
    const selectedTab = useSelector(state => state.workspace.selectedTab)


    const getName = tab => {
        if (tab.type === 'CATALOGUE') {
            let catalogue = props.catalogues.find(t => t.id === tab.id)
            console.log(catalogue)
            return catalogue ? catalogue.name : 'unknown';
        }
        return 'Unknown'
    }

    return (
        <TabContext value={selectedTab}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                <TabList onChange={(e,i) => props.st(i)} aria-label="lab API tabs example">
                    {tabs.map((t, i) => <Tab label={t.type == 'home' ? 'Home' : getName(t)} value={i} />)}
                </TabList>
            </Box>
            {tabs.map((tab, i) =>
                <TabPanel value={i}>{tab.type === 'CATALOGUE' ? <CatalogueInfoPanel catalogue={props.catalogues.find(t => t.id === tab.id)} />:<HomeContainer></HomeContainer>}</TabPanel>
            )}
            {/*<TabPanel value={1}>Item One</TabPanel>*/}
            {/*<TabPanel value={2}>Item Two</TabPanel>*/}
            {/*<TabPanel value={3}>Item Three</TabPanel>*/}
        </TabContext>
    );
}



const workspaceState = state => {
    return {
        tabs: state.workspace.openTabs,
        openTab: state.workspace.selectedTab,
        catalogues: state.catalogue.catalogues,
    }
}

function setTab(value) {
    console.log(value)
    return {
        type: 'SELECT_OPEN_TAB',
        value: value
    }
}


const reduxFunctions = dispatch => {
    return {
        st: id => dispatch(setTab(id))
    }
}


export default connect(workspaceState, reduxFunctions)(WorkspaceContainer);