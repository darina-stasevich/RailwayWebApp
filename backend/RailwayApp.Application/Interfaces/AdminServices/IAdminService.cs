namespace RailwayApp.Domain.Interfaces.IServices.AdminServices;

public interface IAdminService<T, TK>
{
    public Task<IEnumerable<T>> GetAllItems();

    public Task<TK> CreateItem(T item);

    public Task<bool> UpdateItem(TK id, T item);

    public Task<TK> DeleteItem(TK id);
}