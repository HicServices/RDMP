using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSelectOne : ConsoleGuiBigListBox<IMapsDirectlyToDatabaseTable>
    {
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _masterCollection;

        /// <summary>
        /// The maximum number of objects to show in the list box
        /// </summary>
        public const int MaxMatches = 100;

        private ConsoleGuiSelectOne():base("Open","Ok",true)
        {

        }
        public ConsoleGuiSelectOne(ICoreChildProvider childProvider):this()
        {
            _masterCollection = childProvider.GetAllSearchables();
        }

        public ConsoleGuiSelectOne(ICoreChildProvider coreChildProvider, IMapsDirectlyToDatabaseTable[] availableObjects):this()
        {
            _masterCollection = coreChildProvider.GetAllSearchables().Where(k=> availableObjects.Contains(k.Key)).ToDictionary(k=>k.Key,v=>v.Value);
        }

        protected override IList<IMapsDirectlyToDatabaseTable> GetListAfterSearch(string searchText)
        {
            return new SearchablesMatchScorer()
                .ScoreMatches(_masterCollection, searchText, new CancellationToken())
                .Where(score => score.Value > 0)
                .OrderByDescending(score => score.Value)
                .ThenByDescending(id => id.Key.Key.ID) //favour newer objects over ties
                .Take(MaxMatches)
                .Select(score => score.Key.Key)
                .ToList();
        }

        protected override IList<IMapsDirectlyToDatabaseTable> GetInitialSource()
        {
            return _masterCollection.Keys.ToList();
        }
    }
}