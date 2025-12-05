using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CleanDapperApp.Models.Entities;
using CleanDapperApp.Models.Interfaces;

namespace CleanDapperApp.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;

        public GenericRepository(IDbTransaction transaction)
        {
            _transaction = transaction;
            _connection = transaction.Connection!;
        }

        private string TableName => typeof(T).Name + "s"; // Convention: Entity 'User' -> Table 'Users'

        public async Task<T?> GetByIdAsync(int id)
        {
            var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
            return await _connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id }, _transaction);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            var sql = $"SELECT * FROM {TableName}";
            var result = await _connection.QueryAsync<T>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<int> AddAsync(T entity)
        {
            // Simple generic insert. For production, consider a more robust query builder or Dapper.Contrib
            var properties = typeof(T).GetProperties().Where(p => p.Name != "Id" && p.Name != "UpdatedAt");
            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => "@" + p.Name));

            var sql = $"INSERT INTO {TableName} ({columns}) VALUES ({values}); SELECT CAST(SCOPE_IDENTITY() as int)";
            return await _connection.ExecuteScalarAsync<int>(sql, entity, _transaction);
        }

        public async Task<int> UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var properties = typeof(T).GetProperties().Where(p => p.Name != "Id" && p.Name != "CreatedAt");
            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            var sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";
            return await _connection.ExecuteAsync(sql, entity, _transaction);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
            return await _connection.ExecuteAsync(sql, new { Id = id }, _transaction);
        }
    }
}
