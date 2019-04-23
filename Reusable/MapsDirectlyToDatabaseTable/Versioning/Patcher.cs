using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <inheritdoc/>
    public abstract class Patcher:IPatcher
    {
        /// <inheritdoc/>
        public virtual Assembly GetDbAssembly()
        {
            return GetType().Assembly;
        }

        /// <inheritdoc/>
        public string ResourceSubdirectory { get; private set; }

        /// <inheritdoc/>
        public int Tier { get; }

        public string Name => GetDbAssembly().GetName().Name + (string.IsNullOrEmpty(ResourceSubdirectory) ? "" : "/" + ResourceSubdirectory);
        public string LegacyName { get; protected set; }

        protected Patcher(int tier,string resourceSubdirectory)
        {
            Tier = tier;
            ResourceSubdirectory = resourceSubdirectory;
        }
    }
}