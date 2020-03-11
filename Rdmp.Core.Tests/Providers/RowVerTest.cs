using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers
{
    class RowVerTest : DatabaseTests
    {
        [Test]
        public void Test_RowVer()
        {
            var cata = new Catalogue(CatalogueRepository, "FFFF");

            //When we get all the Catalogues we should include cata
            Assert.Contains(cata,CatalogueRepository.GetAllObjects<Catalogue>());
            Assert.AreEqual(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);
            Assert.AreNotSame(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);

            //create a cache
            var rowVerCache = new RowVerCache<Catalogue>(CatalogueRepository);
            List<Catalogue> deleted;
            List<Catalogue> added;
            List<Catalogue> changed;

            //should fill the cache
            cata = rowVerCache.GetAllObjects(out _, out added, out changed)[0];
            Assert.Contains(cata,added);
            
            //should return the same instance
            Assert.AreSame(cata, rowVerCache.GetAllObjects(out deleted, out added, out changed)[0]);
            Assert.IsEmpty(deleted);

            cata.DeleteInDatabase();
            Assert.IsEmpty(rowVerCache.GetAllObjects(out deleted, out added, out changed));
            Assert.AreSame(cata,deleted[0]);
            
            //create some catalogues
            new Catalogue(CatalogueRepository, "1");
            new Catalogue(CatalogueRepository, "2");
            new Catalogue(CatalogueRepository, "3");

            //fill up the cache
            rowVerCache.GetAllObjects(out deleted, out added, out changed);

            //give it a fresh object
            var cata2 = new Catalogue(CatalogueRepository, "dddd");
            
            //fresh fetch for this
            Assert.Contains(cata2,rowVerCache.GetAllObjects(out deleted, out added, out changed));
            Assert.AreEqual(cata2,added.Single());
            Assert.IsFalse(rowVerCache.GetAllObjects(out deleted, out added, out changed).Contains(cata));
            Assert.IsEmpty(deleted);

            //change a value in the background but first make sure that what the cache has is a Equal but not reference to cata2
            Assert.IsFalse(rowVerCache.GetAllObjects(out deleted, out added, out changed).Any(o=>ReferenceEquals(o,cata2)));
            Assert.IsTrue(rowVerCache.GetAllObjects(out deleted, out added, out changed).Any(o=>Equals(o,cata2)));

            cata2.Name = "boom";
            cata2.SaveToDatabase();
            
            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("boom")));
            Assert.AreEqual(1,changed.Count);
            
            cata2.Name = "vroom";
            cata2.SaveToDatabase();
            
            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("vroom")));
            Assert.AreEqual(1,changed.Count);

            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("vroom")));
            Assert.AreEqual(0,changed.Count);
        }
    }

    public class RowVerCache<T> where T : IMapsDirectlyToDatabaseTable
    {
        private readonly TableRepository _repository;

        private List<T> _cachedObjects = new List<T>();

        private byte[] _maxRowVer;

        public RowVerCache(TableRepository repository)
        {
            _repository = repository;
        }

        public List<T> GetAllObjects(out List<T> deleted, out List<T> newObjects, out List<T> changedObjects)
        {
            deleted = new List<T>();
            changedObjects = new List<T>();

            var server = _repository.DiscoveredServer;

            //cache is empty
            if (!_cachedObjects.Any() || _maxRowVer == null)
            {
                _cachedObjects.AddRange(_repository.GetAllObjects<T>());
                newObjects = _cachedObjects;
                UpdateMaxRowVer(server);
                return _cachedObjects;
            }
        

            // Get deleted objects
            /*

SELECT ID
  FROM (
	VALUES (1),(2),(3),(4),(5),(6),(50)
       ) t1 (ID)
EXCEPT
select ID FROM Catalogue

*/
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT ID FROM ( VALUES");
            sb.Append(string.Join(",", _cachedObjects.Select(l => $"({l.ID})")));
            sb.AppendLine(@") t1 (ID)
EXCEPT
select ID FROM ");
            sb.Append(typeof(T).Name);

            using (var con = server.GetConnection())
            {
                con.Open();
                using(var cmd = _repository.DiscoveredServer.GetCommand(sb.ToString(), con))
                    using(var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            var d = _cachedObjects.Single(o => o.ID == (int)r["ID"]);
                            deleted.Add(d);
                            _cachedObjects.Remove(d);
                        }
            }

            //get new objects
            var maxId = _cachedObjects.Any() ? _cachedObjects.Max(o => o.ID) : 0;
            newObjects = new List<T>(_repository.GetAllObjects<T>("WHERE ID > " + maxId));
            _cachedObjects.AddRange(newObjects);

            // Get updated objects
            changedObjects = new List<T>(_repository.GetAllObjects<T>("WHERE RowVer > " + ByteArrayToString(_maxRowVer)));
            //I'm hoping Union prefers references in the LHS of this since they will be fresher!
            _cachedObjects = changedObjects.Union(_cachedObjects).ToList();

            UpdateMaxRowVer(server);

            return _cachedObjects;
        }

        private void UpdateMaxRowVer(DiscoveredServer server)
        {
            //get the earliest RowVer
            using (var con = server.GetConnection())
            {
                con.Open();
                using (var cmd = _repository.DiscoveredServer.GetCommand("select max(RowVer) from " + typeof(T).Name, con))
                    _maxRowVer = (byte[])cmd.ExecuteScalar();
            }
        }
        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return "0x" + hex;
        }
    }
}
