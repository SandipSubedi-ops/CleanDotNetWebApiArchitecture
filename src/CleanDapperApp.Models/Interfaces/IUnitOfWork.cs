using System;
using CleanDapperApp.Models.Entities;

namespace CleanDapperApp.Models.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
