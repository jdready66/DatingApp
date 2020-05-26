using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.Services;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UsersController(IDatingRepository repo, IMapper mapper, IEmailService emailService)
        {
            _emailService = emailService;
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var allPhotos = (id == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            var user = await _repo.GetUser(id, allPhotos);

            var userToReturn = _mapper.Map<UserForDetailDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id, UserForUpdateDto userForUpdateDto, [FromQuery] string baseClientUrl)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _repo.SaveAll())
            {
                _emailService.SendEmailConfirmationEmail(userFromRepo, baseClientUrl, Url);
                return Ok(_mapper.Map<UserForDetailDto>(userFromRepo));
            }

            throw new System.Exception($"Updating user {id} failed on save");
        }

        [HttpPost("{id}/likes/{likeeId}")]
        public async Task<IActionResult> LikeUser(int id, int likeeId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var existingLike = await _repo.GetLike(id, likeeId);

            if (existingLike != null)
                return BadRequest("You already like this user");

            if (await _repo.GetUser(likeeId) == null)
                return NotFound();

            var like = new Like
            {
                LikerId = id,
                LikeeId = likeeId
            };
            _repo.Add<Like>(like);
            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");
        }

    }
}