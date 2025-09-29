using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DescargaDocumentoSLController : ControllerBase
    {
        private readonly IDescargaDocumentosSL _descargaDocumentosSL;

        public DescargaDocumentoSLController(IDescargaDocumentosSL descargaDocumentosSL)
        {
            _descargaDocumentosSL = descargaDocumentosSL;
        }

        [HttpGet("DescargarDocumentos")]
        public async Task<string> DescargarDocumentos()
        {
            var resp = await _descargaDocumentosSL.DescargarDocumentos();
            return resp;
        }
    }
}
