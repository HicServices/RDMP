namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// A potentially executable object.  Can be translated into an ICommandExecution by an ICommandExecutionFactory.  For example ICommand CatalogueCommand can 
    /// be translated into ExecuteCommandPutCatalogueIntoCatalogueFolder (an ICommandExecution) by combining it with a CatalogueFolder.  But you could equally
    /// turn it into an ExecuteCommandAddCatalogueToCohortIdentificationSetContainer (also an ICommandExecution) by combining it with a CohortAggregateContainer.
    /// 
    /// ICommand should reflect a single object and contain all useful information discovered about the object so that the ICommandExecutionFactory can make a 
    /// good decision about what ICommandExecution to create as the user drags it about the place.
    /// </summary>
    public interface ICommand
    {
        string GetSqlString();
    }
}
