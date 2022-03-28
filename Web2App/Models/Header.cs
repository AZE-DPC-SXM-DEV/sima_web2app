using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class Header
    {
        public string AlgName { get; set; }
        public string Signature { get; set; }

        public Header(string algName)
        {
            AlgName = algName;
        }
    }
}
