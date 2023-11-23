/* eslint-disable react/prop-types */

function TabContainer(props) {
    return (
        <div style={{ display: 'flex', flexDirection: 'row' }}>
            <button onClick={()=>props.onClick(0) }> Home</button>
            <button onClick={() => props.onClick(1)}>Catalogues</button>
      </div>
  );
}

export default TabContainer;