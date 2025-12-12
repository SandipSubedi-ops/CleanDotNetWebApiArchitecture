using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CleanDapperApp.Models.Interfaces;

namespace CleanDapperApp.Repository.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultConnectionString;
        private IDbConnection _connection;

        public BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultConnectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");
        }

        #region Transaction Management

        public IDbTransaction BeginTransaction()
        {
            _connection = CreateConnection(_defaultConnectionString);
            _connection.Open();
            return _connection.BeginTransaction();
        }

        public IDbTransaction BeginTransactionGen(string propCode)
        {
            var connectionString = GetConnectionString(propCode);
            _connection = CreateConnection(connectionString);
            _connection.Open();
            return _connection.BeginTransaction();
        }

        #endregion

        #region Connection String Management

        public string GetConnectionString(string clientCode)
        {
            // Try to get connection string from appsettings.json using the clientCode
            // Format: "ConnectionStrings:{clientCode}"
            var connectionString = _configuration.GetConnectionString(clientCode);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // If not found, you might want to build it dynamically or throw an exception
                throw new ArgumentException($"Connection string for client code '{clientCode}' not found.");
            }
            
            return connectionString;
        }

        private IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        #endregion

        #region Default Connection Methods

        public async Task<List<T>> GetList<T>(
            string storedProcedureName,
            DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection(_defaultConnectionString))
            {
                var result = await connection.QueryAsync<T>(
                    storedProcedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                
                return result.ToList();
            }
        }

        public async Task<List<object>> GetMultipleData(
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null)
        {
            var results = new List<object>();

            if (transaction != null)
            {
                using (var multi = await transaction.Connection.QueryMultipleAsync(
                    storedProcedureName,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure))
                {
                    while (!multi.IsConsumed)
                    {
                        var resultSet = await multi.ReadAsync<dynamic>();
                        results.Add(resultSet.ToList());
                    }
                }
            }
            else
            {
                using (var connection = CreateConnection(_defaultConnectionString))
                {
                    using (var multi = await connection.QueryMultipleAsync(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure))
                    {
                        while (!multi.IsConsumed)
                        {
                            var resultSet = await multi.ReadAsync<dynamic>();
                            results.Add(resultSet.ToList());
                        }
                    }
                }
            }

            return results;
        }

        public async Task<T> GetSingleData<T>(
            string storedProcedureName,
            DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection(_defaultConnectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<T>(
                    storedProcedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<T> PostandGetSingleData<T>(
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<T>(
                    storedProcedureName,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                using (var connection = CreateConnection(_defaultConnectionString))
                {
                    return await connection.QuerySingleOrDefaultAsync<T>(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure);
                }
            }
        }

        #endregion

        #region Generic (Multi-Database) Methods

        public async Task<List<T>> GetListGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null)
        {
            var connectionString = GetConnectionString(propCode);
            using (var connection = CreateConnection(connectionString))
            {
                var result = await connection.QueryAsync<T>(
                    storedProcedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                
                return result.ToList();
            }
        }

        public async Task<T> GetSingleDataGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null)
        {
            var connectionString = GetConnectionString(propCode);
            using (var connection = CreateConnection(connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<T>(
                    storedProcedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<T> PostandGetSingleDataGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<T>(
                    storedProcedureName,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                var connectionString = GetConnectionString(propCode);
                using (var connection = CreateConnection(connectionString))
                {
                    return await connection.QuerySingleOrDefaultAsync<T>(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure);
                }
            }
        }

        public async Task<List<object>> GetMultipleDataGen(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null)
        {
            var results = new List<object>();

            if (transaction != null)
            {
                using (var multi = await transaction.Connection.QueryMultipleAsync(
                    storedProcedureName,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure))
                {
                    while (!multi.IsConsumed)
                    {
                        var resultSet = await multi.ReadAsync<dynamic>();
                        results.Add(resultSet.ToList());
                    }
                }
            }
            else
            {
                var connectionString = GetConnectionString(propCode);
                using (var connection = CreateConnection(connectionString))
                {
                    using (var multi = await connection.QueryMultipleAsync(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure))
                    {
                        while (!multi.IsConsumed)
                        {
                            var resultSet = await multi.ReadAsync<dynamic>();
                            results.Add(resultSet.ToList());
                        }
                    }
                }
            }

            return results;
        }

        #endregion
    }
}
