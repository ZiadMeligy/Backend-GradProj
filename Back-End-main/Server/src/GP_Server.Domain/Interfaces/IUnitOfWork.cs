namespace GP_Server.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Dispose();
}
