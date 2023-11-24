/* eslint-disable react/prop-types */
import  { useEffect, useState } from 'react';
import { connect } from 'react-redux';

function CatalogueInfoPanel(props) {
    //const [catalogue,setCatalogue] = useState(props.catalogues.find(c => c.id === props.selectedCatalogue))
    //useEffect(() => {
    //    setCatalogue(props.catalogues.find(c => c.id == props.selectedCatalogue))
    //}, [props.selectedCatalogue])

    //if (!props.selectedCatalogue) return null;
    //if (!catalogue) return null;


  return (
      <div>
          This is some info about the catalogue you have selected
          <thead>
              <tr>
                  <th style={{ width: '100px' }}>Name</th>
                  <th style={{ width: '100px' }}>Description</th>
                  <th style={{ width: '50' }}>ID</th>
              </tr>
          </thead>
          <tbody>
                  <tr key={props.catalogue.ID}>
                      <td style={{ width: '100px' }}>{props.catalogue.name}</td>
                      <td style={{ width: '100px' }} >{props.catalogue.description || 'no description set'}</td>
                      <td style={{ width: '50px' }} >{props.catalogue.id}</td>
                  </tr>
              
          </tbody>
      </div>
  );
}

const catalogueState = state => {
    return {
        catalogues: state.catalogue.catalogues,
        //selectedCatalogue: state.catalogue.selectedCatalogue,
        isLoading: state.catalogue.isLoading
    }
}

export default connect(catalogueState, null)(CatalogueInfoPanel);