using System;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.ItemActivation;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Provides UI specific helpful overloads to ObjectConstructor (which is defined in a data class)
    /// </summary>
    public class UIObjectConstructor:ObjectConstructor
    {
        public object Construct(Type t,IActivateItems itemActivator, bool allowBlankConstructors = true)
        {
            return Construct<IActivateItems>(t, itemActivator, allowBlankConstructors);
        }
    }
}