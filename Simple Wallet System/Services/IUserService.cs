using Simple_Wallet_System.Models;

namespace Simple_Wallet_System.Services
{
    public interface IUserService
    {
        public void ReadAll();
        public void Register(Registration registration);
        public bool IsLoginNameExist(string loginName);
        public int InsertRegistration(Registration registration);
        public User LoginUser(Login login);
    }
}