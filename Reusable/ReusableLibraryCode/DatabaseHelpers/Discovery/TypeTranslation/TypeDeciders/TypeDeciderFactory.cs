using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    public class TypeDeciderFactory
    {
        public readonly Dictionary<Type, IDecideTypesForStrings> Dictionary = new Dictionary<Type, IDecideTypesForStrings>();

        public TypeDeciderFactory()
        {

            var deciders = new IDecideTypesForStrings[]
            {
                new BoolTypeDecider(),
                new IntTypeDecider(),
                new DecimalTypeDecider(),

                new TimeSpanTypeDecider(),
                new DateTimeTypeDecider(),
            };

            foreach (IDecideTypesForStrings decider in deciders)
                foreach (Type type in decider.TypesSupported)
                    Dictionary.Add(type, decider);
        }

        public IDecideTypesForStrings Create(Type forDataType)
        {
            if(!Dictionary.ContainsKey(forDataType))
                throw new Exception("DataType " + forDataType + " does not have an associated IDecideTypesForStrings");

            return Dictionary[forDataType];
        }

        public bool IsSupported(Type forDataType)
        {
            return Dictionary.ContainsKey(forDataType);
        }
    }
}
