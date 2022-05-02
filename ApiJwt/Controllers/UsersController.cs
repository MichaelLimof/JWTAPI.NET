using ApiJwt.Helpers;
using ApiJwt.Models;
using ApiJwt.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiJwt.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;           
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or Password is Incorrect" });
            
            return Ok(response);
        }


        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            var users = _userService.GetUsers();
            return Ok(users);
        }

        //[Authorize]
        [HttpPost("Register")]
        public IActionResult AddUsers(User user)
        {

            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

             var userAdded= _userService.GravaUsuario(user);

            return Ok(userAdded);
        }

        //[Authorize]
        [HttpGet("Login")]
        public IEnumerable<string> Login(string email, string password)
        {
            var user = _userService.GetByEmail(email);

            if (user.Item1 == null)
                return new string[] { "User Not found" };

            bool isPasswordMatched = Utils.VerifyPassword(password, user.Item2.Hash, user.Item2.Salt);

            if (isPasswordMatched)
            {
                return new string[] { "Login Realizado com Sucesso"};
            }
            else
            {
                return new string[] { "Falha no Login" };
            }

        }

    }
}
