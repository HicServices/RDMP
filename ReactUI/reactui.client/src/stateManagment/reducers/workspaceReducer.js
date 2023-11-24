const defaultState = {
    openTabs: [{ type: 'home' }],
    selectedTab:0
}

export default function workspaceReducer(state = defaultState, action) {
    switch (action.type) {
        case 'OPEN_TAB':
            var s = structuredClone(state)
            s.openTabs = [...state.openTabs, action.tab]
            s.selectedTab = [...state.openTabs, action.tab].length - 1
            return s
        case 'SELECT_OPEN_TAB':
            var s1 = structuredClone(state)
            s1.selectedTab =action.value
            return s1
        case 'CLOSE_TAB':
            state.openTabs = state.openTabs.filter(tab => tab == action.tab)
            state.selectedTab = state.selectedTab > state.openTabs.length - 1 ? state.openTabs.length - 1 : state.selectedTab
            return state
        default:
            return state
    }
}