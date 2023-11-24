import { combineReducers } from "redux";
import globalReducer from "./globalReducer";
import catalogueReducer from "./catalogueReducer";
export const rootReducer = combineReducers({
    globals: globalReducer,
    catalogue: catalogueReducer
})

export default rootReducer;