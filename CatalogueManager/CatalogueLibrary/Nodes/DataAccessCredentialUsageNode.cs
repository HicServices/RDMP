// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Tree Node for documenting the allowed usage of a specific DataAccessCredentials (username / password) under a given DataAccessContext (loading, extracting etc).
    /// </summary>
    public class DataAccessCredentialUsageNode:IDeleteable
    {
        public DataAccessCredentials Credentials { get; private set; }
        public TableInfo TableInfo { get; private set; }
        public DataAccessContext Context { get; private set; }

        public DataAccessCredentialUsageNode(DataAccessCredentials credentials,TableInfo tableInfo,DataAccessContext context)
        {
            Credentials = credentials;
            TableInfo = tableInfo;
            Context = context;
        }

        public override string ToString()
        {
            return Credentials.ToString() + " (Under Context:" + Context + ")";
        }

        protected bool Equals(DataAccessCredentialUsageNode other)
        {
            return Equals(Credentials, other.Credentials) && Equals(TableInfo, other.TableInfo) && Context == other.Context;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataAccessCredentialUsageNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Credentials != null ? Credentials.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (TableInfo != null ? TableInfo.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Context;
                return hashCode;
            }
        }

        public void DeleteInDatabase()
        {
            //if (MessageBox.Show("Are you sure you want to remove usage rights under Context " + credentialUsage.Context + " for TableInfo " + credentialUsage.TableInfo, "Revoke Usage Permission", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
            var cataRepo = (CatalogueRepository)TableInfo.Repository;
                cataRepo.TableInfoCredentialsManager.BreakLinkBetween(Credentials, TableInfo, Context);

            //;
        }
    }
}
