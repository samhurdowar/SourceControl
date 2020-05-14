using SourceControl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace SourceControl.Services
{
	public class EmailService
	{

		public static void SendEmail(string fromAddress, string toAddress, string cc, string subject, string emailBody, string attachmentFilePath = "")
		{
			try
			{
				MailAddress to = new MailAddress(toAddress);
				MailAddress from = new MailAddress(fromAddress);
				MailMessage mail = new MailMessage(from, to);
				mail.Subject = subject;
                if (cc.Length > 1)
                {
                    mail.CC.Add(cc);
                }
                if (attachmentFilePath.Length > 1)
                {
                    Attachment attachment = new Attachment(attachmentFilePath, MediaTypeNames.Application.Octet);
                    mail.Attachments.Add(attachment);
                }
                mail.IsBodyHtml = true;

				mail.Body = emailBody;
				SmtpClient smtp = new SmtpClient();
				smtp.Host = "mailhost.rdcms.eds.com";
				smtp.Port = 25;
				smtp.Send(mail);
			}
			catch (Exception ex)
			{
				Helper.LogError("SourceControl.Services.EmailService.SendEmail()" + ex.Message);
			}

		}

	}
}