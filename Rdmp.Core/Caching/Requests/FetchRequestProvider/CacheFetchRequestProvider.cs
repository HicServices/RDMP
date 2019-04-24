// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public class CacheFetchRequestProvider : ICacheFetchRequestProvider
    {
        private readonly ICacheFetchRequest _initialRequest;
        public ICacheFetchRequest Current { get; private set; }

        public CacheFetchRequestProvider(ICacheFetchRequest initialRequest)
        {
            Current = null;
            _initialRequest = initialRequest;
        }

        public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
        {
            Current = Current == null ? _initialRequest : CreateNext();
            return Current;
        }

        private ICacheFetchRequest CreateNext()
        {
            var nextStart = Current.Start.Add(Current.ChunkPeriod);
            return new CacheFetchRequest(_initialRequest.Repository,nextStart)
            {
                CacheProgress = Current.CacheProgress,
                PermissionWindow = Current.PermissionWindow,
                ChunkPeriod = Current.ChunkPeriod
            };
        }
    }
}