using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class OperationInfo
    {
        public string Type { get; set; }
        public string OperationId { get; set; }
        public long NbfUTC { get; set; }
        public long ExpUTC { get; set; }
        //public int Limit { get; set; }
        public List<string> Assignee { get; set; }

        public OperationInfo(string type, string operationId, long nbfUTC, long expUTC)
        {
            Assignee = new List<string>();

            Type = type;
            OperationId = operationId;
            NbfUTC = nbfUTC;
            ExpUTC = expUTC;
        }

        public void AddAssignee(string finCode)
        {
            if (!Assignee.Contains("finCode"))
                Assignee.Add(finCode);
        }
    }
}
