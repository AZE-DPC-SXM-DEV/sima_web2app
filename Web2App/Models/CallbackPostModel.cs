using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class CallbackPostModel
    {
        public string Type { get; set; }
        public string OperationId { get; set; }
        public string DataSignature { get; set; }
        public string SignedDataHash { get; set; }
        public string AlgName { get; set; }
    }
}
