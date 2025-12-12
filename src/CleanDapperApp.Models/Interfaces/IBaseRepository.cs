using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CleanDapperApp.Models.Interfaces
{
    public interface IBaseRepository
    {
        /// <summary>
        /// Begins a transaction on the default connection
        /// </summary>
        IDbTransaction BeginTransaction();

        /// <summary>
        /// Begins a transaction on a connection specific to the property code
        /// </summary>
        IDbTransaction BeginTransactionGen(string propCode);

        /// <summary>
        /// Gets connection string for a specific client code
        /// </summary>
        string GetConnectionString(string clientCode);

        /// <summary>
        /// Executes a stored procedure and returns a list of results
        /// </summary>
        Task<List<T>> GetList<T>(
            string storedProcedureName,
            DynamicParameters parameters = null);

        /// <summary>
        /// Executes a stored procedure and returns multiple result sets
        /// </summary>
        Task<List<object>> GetMultipleData(
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null);

        /// <summary>
        /// Executes a stored procedure and returns a single result
        /// </summary>
        Task<T> GetSingleData<T>(
            string storedProcedureName,
            DynamicParameters parameters = null);

        /// <summary>
        /// Executes a stored procedure (typically INSERT/UPDATE) and returns a single result
        /// </summary>
        Task<T> PostandGetSingleData<T>(
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null);

        /// <summary>
        /// Executes a stored procedure with a specific property code and returns a single result
        /// </summary>
        Task<T> GetSingleDataGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null);

        /// <summary>
        /// Executes a stored procedure (typically INSERT/UPDATE) with a specific property code and returns a single result
        /// </summary>
        Task<T> PostandGetSingleDataGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null);

        /// <summary>
        /// Executes a stored procedure with a specific property code and returns multiple result sets
        /// </summary>
        Task<List<object>> GetMultipleDataGen(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null,
            IDbTransaction transaction = null);

        /// <summary>
        /// Executes a stored procedure with a specific property code and returns a list of results
        /// </summary>
        Task<List<T>> GetListGen<T>(
            string propCode,
            string storedProcedureName,
            DynamicParameters parameters = null);
    }
}
