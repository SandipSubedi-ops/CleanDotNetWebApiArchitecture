using CleanDapperApp.Models.Entities;

namespace CleanDapperApp.Models.Interfaces
{
    public interface IJwtProvider
    {
        string Generate(User user);
    }
}
