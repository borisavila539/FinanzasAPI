using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditelasController: ControllerBase
    {
        private readonly IAudiTelasRepository _audiTelasRepository;

        public AuditelasController(IAudiTelasRepository audiTelasRepository)
        {
            _audiTelasRepository = audiTelasRepository;
        }

        [HttpGet("{RollID}/{ApVendRoll}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<ObtenerRollosAuditarDTO>>> getRollosAuditar(string RollID, string ApVendRoll, int page, int size)
        {
            var resp = await _audiTelasRepository.GetRollosAuditar(RollID,ApVendRoll,page,size);
            return resp;
        }

    }
}
