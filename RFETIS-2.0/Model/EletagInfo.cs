using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Model
{
    public class EletagInfo
    {
        public int Guid { get; set; }
        public string Type { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return Guid + " " + Type + " " + Value;
        }
    }
}
