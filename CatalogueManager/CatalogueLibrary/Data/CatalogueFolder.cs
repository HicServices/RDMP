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
    /// CatalogueFolder is basically a string but has method to help prevent illegal paths and to calculate hierarchy based on multiple Catalogues (See GetImmediateSubFoldersUsing)
    /// </summary>
    public class CatalogueFolder : IConvertible
    {
        private readonly Catalogue _parent;
        private string _path;
        
        public static CatalogueFolder Root = new CatalogueFolder("\\");

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

        public static bool IsValidPath(string candidatePath)
        {
            string whoCares;
            return new CatalogueFolder(candidatePath).IsValidPath(candidatePath, out whoCares);
        }

        public override string ToString()
        {
            return Path;
        }


        public override bool Equals(object obj)
        {
            var other = obj as CatalogueFolder;
            if (other != null)
                return other.Path.Equals(this.Path);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public string ToString(IFormatProvider provider)
        {
            return Path;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            //if it is a string or subtype of string?
            if (typeof (string).IsAssignableFrom(conversionType))
                return Path;//return path

            throw new NotSupportedException();
        }

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
        /// Pass in 
        /// CatalogueA - \2005\Research\
        /// CatalogueB - \2006\Research\
        /// 
        /// This is Root (\)
        /// Returns:
        ///     \2005\ - empty 
        ///     \2006\ - empty
        /// 
        /// Pass in the SAME collection but call on This (\2005\)
        /// Returns :
        /// \2005\Research\ - containing CatalogueA
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