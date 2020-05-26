using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace DatingApp.API.Services
{
    public class EmailMsgBuilder : IEmailMsgBuilder
    {
        private class EmailAddress
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }

        private EmailAddress _from;
        private List<EmailAddress> _toList;
        private string _subject;
        private string _htmlPart;
        private string _textPart;

        public EmailMsgBuilder()
        {
            _from = null;
            _toList = new List<EmailAddress>();
            _subject = null;
            _textPart = null;
            _htmlPart = null;
        }

        private void CheckNullAndThrow(object value, string name)
        {
            if (value != null)
            {
                throw new System.Exception(string.Format("'{0} has already been set", name));
            }
        }


        public IEmailMsgBuilder AddFrom(string name, string email)
        {
            CheckNullAndThrow(_from, "From Address");
            _from = new EmailAddress()
            {
                Name = name,
                Email = email
            };

            return this;
        }

        public IEmailMsgBuilder AddHtmlPart(string htmlPart)
        {
            CheckNullAndThrow(_htmlPart, "HTML Part");
            _htmlPart = htmlPart;
            return this;
        }

        public IEmailMsgBuilder AddSubject(string subject)
        {
            CheckNullAndThrow(_subject, "Subject");
            _subject = subject;
            return this;
        }

        public IEmailMsgBuilder AddTextPart(string textPart)
        {
            CheckNullAndThrow(_textPart, "Text Part");
            _textPart = textPart;
            return this;
        }

        public IEmailMsgBuilder AddTo(string name, string email)
        {
            var to = new EmailAddress()
            {
                Name = name,
                Email = email
            };
            _toList.Add(to);

            return this;
        }

        public bool ValidateMsg()
        {
            if (_from == null)
            {
                throw new System.Exception("'From Address' is required.");
            }
            if (_toList.Count < 1)
            {
                throw new System.Exception("At least 1 'To Address' is required.");
            }
            if (_subject == null)
            {
                throw new System.Exception("'Subject' is required.");
            }
            if (_textPart == null && _htmlPart == null)
            {
                throw new System.Exception("At least 1 'Text Part' or 'HTML Part' is required.");
            }
            return true;
        }

        public string BuildMsgJson()
        {
            ValidateMsg();

            var msgJson = new StringBuilder("{");

            msgJson.AppendFormat(@"'From': {{'Email': '{0}', 'Name': '{1}'}},", _from.Email, _from.Name);
            msgJson.Append("'To': [");
            var cnt = 0;
            foreach (var to in _toList)
            {
                cnt++;
                if (cnt > 1)
                {
                    msgJson.Append(",");
                }
                msgJson.AppendFormat(@"{{'Email': '{0}', 'Name': '{1}'}}", to.Email, to.Name);
            }
            msgJson.Append("],");

            msgJson.AppendFormat("'Subject': '{0}'", _subject);

            if (_textPart != null)
            {
                msgJson.AppendFormat(",'TextPart': '{0}'", _textPart);
            }

            if (_htmlPart != null)
            {
                msgJson.AppendFormat(",'HTMLPart': '{0}'", _htmlPart);
            }
            msgJson.Append("}");
            return msgJson.ToString();
        }

        public async Task<MailjetResponse> SendMsg()
        {
            MailjetClient client = new MailjetClient("5307cffa9a610ab7795e62a683b948a7",
                "1da9c8a903f145ddb16817a4b1b03e57")
            {
                Version = ApiVersion.V3_1
            };

            MailjetRequest request = new MailjetRequest()
            {
                Resource = Send.Resource
            }
            .Property(Send.Messages, new JArray
            {
                JObject.Parse(BuildMsgJson())
            });

            MailjetResponse response = await client.PostAsync(request);
            return response;
        }
    }
}