using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Models.DTO;
using ReactApiPract.Services;
using ReactApiPract.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ApiResponse _response;
        private string secretKey;
        readonly UserManager<ApplicationUser> _usermanager;
        readonly RoleManager<IdentityRole> _rolemanager;
        public AuthController(AppDbContext context, ApiResponse response,IConfiguration conf,
            UserManager<ApplicationUser> usermanager, RoleManager<IdentityRole> rolemanager)
        {
            _context = context;
            _response = response;
            secretKey = conf.GetValue<string>("ApiSettings:Secret");
            _usermanager = usermanager;
            _rolemanager = rolemanager;
        }
       
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO register)
        {
            ApplicationUser user = _context.ApplicationUsers
                .FirstOrDefault(q => q.UserName.ToLower() == register.UserName.ToLower());
            if(user != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User already exists");
                return BadRequest(_response);
            }
            try
            {
                ApplicationUser newUser = new()
                {
                    UserName = register.UserName,
                    Email = register.UserName,
                    NormalizedEmail = register.UserName.ToUpper(),
                    Name = register.Name,
                };
                var res = await _usermanager.CreateAsync(newUser, register.Password);
                if (res.Succeeded)
                {
                    if (!await _rolemanager.RoleExistsAsync(SD.Role_Admin)) // .GetAwaiter().GetResult()
                    {
                        await _rolemanager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _rolemanager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    if (register.Role.ToLower() == SD.Role_Admin)
                    {
                        await _usermanager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await _usermanager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);

                }
            }
            catch(Exception)
            {

            }
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Register Error");
            return BadRequest(_response);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO login)
        {
            ApplicationUser user = _context.ApplicationUsers
                .FirstOrDefault(q => q.UserName.ToLower() == login.UserName.ToLower());

            bool isValid = await _usermanager.CheckPasswordAsync(user, login.Password);
            if (!isValid)
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Register Error");
                return BadRequest(_response);
            }


            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            var roles = await _usermanager.GetRolesAsync(user);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullname", user.Name),
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        


            LoginResponseDTO loginResponse = new LoginResponseDTO()
            {
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };
            if(loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("UserName or Password is incorrect");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }


    }
}
