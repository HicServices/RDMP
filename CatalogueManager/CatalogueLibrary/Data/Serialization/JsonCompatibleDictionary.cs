using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Allows Json serialization of complex key Types.
    /// 
    /// <para>Out of the box Json serializes Dictionary keys using ToString and seems to ignore any custom JsonConverter specified on the key class.  This class works around that behaviour
    /// by only serializing an array of keys and an array of values.  Once both are populated then the underlying Dictionary Key/Values are created.</para>
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonCompatibleDictionary<TK, TV> : Dictionary<TK, TV>
    {
        [JsonProperty]
        public TK[] SerializeableKeys { get { return Keys.ToArray(); }
            set { Hydrate(value);}
        }
        
        [JsonProperty]
        public TV[] SerializeableValues
        {
            get { return Values.ToArray(); }
            set { Hydrate(value); }
        }
        
        private TK[] _hydrateV1;
        private TV[] _hydrateV2;

        private void Hydrate(TK[] value)
        {
            _hydrateV1 = value;
            Hydrate(_hydrateV1,_hydrateV2);
        }

        private void Hydrate(TV[] value)
        {
            _hydrateV2 = value;
            Hydrate(_hydrateV1, _hydrateV2);
        }

        private void Hydrate(TK[] hydrateV1, TV[] hydrateV2)
        {
            if(hydrateV1 == null || hydrateV2 == null)
                return;
            
            if(_hydrateV1.Length != hydrateV2.Length)
                return;

            Clear();

            for (int i = 0; i < _hydrateV1.Length; i++)
                Add(_hydrateV1[i], _hydrateV2[i]);
        }
    }
}
