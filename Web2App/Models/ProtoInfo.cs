using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class ProtoInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public ProtoInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
