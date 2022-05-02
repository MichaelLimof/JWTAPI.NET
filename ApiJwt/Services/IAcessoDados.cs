using ApiJwt.Models;
using System.Data;

namespace ApiJwt.Services
{
    public interface IAcessoDados
    {
        public DataSet GravaUsuario(User objUser);
    }
}
 