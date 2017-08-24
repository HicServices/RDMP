using System.Collections.Generic;

namespace ReusableUIComponents.Dependencies.Models
{
    public class ByteComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) return false;
            for (int ix = 0; ix < x.Length; ++ix)
                if (x[ix] != y[ix]) return false;
            return true;
        }
        public int GetHashCode(byte[] obj)
        {
            int retval = 0;
            foreach (byte value in obj) retval = (retval << 6) ^ value;
            return retval;
        }
    }
}