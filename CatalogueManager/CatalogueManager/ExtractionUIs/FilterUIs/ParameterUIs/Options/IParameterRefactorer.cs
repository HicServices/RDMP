using CatalogueLibrary.Data;

namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options
{
    public interface IParameterRefactorer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns>true if you changed the parameters owner</returns>
        bool HandleRename(ISqlParameter parameter, string oldName, string newName);
    }
}