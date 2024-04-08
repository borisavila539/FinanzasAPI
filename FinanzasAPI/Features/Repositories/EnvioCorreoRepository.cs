using Core.DTOs;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class EnvioCorreoRepository : IEnvioCorreoRepository
    {
        public Boolean postEnvioCorreo(EmailDTO emailDTO)
        {
            string emailOrigen = "sistema@intermoda.com.hn";
            string contrasena = "1nT3rM0d@.Syt3ma1l";

            
            MailMessage OMailMesage = new MailMessage(emailOrigen, emailDTO.To, emailDTO.Asunto, emailDTO.Html);
            OMailMesage.IsBodyHtml = true;
            int cont = 0;
            foreach(var file in emailDTO.fileBytes)
            {
                Attachment tmp = new Attachment(new MemoryStream(file),Path.GetFileName(emailDTO.Attachments[cont]));
                OMailMesage.Attachments.Add(tmp);
                cont++;
            }
           

            try
            {
                SmtpClient oSmtpClient = new SmtpClient();

                oSmtpClient.Host = "smtp.office365.com";
                oSmtpClient.Port = 587;
                oSmtpClient.EnableSsl = true;
                oSmtpClient.UseDefaultCredentials = false;

                NetworkCredential userCredential = new NetworkCredential(emailOrigen, contrasena);

                oSmtpClient.Credentials = userCredential;

                oSmtpClient.Send(OMailMesage);
                oSmtpClient.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
