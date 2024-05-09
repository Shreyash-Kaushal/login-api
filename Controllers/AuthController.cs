using loginapi.Models;
using loginapi.Repository;
using loginapi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace loginapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserRepository _userRepository;

        public AuthController(JwtService jwtService, UserRepository userRepository)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            int? userId = _userRepository.AuthenticateUser(model.Username, model.Password);

            if (userId.HasValue && userId > 0)
            {
                var token = _jwtService.GenerateToken(userId.Value.ToString());
                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] LoginModel model)
        {
            bool isRegistered = _userRepository.RegisterUser(model.Username, model.Password);
            return Ok(new { IsRegistered = isRegistered });
        }
    }
}
