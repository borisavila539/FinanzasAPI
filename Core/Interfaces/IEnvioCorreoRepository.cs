using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IEnvioCorreoRepository
    {
        public Boolean postEnvioCorreo(EmailDTO emailDTO);
    }
}
