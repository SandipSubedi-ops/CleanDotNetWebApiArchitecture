using Dapper;
using CleanDapperApp.Models.Entities;
using CleanDapperApp.Models.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CleanDapperApp.Repository.Services
{
    /// <summary>
    /// Example service demonstrating how to use the BaseRepository
    /// </summary>
    public class SampleService
    {
        private readonly IBaseRepository _baseRepository;

        public SampleService(IBaseRepository baseRepository)
        {
            _baseRepository = baseRepository;
        }

        #region Basic Usage Examples

        /// <summary>
        /// Example: Get a list of items using stored procedure
        /// </summary>
        public async Task<List<User>> GetAllUsersExample()
        {
            var parameters = new DynamicParameters();
            // Add parameters if needed
            // parameters.Add("@Status", "Active");

            return await _baseRepository.GetList<User>(
                "sp_GetAllUsers",
                parameters);
        }

        /// <summary>
        /// Example: Get a single item using stored procedure
        /// </summary>
        public async Task<User> GetUserByIdExample(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await _baseRepository.GetSingleData<User>(
                "sp_GetUserById",
                parameters);
        }

        /// <summary>
        /// Example: Insert/Update and return result
        /// </summary>
        public async Task<int> CreateUserExample(string email, string userName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            parameters.Add("@UserName", userName);
            parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _baseRepository.PostandGetSingleData<int>(
                "sp_CreateUser",
                parameters);

            return parameters.Get<int>("@UserId");
        }

        #endregion

        #region Transaction Examples

        /// <summary>
        /// Example: Using transactions with default connection
        /// </summary>
        public async Task<bool> CreateUserWithTransactionExample(string email, string userName)
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = _baseRepository.BeginTransaction();

                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                parameters.Add("@UserName", userName);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _baseRepository.PostandGetSingleData<int>(
                    "sp_CreateUser",
                    parameters,
                    transaction);

                var userId = parameters.Get<int>("@UserId");

                // Additional operations within the same transaction
                var auditParams = new DynamicParameters();
                auditParams.Add("@UserId", userId);
                auditParams.Add("@Action", "UserCreated");

                await _baseRepository.PostandGetSingleData<int>(
                    "sp_LogAudit",
                    auditParams,
                    transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        #endregion

        #region Multi-Database (Generic) Examples

        /// <summary>
        /// Example: Get data from a specific client database using propCode
        /// </summary>
        public async Task<List<User>> GetUsersFromClientDatabaseExample(string clientCode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Status", "Active");

            return await _baseRepository.GetListGen<User>(
                clientCode, // This should match a connection string name in appsettings.json
                "sp_GetAllUsers",
                parameters);
        }

        /// <summary>
        /// Example: Transaction on a specific client database
        /// </summary>
        public async Task<bool> CreateUserInClientDatabaseExample(string clientCode, string email, string userName)
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = _baseRepository.BeginTransactionGen(clientCode);

                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                parameters.Add("@UserName", userName);

                var result = await _baseRepository.PostandGetSingleDataGen<int>(
                    clientCode,
                    "sp_CreateUser",
                    parameters,
                    transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        #endregion

        #region Multiple Result Sets Example

        /// <summary>
        /// Example: Get multiple result sets from a single stored procedure
        /// Note: This uses dynamic types. For production, create DTOs matching your result sets.
        /// </summary>
        public async Task<(List<User> activeUsers, List<User> inactiveUsers)> GetMultipleResultSetsExample()
        {
            var parameters = new DynamicParameters();

            var results = await _baseRepository.GetMultipleData(
                "sp_GetActiveAndInactiveUsers",
                parameters);

            // Assuming the stored procedure returns active users first, then inactive users
            var activeUsers = ((IEnumerable<dynamic>)results[0])
                .Select(x => new User 
                { 
                    Id = x.Id, 
                    Email = x.Email,
                    Username = x.Username,
                    PasswordHash = x.PasswordHash,
                    Role = x.Role
                }).ToList();

            var inactiveUsers = ((IEnumerable<dynamic>)results[1])
                .Select(x => new User 
                { 
                    Id = x.Id, 
                    Email = x.Email,
                    Username = x.Username,
                    PasswordHash = x.PasswordHash,
                    Role = x.Role
                }).ToList();

            return (activeUsers, inactiveUsers);
        }

        #endregion
    }
}
