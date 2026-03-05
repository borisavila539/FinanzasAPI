using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    public class PagoBoletinController: ControllerBase
    {
        private readonly IAX _aX;
        public PagoBoletinController( IAX aX)
        {
            _aX = aX;
        }

        [HttpGet("RegistrarBoletin/{JournalID}")]
        public Task<string> GetEnvioAX(string JournalID)
        {
            var resp = _aX.RegistroPagoBoletin(JournalID);

            return resp;
        }

    }
}
