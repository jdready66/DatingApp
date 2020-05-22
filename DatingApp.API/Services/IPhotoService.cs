using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Services
{
    public enum DeletePhotoResponse
    {
        Ok,
        CannotDeleteMain,
        FailedToDelete
    }
    public interface IPhotoService
    {
         public Task<DeletePhotoResponse> DeletePhoto(int photoId);

         public IActionResult WebResponse(DeletePhotoResponse response);

         public ImageUploadResult UploadPhoto(PhotoForCreationDto photoForCreationDto);
    }
}