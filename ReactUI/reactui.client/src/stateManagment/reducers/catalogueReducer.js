const defaultState = {
    catalogues: [],
    selectedCatalogue: null,
    isLoading:false
}

export default function catalogueReducer(state = defaultState, action) {
    switch (action.type) {
        case 'SET_CATALOGUES':
            var s = structuredClone(state)
            s.catalogues = action.value
            return s
        case 'SET_SELECTED_CATALOGUE':
            state = { ...state, selectedCatalogue: action.value }
            return state
        default:
            return state
    }
}