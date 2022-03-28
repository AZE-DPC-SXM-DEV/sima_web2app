using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class SignableContainer
    {
        public ProtoInfo ProtoInfo { get; set; }
        public OperationInfo OperationInfo { get; set; }
        //public DataInfo DataInfo { get; set; }
        public ClientInfo ClientInfo { get; set; }
       
    }
}
