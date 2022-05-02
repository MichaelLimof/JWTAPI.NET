using ApiJwt.Models;
using System.Data;

namespace ApiJwt.Services
{

    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetUsers();
        User GetById(int id);
        Tuple<User, HashSalt> GetByEmail(string email);
        User GravaUsuario(User objUser);
    }

}
