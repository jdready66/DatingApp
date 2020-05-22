using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        private readonly IPhotoService _photoService;
        public AdminController(DataContext context,
            UserManager<User> userManager,
            IMapper mapper,
            IDatingRepository repo,
            IPhotoService photoService)
        {
            _photoService = photoService;
            _repo = repo;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await _context.Users
                .OrderBy(x => x.UserName)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles
                             join role in _context.Roles
                             on userRole.RoleId equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();

            return Ok(userList);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDto.RoleNames;
            selectedRoles = selectedRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos
            .IgnoreQueryFilters()
                .Where(p => p.isApproved == false)
                .OrderBy(p => p.UserId)
                .Select(p => new
                {
                    Id = p.Id,
                    Url = p.Url,
                    UserId = p.UserId,
                    UserName = p.User.UserName
                })
                .ToListAsync();

            if (photos.Count == 0)
                return NoContent();

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{id}")]
        public async Task<IActionResult> ApprovePhoto(int id)
        {
            var photo = await _repo.GetPhoto(id);

            if (photo.isApproved)
                return BadRequest("Photo has already been approved");

            photo.isApproved = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Error approving photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpDelete("rejectPhoto/{id}")]
        public async Task<IActionResult> rejectPhoto(int id)
        {
            return _photoService.WebResponse(await _photoService.DeletePhoto(id));
        }
    }
}