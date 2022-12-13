using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtWebApi.Configurations;
using JwtWebApi.Models;
using JwtWebApi.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JwtWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exists
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return BadRequest(new AuthResult
                    {
                        Result = false,
                        Errors = new List<string> { "Invalid authentication request" }
                    });
                }

                // Check if the password is correct
                var isCorrect = await _userManager.CheckPasswordAsync(user, request.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult
                    {
                        Result = false,
                        Errors = new List<string> { "Invalid credentials" }
                    });
                }

                var token = GenerateJwtToken(user);
                return Ok(new AuthResult
                {
                    Result = true,
                    Token = token
                });

            }

            return BadRequest(new AuthResult
            {
                Result = false,
                Errors = new List<string> { "Invalid authentication request" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(request.Email);

                if (userExists != null)
                {
                    return BadRequest(new AuthResult
                    {
                        Result = false,
                        Errors = new List<string> { "User with this email already exists" }
                    });
                }

                var user = new IdentityUser()
                {
                    Email = request.Email,
                    UserName = request.Email
                };

                var isCreated = await _userManager.CreateAsync(user, request.Password);

                if (isCreated.Succeeded)
                {
                    //generate token on successfull user creation
                    var token = GenerateJwtToken(user);
                    return Ok(new AuthResult
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = new List<string> { "Server Error: User creation failed" }
                });
            }

            return BadRequest();
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.GetSection("JwtConfig").GetSection("Secret").Value);

            //token descriptor

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]{
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                }),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            //convert token to string
            string jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }

    }
}