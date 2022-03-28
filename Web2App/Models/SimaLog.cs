using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class SimaLog
    {
        public int Id { get; set; }
        public string RequestBody { get; set; }
        public string Headers { get; set; }
        public string ErrorMessage { get; set; }
        public string Description { get; set; }
        public int SimaLogTypeId { get; set; }
        public DateTime Created { get; set; }
        public string IpAddress { get; set; }
    }
}
