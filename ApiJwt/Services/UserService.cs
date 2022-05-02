using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiJwt.Helpers;
using ApiJwt.Models;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace ApiJwt.Services
{
    public class UserService : IUserService
    {
        public IConfiguration _configuration;
        public string connectionString { get; set; }

        public UserService(IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");

        }

        private List<User> _users = new List<User>
        {
        new User
            {
                Id = 1,FirstName ="NOME02", LastName ="SOBRENOME02", Username ="USERNAME2", Email = "EMAIL@USUARIO2.COM",Password = "SENHA02"
            },
        };

        private readonly AppSettings _appSettings;


        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user == null) return null;

            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        User IUserService.GetById(int id)
        {

            return _users.FirstOrDefault(x => x.Id == id);
        }

        Tuple<User,HashSalt> IUserService.GetByEmail(string email)
        {

            if (email == null) return null;

            User user = new User();
            HashSalt hashSalt = new HashSalt();


            try
            {
                using (SqlConnection connStr = new SqlConnection(connectionString))
                {
                    SqlCommand sqlComm = new SqlCommand($"select * from TB01_USUARIOS WHERE USR_EMAIL  = '{email}' ", connStr);

                    connStr.Open();
                    SqlDataReader reader = sqlComm.ExecuteReader();
                    try
                    {
                        if (reader.Read())
                        {
                            user.Email = reader["USR_EMAIL"].ToString();
                            user.Username = (string)reader["USR_USERNAME"];
                            hashSalt.Hash = (string)reader["USR_HASH"];
                            hashSalt.Salt = (string)reader["USR_SALT"];                      
                        }
                        else
                        {
                            user = null;
                            hashSalt = null;
                        }

                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }

                    return Tuple.Create( user, hashSalt);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        public IEnumerable<User> GetUsers()
        {
            return _users;
        }

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User GravaUsuario(User user)
        {
            DataSet ds = new DataSet();
            if (user == null) return null;

            HashSalt hashSalt = Utils.GenerateSaltedHash(64, user.Password);

            try
            {
                using (SqlConnection connStr = new SqlConnection(connectionString))
                {
                    SqlCommand sqlComm = new SqlCommand("SP_ADD_USER", connStr);
                    sqlComm.Parameters.AddWithValue("@USR_NAME", user.FirstName);
                    sqlComm.Parameters.AddWithValue("@USR_LASTNAME", user.LastName);
                    sqlComm.Parameters.AddWithValue("@USR_USERNAME", user.Username);
                    sqlComm.Parameters.AddWithValue("@USR_EMAIL", user.Email);
                    sqlComm.Parameters.AddWithValue("@USR_HASH", hashSalt.Hash);
                    sqlComm.Parameters.AddWithValue("@USR_SALT", hashSalt.Salt);

                    sqlComm.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds);
                    user.Password = string.Empty;

                    return user;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
