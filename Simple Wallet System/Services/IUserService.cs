using Simple_Wallet_System.Models;

namespace Simple_Wallet_System.Services
{
    public interface IUserService
    {
        public bool Register(Registration registration);
        public bool IsLoginNameExist(string loginName);
        public bool InsertRegistration(Registration registration);
        public User LoginUser(Login login);
    }
}