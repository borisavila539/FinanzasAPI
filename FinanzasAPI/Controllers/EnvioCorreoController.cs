using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvioCorreoController : ControllerBase
    {
        private readonly IEnvioCorreoRepository _envioCorreoRepository;

        public EnvioCorreoController(IEnvioCorreoRepository envioCorreoRepository)
        {
            _envioCorreoRepository = envioCorreoRepository;
        }
        [HttpPost("EnvioCorreo")]
        public  Boolean postEnvioCorreo(EmailDTO OMailMessage)
        {
            var resp = _envioCorreoRepository.postEnvioCorreo(OMailMessage);

            return resp;
        }

    }
}
