using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web2App.Models;

namespace Web2App.Interfaces
{
    public interface IApplicationDbContext
    {
        Task LogAsync(SimaLog model);
    }
}
