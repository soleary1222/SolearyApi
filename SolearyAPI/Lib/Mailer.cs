using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using System.Configuration;

namespace SolearyAPI.Lib
{
    public class Mailer
    {
        public static IRestResponse SendSimpleMessage(string email, int type)
        {
            string subject = "";
            string content = "";
            switch (type)
            {
                case 1: //welcome
                    subject = "Welcome to Soleary's LODS Assessment";
                    content = "You have been registered!";
                    break;
                case 2: //password reset
                    subject = "Your password has been reset";
                    content = "Your new password is: abc123";
                    break;
                case 3: //new company
                    subject = "Thank you for adding your company information";
                    content = "Your company information has been added to Soleary's LODS Assessment";
                    break;
            }

            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                                            ConfigurationManager.AppSettings["MailgunApiKey"]);
            RestRequest request = new RestRequest();
            request.AddParameter("domain", ConfigurationManager.AppSettings["MailgunDomain"], ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", ConfigurationManager.AppSettings["MailgunFromName"] + " <" + ConfigurationManager.AppSettings["MailgunFromEmail"] + ">");
            request.AddParameter("to", email);
            request.AddParameter("subject", subject);
            request.AddParameter("text", content);
            request.Method = Method.POST;
            return client.Execute(request);
        }
    }
}