using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Http
{
    public class HttpRequest<T, L>
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public T EntityBody { get; set; }
        public List<L> ListBody { get; set; }
    }
}
