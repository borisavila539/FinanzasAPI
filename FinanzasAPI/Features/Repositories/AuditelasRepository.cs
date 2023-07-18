using Core.DTOs;
using Core.Interfaces;
using System.Collections.Generic;
using Infraestructure.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace FinanzasAPI.Features.Repositories
{
    public class AuditelasRepository : IAudiTelasRepository
    {
        private readonly string _connectionString;
        private readonly AxContext _context;

        public AuditelasRepository (AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
        }
        public async Task<List<ObtenerRollosAuditarDTO>> GetRollosAuditar(string RollID, string ApVendRoll, int page, int size)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_BuscarRollos]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@RollId", RollID));
                    if(ApVendRoll == "-")
                    {
                        cmd.Parameters.Add(new SqlParameter("@ApVendRoll", ""));
                    }
                    else
                    {
                        cmd.Parameters.Add(new SqlParameter("@ApVendRoll", ApVendRoll));
                    }
                    cmd.Parameters.Add(new SqlParameter("@page", page));
                    cmd.Parameters.Add(new SqlParameter("@size", size));

                    var response = new List<ObtenerRollosAuditarDTO>();

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            response.Add(GetRollosAuditar(reader));
                        }
                    }

                    return response;
                }
            }
        }

        public ObtenerRollosAuditarDTO GetRollosAuditar(SqlDataReader reader)
        {
            return new ObtenerRollosAuditarDTO()
            {
                RollId = reader["RollId"].ToString(),
                ApVendRoll = reader["ApVendRoll"].ToString(),
                NameAlias = reader["NameAlias"].ToString()
            };
        }
    }
}
