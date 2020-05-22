using System.Threading.Tasks;
using Mailjet.Client;

namespace DatingApp.API.Services
{
    public interface IEmailMsgBuilder
    {
        IEmailMsgBuilder AddFrom(string name, string email);
        IEmailMsgBuilder AddTo(string name, string email);
        IEmailMsgBuilder AddSubject(string subject);
        IEmailMsgBuilder AddTextPart(string textPart);
        IEmailMsgBuilder AddHtmlPart(string htmlPart);
        string BuildMsgJson();
        public Task<MailjetResponse> SendMsg();
    }
}