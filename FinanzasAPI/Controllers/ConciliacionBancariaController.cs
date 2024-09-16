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
    public class ConciliacionBancariaController : ControllerBase
    {
        private readonly IConciliacionBancaria _conciliacionBancaria;
        public ConciliacionBancariaController(IConciliacionBancaria conciliacionBancaria)
        {
            _conciliacionBancaria = conciliacionBancaria;
        }
        [HttpGet("CreateReconciliationFolders/{path}/{date}/{dataAreaId}")]
        public async Task<string> CreateReconciliationFolders(string path, string date, string dataAreaId)
        {
            string response = await _conciliacionBancaria.CreatereConciliacionFolders(path, date, dataAreaId);
            return response;
        }
    }
}
