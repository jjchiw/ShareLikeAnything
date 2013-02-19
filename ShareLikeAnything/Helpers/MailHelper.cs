using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace ShareLikeAnything.Helpers
{
	public class MailHelper
	{
		public void SendMessage(List<string> ccAddress, string body, string from)
		{
			MailMessage mail = new MailMessage()
			{
				From = new MailAddress("sla@sharelikeanything.apphb.com"),
			};

			foreach (var cc in ccAddress)
			{
				mail.Bcc.Add(new MailAddress(cc));
			}

			SmtpClient client = new SmtpClient();
			mail.Subject = "Someone has share something in SLA";
			mail.Body = "this is my test email body";
			client.Send(mail);
		}
	}
}