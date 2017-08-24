using System;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.ItemActivation;

namespace ResearchDataManagementPlatform.WindowManagement
{
    public class UIObjectConstructor:ObjectConstructor
    {
        public object Construct(Type t,IActivateItems itemActivator, bool allowBlankConstructors = true)
        {
            return Construct<IActivateItems>(t, itemActivator, allowBlankConstructors);
        }
    }
}