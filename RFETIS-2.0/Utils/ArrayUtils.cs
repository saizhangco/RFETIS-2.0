using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Utils
{
    public class ArrayUtils<T>
    {
        public static string ToString(List<T> list)
        {
            string result = "";
            foreach( T t in list)
            {
                result += t.ToString() + "  ";
            }
            return result;
        }
    }
}
