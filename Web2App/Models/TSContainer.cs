using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class TSContainer
    {
        public SignableContainer SignableContainer { get; set; }
        public Header Header { get; set; }


        public TSContainer()
        {
            SignableContainer = new SignableContainer();
        }

    }
}
