using CatalogueLibrary.Nodes;

namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Tree node for showing the single Private Key location in TableInfoCollectionUI (See PasswordEncryptionKeyLocationUI)
    /// </summary>
    public class DecryptionPrivateKeyNode:SingletonNode
    {
        public DecryptionPrivateKeyNode() : base("Decryption Certificate")
        {
        }
    }
}