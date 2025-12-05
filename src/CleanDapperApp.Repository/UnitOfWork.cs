using System;
using System.Data;
using CleanDapperApp.Models.Entities;
using CleanDapperApp.Models.Interfaces;
using CleanDapperApp.Repository.Repositories;

namespace CleanDapperApp.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DapperContext _context;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(DapperContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            if (_transaction == null)
            {
                BeginTransaction();
            }
            return new GenericRepository<T>(_transaction!);
        }

        public void BeginTransaction()
        {
            _connection = _context.CreateConnection();
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
