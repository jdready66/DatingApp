using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Services
{
    public interface IEmailService
    {
         public void SendEmailConfirmationEmail(User user, string baseClientUrl, IUrlHelper url);
    }
}