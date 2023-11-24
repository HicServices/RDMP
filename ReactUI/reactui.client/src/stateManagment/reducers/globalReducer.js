const defaultState = {
    selectedTab:0
}

export default function globalReducer(state = defaultState, action) {
    switch (action.type) {
        case 'SELECT_TAB':
            state.selectedTab = action.value
            return state
        default:
            return state
    }
}