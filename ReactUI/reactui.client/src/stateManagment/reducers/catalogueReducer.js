const defaultState = {
    catalogues: [],
    selectedCatalogue: null,
    isLoading:false
}

export default function catalogueReducer(state = defaultState, action) {
    switch (action.type) {
        case 'SET_CATALOGUES':
            state = { ...state, catalogues:action.value }
            return state
        case 'SET_SELECTED_CATALOGUE':
            state = { ...state, selectedCatalogue: action.value }
            console.log(action)
            return state
        default:
            return state
    }
}