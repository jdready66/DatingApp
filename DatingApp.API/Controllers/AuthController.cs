using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly IEmailService _emailService;

        public AuthController(IConfiguration config, IMapper mapper, UserManager<User> userManager,
                              SignInManager<User> signInManager, IEmailService emailService)
        {
            _emailService = emailService;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _mapper = mapper;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto, [FromQuery] string baseClientUrl)
        {
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
            if (!result.Succeeded)
                return BadRequest("Error creating user account");

            var user = await _userManager.FindByNameAsync(userToCreate.UserName);

            await _userManager.AddToRolesAsync(user, new[] { "Member" });

            _emailService.SendEmailConfirmationEmail(user, baseClientUrl, Url);

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
            if (user == null)
                return Unauthorized();

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

        [Authorize(Roles = "Manager")]
        [HttpGet("impersonateLogin/{id}")]
        public async Task<IActionResult> ImpersonateLogin(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest("User not found.");

            var appUser = _mapper.Map<UserForListDto>(user);
            return Ok(new
            {
                token = GenerateJwtToken(user).Result,
                user = appUser
            });
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

        [Authorize(Roles = "Member")]
        [HttpGet("resendConfirmation/{id}")]
        public async Task<IActionResult> ResendConfirmationEmail(int id, [FromQuery] string baseClientUrl)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(id.ToString());
            _emailService.SendEmailConfirmationEmail(user, baseClientUrl, Url);

            return Ok();
        }

        [HttpGet("ConfirmEmail")]
        public IActionResult ConfirmEmail(string userId, string token)
        {

            var user = _userManager.FindByIdAsync(userId).Result;

            if (user.EmailConfirmed)
                return BadRequest("Email already confirmed");

            var result = _userManager.ConfirmEmailAsync(user, token).Result;

            if (result.Succeeded)
                return Ok();

            return BadRequest("Problem Confirming Email");
        }

        [HttpGet("GetUserByUsername/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return Ok(user);
        }

        [HttpGet("GetUserByEmail/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(user);
        }

        [HttpGet("sendPasswordResetLink/{email}")]
        public async Task<IActionResult> SendPasswordResetLink(string email, [FromQuery] string baseClientUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return BadRequest("Email address not found.");

            _emailService.SendPasswordResetEmail(user, baseClientUrl, Url);
            return Ok();
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return BadRequest("User not found");

            var resetPasswordResult =
                await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

            if (resetPasswordResult.Succeeded)
                return Ok();

            return BadRequest(resetPasswordResult.Errors);
        }
    }
}