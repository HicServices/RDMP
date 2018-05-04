using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// The virtual 'folder' in which to describe the Catalogue as residing in to the user.  This is implemented in the UI as a tree of folders but is calculated from all the
    /// visible Catalogues at any given time (you can't create an empty CatalogueFolder you just have to declare a Catalogue as being in a new folder name).
    /// 
    /// <para>CatalogueFolder is basically a string but has method to help prevent illegal paths and to calculate hierarchy based on multiple Catalogues (See GetImmediateSubFoldersUsing)</para>
    /// </summary>
    public class CatalogueFolder : IConvertible
    {
        private readonly Catalogue _parent;
        private string _path;
         
        /// <summary>
        /// The topmost folder under which all <see cref="CatalogueFolder"/> reside
        /// </summary>
        public static CatalogueFolder Root = new CatalogueFolder("\\");


        /// <summary>
        /// The full path of the folder (starts and ends with a slash).  Throws if you try to set property to an invalid path 
        /// <seealso cref="IsValidPath"/> 
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                string reason;

                if (IsValidPath(value, out reason))
                {
                    _path = value.ToLower();
                    
                    //ensure it ends with a slash
                    if (!_path.EndsWith("\\"))
                        _path += "\\";
                }
                else
                    throw new NotSupportedException(reason);
            }
        }

        /// <summary>
        /// Creates a new folder that the Catalogue should now reside in.
        /// <para><remarks>After calling this you should use <code>parent.Folder = instance; parent.SaveToDatabase();</code></remarks></para>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        public CatalogueFolder(Catalogue parent, string folder)
        {
            //always Lower everything!
            folder = folder.ToLower();

            _parent = parent;
            Path = folder;
        }

        //private because this is a folder that does not know who it's associated Catalogues are (and indeed there might not even be any e.g. if user has \2001\Research\ then probably the \2001\ folder is empty, certainly the root is probably empty)
        private CatalogueFolder(string path)
        {
            Path = path;
        }

        private bool IsValidPath(string candidatePath, out string reason)
        {
            reason = null;

            if (string.IsNullOrWhiteSpace(candidatePath))
                reason = "An attempt was made to set Catalogue " +_parent+ " Folder to null, every Catalogue must have a folder, set it to \\ if you want the root";
            else
            if (!candidatePath.StartsWith("\\"))
                reason = "All catalogue paths must start with \\ but Catalogue " + _parent + " had an attempt to set it's folder to :" + candidatePath;
            else
            if (candidatePath.Contains("\\\\"))//if it contains double slash
                reason = "Catalogue paths cannot contain double slashes '\\\\', Catalogue " + _parent + " had an attempt to set it's folder to :" + candidatePath;
            else
            if (candidatePath.Contains("/"))//if it contains double slash
                reason = "Catalogue paths must use backwards slashes not forward slashes, Catalogue " + _parent + " had an attempt to set it's folder to :" + candidatePath;

            return reason == null;
        }

        /// <summary>
        /// Returns true if the specified path is valid for a <see cref="CatalogueFolder"/>.  Not blank, starts with '\' etc.
        /// </summary>
        /// <param name="candidatePath"></param>
        /// <returns></returns>
        public static bool IsValidPath(string candidatePath)
        {
            string whoCares;
            return new CatalogueFolder(candidatePath).IsValidPath(candidatePath, out whoCares);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Path;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as CatalogueFolder;
            if (other != null)
                return other.Path.Equals(this.Path);

            return base.Equals(obj);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        /// <summary>
        /// Makes this class behave as a string for IConvertible
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public long ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
        /// <summary>
        /// Returns the Path
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider)
        {
            return Path;
        }

        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            //if it is a string or subtype of string?
            if (typeof (string).IsAssignableFrom(conversionType))
                return Path;//return path

            throw new InvalidCastException();
        }

        /// <summary>
        /// Returns true if the passed value is resident in a subfolder of this one.
        /// </summary>
        /// <param name="potentialParent"></param>
        /// <returns></returns>
        public bool IsSubFolderOf(CatalogueFolder potentialParent)
        {
            if (potentialParent == null)
                return false;

            //they are the same folder so not subfoldres
            if (Path.Equals(potentialParent.Path))
                return false;

            //we contain the potential parents path therefore we are a child of them
            return Path.StartsWith(potentialParent.Path);
        }
        
        /// <summary>
        /// Returns the next level of folder down towards the Catalogues in collection - note that the next folder down might be empty 
        /// e.g.
        /// 
        /// <para>Pass in 
        /// CatalogueA - \2005\Research\
        /// CatalogueB - \2006\Research\</para>
        /// 
        /// <para>This is Root (\)
        /// Returns:
        ///     \2005\ - empty 
        ///     \2006\ - empty</para>
        /// 
        /// <para>Pass in the SAME collection but call on This (\2005\)
        /// Returns :
        /// \2005\Research\ - containing CatalogueA</para>
        /// </summary>
        /// <param name="collection"></param>
        [Pure]
        public CatalogueFolder[] GetImmediateSubFoldersUsing(IEnumerable<Catalogue> collection)
        {
            List<CatalogueFolder> toReturn = new List<CatalogueFolder>();


            var remoteChildren = collection.Where(c => c.Folder.IsSubFolderOf(this)).Select(c=>c.Folder).ToArray();

            //no subfolders exist
            if (!remoteChildren.Any())
                return toReturn.ToArray();//empty
            

            foreach (CatalogueFolder child in remoteChildren)
            {
                // We are \bob\

                //we are looking at \bob\fish\smith\harry\

                //chop off \bob\
                string trimmed = child.Path.Substring(Path.Length);

                //trimmed = fish\smith\harry\

                string nextFolder = trimmed.Substring(0, trimmed.IndexOf('\\')+1);
                
                //nextFolder = fish\

                //add 
                toReturn.Add(new CatalogueFolder(Path + nextFolder));
            }

            return toReturn.Distinct().ToArray();

        }
    }
}
