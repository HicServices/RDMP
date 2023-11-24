/* eslint-disable react/prop-types */

function TabContainer(props) {
    return (
        <div style={{ display: 'flex', flexDirection: 'row' }}>
            <img src="https://raw.githubusercontent.com/HicServices/RDMP/develop/Application/ResearchDataManagementPlatform/Icon/mainsmall.png" style={{ width: '50px',height:'50px',marginLeft: '5px', marginRight:'5px' } } />
            <button onClick={()=>props.onClick(0) }> Home</button>
            <button onClick={() => props.onClick(1)}>Catalogues</button>
      </div>
  );
}

export default TabContainer;