/* eslint-disable react/prop-types */
import {  useEffect } from 'react';
import { connect } from 'react-redux';
function CatalogueLeftHandComponent(props) {
    useEffect(() => {
        populateCatalogues();
    }, []);
    const catalogues = props.catalogues;
    const contents = catalogues === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <table className="table table-striped" aria-labelledby="tabelLabel" style={{ width: '250px' }}>
            <thead>
                <tr>
                    <th style={{ width: '100px' }}>Name</th>
                    {/*<th style={{ width: '100px' }}>Description</th>*/}
                    <th style={{ width: '50' }}>ID</th>
                </tr>
            </thead>
            <tbody>
                {catalogues.map(catalogue =>
                    //<tr key={catalogue.ID} onClick={() => props.setSelectedCatalogue(catalogue.id)} style={{ cursor: 'pointer' }}>
                        <tr key={catalogue.ID} onClick={() => props.openTab(catalogue.id)} style={{ cursor: 'pointer' }}>
                        <td style={{ width: '100px' }}>{catalogue.name}</td>
                        {/*<td style={{ width: '100px' }} >{catalogue.description || 'no description set'}</td>*/}
                        <td style={{ width: '50px' }} >{catalogue.id}</td>
                    </tr>
                )}
            </tbody>
        </table>;
    return (

        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

            <h4 id="tabelLabel">Catalogues</h4>
            <p>This component demonstrates fetching data from the server.</p>
            {contents}
        </div>
    )




    async function populateCatalogues() {
        const response = await fetch('catalogues');
        const data = await response.json();
        props.setGlobalCatalogues(data)
    }
}

function setStoreCatalogues(catalogues) {
    return {
        type: 'SET_CATALOGUES',
        value: catalogues
    }
}

function setSelectedCatalogue(id) {
    return {
        type: 'SET_SELECTED_CATALOGUE',
        value: id
    }
}

function openTab(id) {
    return {
        type: 'OPEN_TAB',
        tab: {
            id: id,
            type:'CATALOGUE'
        }
    }
}


const catalogueActions = (dispatch) => {
    return {
        setGlobalCatalogues: (catalogues) => dispatch(setStoreCatalogues(catalogues)),
        setSelectedCatalogue: id => dispatch(setSelectedCatalogue(id)),
        openTab: id => dispatch(openTab(id))
    }
}

const catalogueState = state => {
    return {
        catalogues: state.catalogue.catalogues,
        isLoading: state.catalogue.isLoading
    }
}

export default connect(catalogueState, catalogueActions)(CatalogueLeftHandComponent);