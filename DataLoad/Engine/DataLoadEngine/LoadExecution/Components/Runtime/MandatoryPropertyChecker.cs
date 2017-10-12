using System.Linq;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public class MandatoryPropertyChecker:ICheckable
    {
        private readonly ICheckable _classInstanceToCheck;

        public MandatoryPropertyChecker(ICheckable classInstanceToCheck)
        {
            _classInstanceToCheck = classInstanceToCheck;
        }

        public void Check(ICheckNotifier notifier)
        {
            //get all possible properties that we could set
            foreach (var propertyInfo in _classInstanceToCheck.GetType().GetProperties())
            {
                //see if any demand initialization
                DemandsInitializationAttribute demand = System.Attribute.GetCustomAttributes(propertyInfo).OfType<DemandsInitializationAttribute>().FirstOrDefault();

                //this one does
                if (demand != null)
                {
                    if(demand.Mandatory)
                    {
                        var value = propertyInfo.GetValue(_classInstanceToCheck);
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                            notifier.OnCheckPerformed(new CheckEventArgs( "DemandsInitialization Property '" + propertyInfo.Name + "' is marked Mandatory but does not have a value", CheckResult.Fail));

                    }
                }
            }
        }
    }
}