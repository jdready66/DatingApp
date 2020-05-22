using System;
using System.Threading.Tasks;
using DatingApp.API.Services;
using Newtonsoft.Json.Linq;

namespace DatingApp.API.Data
{
    public class TestEmail
    {
        public static async Task RunAsync(IEmailMsgBuilder emailMsgBuilder)
        {
            emailMsgBuilder
                .AddFrom("JD", "jdready@comcast.net")
                .AddTo("JD", "jdready@comcast.net")
                .AddSubject("Test from builder")
                .AddTextPart("This is the text part")
                .AddHtmlPart(@"<h3>Dear passenger 1, welcome to <a href=""https://www.mailjet.com"">Mailjet</a>!</h3><br />May the delivery force be with you!");
            Console.WriteLine("----");
            Console.WriteLine(emailMsgBuilder.BuildMsgJson());
            Console.WriteLine("----");
            Console.WriteLine(JObject.Parse(emailMsgBuilder.BuildMsgJson()).ToString());

            var response = await emailMsgBuilder.SendMsg();
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(response.GetData());
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
            }
        }
    }
}
