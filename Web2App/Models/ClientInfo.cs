using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Models
{
    public class ClientInfo
    {
        public int ClientId { get; set; }
        public string IconURI { get; set; }
        public string Callback { get; set; }

        public ClientInfo(int clientId , string iconUri, string callBack)
        {
            ClientId = clientId;
            IconURI = iconUri;
            Callback = callBack;
        }

    }
}
