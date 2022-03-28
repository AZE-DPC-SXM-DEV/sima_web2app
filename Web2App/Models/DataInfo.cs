using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class DataInfo
    {
        public string AlgName { get; set; }
        public string FingerPrint { get; set; }

        public DataInfo(string algName)
        {
            AlgName = algName;
        }
    }
}
