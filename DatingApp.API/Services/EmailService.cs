using System.ComponentModel.DataAnnotations;
using System.Net;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailMsgBuilder _emailMsgBuilder;
        public EmailService(UserManager<User> userManager, IEmailMsgBuilder emailMsgBuilder)
        {
            _emailMsgBuilder = emailMsgBuilder;
            _userManager = userManager;

        }
        public async void SendEmailConfirmationEmail(User user, string baseClientUrl, IUrlHelper url)
        {
            var confirmationToken = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;
            var confirmationLink = url.Action("ConfirmEmail", "Auth", new
            {
                userid = user.Id,
                token = confirmationToken
            },
               protocol: "http");

            var html = string.Format(@"
            Thank you for registering with the site.  Your access to the site will be limited until you verify your 
            eamil address by clicking the link below.<br><br> 
            <a href=""{0}/confirmEmail/{1}"">Click Here to Verify Your Email Address</a>",
                baseClientUrl, WebUtility.UrlEncode(confirmationLink));

            var msg = _emailMsgBuilder
                .AddFrom("JD", "jdready@comcast.net")
                .AddTo(user.KnownAs, user.Email)
                .AddSubject("Confirm your email")
                .AddTextPart(confirmationLink)
                .AddHtmlPart(html);
            var rsp = await msg.SendMsg();
        }

    }
}