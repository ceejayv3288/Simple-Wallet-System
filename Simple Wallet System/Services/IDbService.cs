namespace Simple_Wallet_System.Services
{
    public interface IDbService
    {
        Task<T> GetAsync<T>(string command, object parms);
        Task<List<T>> GetAll<T>(string command, object parms);
        Task<T> Insert<T>(string command, object parms);
        Task<T> Update<T>(string command, object parms);
    }
}