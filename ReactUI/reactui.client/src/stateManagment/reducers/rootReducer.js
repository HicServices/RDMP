import { combineReducers } from "redux";
import globalReducer from "./globalReducer";
import catalogueReducer from "./catalogueReducer";
import workspaceReducer from "./workspaceReducer";
export const rootReducer = combineReducers({
    globals: globalReducer,
    catalogue: catalogueReducer,
    workspace: workspaceReducer
})

export default rootReducer;