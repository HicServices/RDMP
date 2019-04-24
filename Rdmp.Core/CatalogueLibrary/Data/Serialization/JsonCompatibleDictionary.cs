// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Rdmp.Core.CatalogueLibrary.Data.Serialization
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
        /// <summary>
        /// Returns the keys of the dictionary as an Array.  Json loves arrays and hates dictionary keys
        /// </summary>
        [JsonProperty]
        public TK[] SerializeableKeys { get { return Keys.ToArray(); }
            set { Hydrate(value);}
        }

        /// <summary>
        /// Returns the values of the dictionary as an Array.
        /// </summary>
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
