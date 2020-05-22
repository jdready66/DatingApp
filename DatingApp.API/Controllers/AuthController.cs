using System.Net;
using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using DatingApp.API.Services;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]     // Parameter injection [FromBody] & modelState (ModelState.isvalid) processing
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailMsgBuilder _emailMsgBuilder;

        public AuthController(IConfiguration config, IMapper mapper, UserManager<User> userManager,
                              SignInManager<User> signInManager, IEmailMsgBuilder emailMsgBuilder)
        {
            _emailMsgBuilder = emailMsgBuilder;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _mapper = mapper;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto, [FromQuery]string baseClientUrl)
        {
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            var confirmationToken = _userManager.GenerateEmailConfirmationTokenAsync(userToCreate).Result;
            var confirmationLink = Url.Action("ConfirmEmail",
              "Auth", new
              {
                  userid = userToCreate.Id,
                  token = confirmationToken
              },
               protocol: Request.Scheme);

            var html = string.Format(@"
            Thank you for registering with the site.  Your access to the site will be limited until you verify your 
            eamil address by clicking the link below.<br><br> 
            <a href=""{0}/confirmEmail?address={1}"">Click Here to Verify Your Email Address</a>",
                baseClientUrl, WebUtility.UrlEncode(confirmationLink));

            var msg = _emailMsgBuilder
                .AddFrom("JD", "jdready@comcast.net")
                .AddTo("JD", "jdready@comcast.net")
                .AddSubject("Confirm your email")
                .AddTextPart(confirmationLink)
                .AddHtmlPart(html);
            var rsp = await msg.SendMsg();

            var userToReturn = _mapper.Map<UserForDetailDto>(userToCreate);

            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            //var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);
                return Ok(new
                {
                    token = GenerateJwtToken(user).Result,
                    user = appUser
                });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string userId, string token) {

            var user = _userManager.FindByIdAsync(userId).Result;
            var result = _userManager.ConfirmEmailAsync(user, token).Result;

            if (result.Succeeded)
                return Ok();

            return BadRequest("Problem Confirming Email");
        }
    }
}