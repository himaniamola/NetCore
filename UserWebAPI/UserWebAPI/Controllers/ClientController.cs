using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UserWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly ClientContext _context;
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly SignInManager<IdentityUser> _signInMgr;
        private readonly JwtConfig _jwtConfig;

        public ClientController(ILogger<ClientController> logger, ClientContext context, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr, IOptions<JwtConfig> jwtConfig)
        {
            _context = context;
            _logger = logger;
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpGet]
        public IQueryable<Client> Get()
        {
            return _context.Clients.Select(p => p);
        }

        [HttpPost("RegisterUser")]
        public async Task<object> RegisterUser([FromBody] AddUpdateRegisterUserBindingModel model)
        {
            try
            {
                var user = new IdentityUser() { UserName = model.FullName, Email = model.Email };
                var result = await _userMgr.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return await Task.FromResult("User registered successfully");
                }

                return await Task.FromResult(string.Join(",", result.Errors.Select(p => p.Description.ToArray())));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetAllUsers")]
        public async Task<object> GetAllUsers()
        {
            try
            {
                var allUsers = _userMgr.Users.Select(p => new { FullName=p.UserName, Email = p.Email });
                return await Task.FromResult(allUsers);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<object> Login([FromBody] LoginBindingModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInMgr.PasswordSignInAsync(loginModel.UserName, loginModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        var usr = await _userMgr.FindByNameAsync(loginModel.UserName);
                        var user = new { FullName = usr.UserName, Email = usr.Email, Token = GenerateToken(usr) };
                        return await Task.FromResult(user);
                    }
                }

                return await Task.FromResult("Invalid user name or password");
            }
            catch (Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }
        }

        private string GenerateToken(IdentityUser identityUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Key);
            var tokenDesc = new SecurityTokenDescriptor {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.NameId, identityUser.Id),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, identityUser.Email),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires=DateTime.UtcNow.AddHours(12),
                SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDesc);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
