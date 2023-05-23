using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace Simple_Wallet_System.Services
{
    public class DbService : IDbService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly SqlConnection _db;

        public DbService(ILogger<UserService> log, IConfiguration config)
        {
            _logger = log;
            _config = config;
            _db = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString"));
        }

        public async Task<List<T>> GetAll<T>(string command, object parms)
        {
            return (_db.Query<T>(command)).ToList();
        }

        public async Task<T> GetAsync<T>(string command, object parms)
        {
            //return _db.Query<T>(command, parms).FirstOrDefault();
            return (await _db.QueryAsync<T>(command, parms).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<T> Insert<T>(string command, object parms)
        {
            T result;
            result = _db.Query<T>(command, parms, transaction: null, commandTimeout: 60, commandType: CommandType.Text).FirstOrDefault();
            return result;
        }

        public async Task<T> Update<T>(string command, object parms)
        {
            T result;
            result = _db.Query<T>(command, parms, transaction: null, commandTimeout: 60, commandType: CommandType.Text).FirstOrDefault();
            return result;
        }
    }
}
