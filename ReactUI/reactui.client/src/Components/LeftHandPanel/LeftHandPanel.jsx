/* eslint-disable react/prop-types */

import CatalogueLeftHandComponent from "../Catalogue/CatalogueLeftHandComponent";

function LeftHandPanel(props) {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', backgroundColor: 'white', width: '250px', height: 'fill-available', borderRadius: '8px', margin: '4px' }}>
            {props.selectedTab != 1 && 'I am a column'}
            {props.selectedTab == 1 && <CatalogueLeftHandComponent/>}
        </div>
    );
}

export default LeftHandPanel;