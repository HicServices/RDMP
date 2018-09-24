using System;

namespace CatalogueLibrary.Data.DataLoad.Extensions
{
    /// <summary>
    /// Static type extensions for Enum <see cref="LoadBubble"/>
    /// </summary>
    public static class LoadBubbleExtensions
    {
        /// <summary>
        /// Converts a <see cref="LoadBubble"/> into a <see cref="LoadStage"/>
        /// </summary>
        /// <param name="bubble"></param>
        /// <returns></returns>
        public static LoadStage ToLoadStage(this LoadBubble bubble)
        {
            switch (bubble)
            {
                case LoadBubble.Raw:
                    return LoadStage.AdjustRaw;
                case LoadBubble.Staging:
                    return LoadStage.AdjustStaging;
                case LoadBubble.Live:
                    return LoadStage.PostLoad;
                case LoadBubble.Archive:
                    throw new Exception("LoadBubble.Archive refers to _Archive tables, therefore it cannot be translated into a LoadStage");
                default:
                    throw new ArgumentOutOfRangeException("bubble");
            }
        }
    }
}