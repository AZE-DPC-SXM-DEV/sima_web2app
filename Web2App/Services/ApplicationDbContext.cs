using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Web2App.Interfaces;
using Web2App.Models;

namespace Web2App.Services
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        private readonly SqlConnection _sqlConnection;
        private readonly IConfiguration _configuration;
        public ApplicationDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _sqlConnection = new SqlConnection(_configuration["Database:Connection"]);

        }
        public async Task LogAsync(SimaLog model)
        {
            _sqlConnection.Open();
            string command = @$"Insert Into SimaLogs ([Description],[ErrorMessage],[RequestBody],[Headers],[SimaLogTypeId],[Created],[IpAddress])
                                     Values ('{model.Description}','{model.ErrorMessage}','{model.RequestBody}','{model.Headers}','{model.SimaLogTypeId.ToString()}','{model.Created}','{model.IpAddress}')";
           using(SqlCommand sqlCommand = new SqlCommand(command, _sqlConnection))
            {
                var affectedRow = await sqlCommand.ExecuteNonQueryAsync();
                _sqlConnection.Close();

            }
        }



    }
}
