using System;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Static extensions for <see cref="LoadStage"/>
    /// </summary>
    public static class LoadStageExtensions
    {
        /// <summary>
        /// Converts a <see cref="LoadStage"/> into a <see cref="LoadBubble"/>
        /// </summary>
        /// <param name="loadStage"></param>
        /// <returns></returns>
        public static LoadBubble ToLoadBubble(this LoadStage loadStage)
        {
            switch (loadStage)
            {
                case LoadStage.GetFiles:
                    return LoadBubble.Raw;
                case LoadStage.Mounting:
                    return LoadBubble.Raw;
                case LoadStage.AdjustRaw:
                    return LoadBubble.Raw;
                case LoadStage.AdjustStaging:
                    return LoadBubble.Staging;
                case LoadStage.PostLoad:
                    return LoadBubble.Live;
                default:
                    throw new ArgumentOutOfRangeException("Unknown value for LoadStage: " + loadStage);
            }
        }
    }
}