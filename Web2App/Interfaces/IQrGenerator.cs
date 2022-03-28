using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Interfaces
{
    public interface IQrGenerator
    {
        Task<string> GenerateQr(string body);
    }
}
