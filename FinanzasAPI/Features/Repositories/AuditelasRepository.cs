using Core.DTOs;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class AuditelasRepository : IAudiTelasRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringCubo;
        private readonly AxContext _context;

        public AuditelasRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
            _connectionStringCubo = configuracion.GetConnectionString("IMDesarrollos");
        }
        public async Task<List<ObtenerRollosAuditarDTO>> GetRollosAuditar(string RollID, string ApVendRoll, int page, int size)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_BuscarRollos]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@RollId", RollID));
                    if (ApVendRoll == "-")
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
                        while (await reader.ReadAsync())
                        {
                            response.Add(GetRollosAuditar(reader));
                        }
                    }

                    return response;
                }
            }
        }
        public async Task<List<DatosRollosInsertDTOs>> postDatosRollos(List<DatosRollosInsertDTOs> datos)
        {
            var response = new List<DatosRollosInsertDTOs>();

            for (int x = 0; x < datos.Count; x++)
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[IM_InsertarDefectoTelas]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id_Auditor", datos[x].Id_Auditor_Creacion));
                        cmd.Parameters.Add(new SqlParameter("@Id_Rollo", datos[x].Id_Rollo));
                        cmd.Parameters.Add(new SqlParameter("@Id_Estado", datos[x].Id_Estado));
                        cmd.Parameters.Add(new SqlParameter("@Id_Defecto", datos[x].Id_Defecto));
                        cmd.Parameters.Add(new SqlParameter("@Total_Defectos", datos[x].Total_Defectos));
                        cmd.Parameters.Add(new SqlParameter("@Nivel_1", datos[x].Nivel_1));
                        cmd.Parameters.Add(new SqlParameter("@Nivel_2", datos[x].Nivel_2));
                        cmd.Parameters.Add(new SqlParameter("@Nivel_3", datos[x].Nivel_3));
                        cmd.Parameters.Add(new SqlParameter("@Nivel_4", datos[x].Nivel_4));

                        await sql.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getDatosRollosInsert(reader));
                            }
                        }
                    }

                }
            }
            return response;
        }
        public async Task<List<InsertarAnchoYardasDTO>> postInsertarAnchoYardas(List<InsertarAnchoYardasDTO> datos)
        {
            var response = new List<InsertarAnchoYardasDTO>();

            for (int x = 0; x < datos.Count; x++)
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[IM_InsertAncho_Yardas]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@IdRollo", datos[x].Id_Rollo));
                        cmd.Parameters.Add(new SqlParameter("@Ancho_1", datos[x].Ancho_1));
                        cmd.Parameters.Add(new SqlParameter("@Ancho_2", datos[x].Ancho_2));
                        cmd.Parameters.Add(new SqlParameter("@Ancho_3", datos[x].Ancho_3));
                        cmd.Parameters.Add(new SqlParameter("@Yardas_Proveedor", datos[x].Yardas_Proveedor));
                        cmd.Parameters.Add(new SqlParameter("@Yardas_Reales", datos[x].Yardas_Reales));
                        cmd.Parameters.Add(new SqlParameter("@Diferencia_Yardas", datos[x].Diferencia_Yardas));
                        cmd.Parameters.Add(new SqlParameter("@Observaciones", datos[x].Observaciones));

                        await sql.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getInsertAnchoYardas(reader));
                            }
                        }
                    }

                }
            }
            return response;
        }
        public async Task<List<ObtenerDetalleRolloDTO>> getObtenerDetalleRollo(int Id)
        {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDetalleRollo]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@Id", Id));

                        var response = new List<ObtenerDetalleRolloDTO>();

                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getObtenerDetalleRollo(reader));
                            }
                        }
                        return response;
                    }
                }
        }

        public async Task<List<ActualizarRollosDTO>> postActualizarRollos(List<ActualizarRollosDTO> datos)
        {
            var response = new List<ActualizarRollosDTO>();
            for (int x = 0; x < datos.Count; x++)
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[IM_ActualizarRollos]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id_Pieza", datos[x].Id_Pieza));
                        cmd.Parameters.Add(new SqlParameter("@Numero_Rollo_Proveedor", datos[x].Numero_Rollo_Proveedor));
                        cmd.Parameters.Add(new SqlParameter("@Observaciones", datos[x].Observaciones));

                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(postactualizarRollos(reader));
                            }
                        }
                    }
                }
            }
            return response;
        }  
   
public ObtenerDatosDefectosDTO GetDatosDefectos(SqlDataReader reader)
        {
            return new ObtenerDatosDefectosDTO()
            {
                Id = Convert.ToInt32(reader["Id"].ToString()),
                Descripcion = reader["Descripcion"].ToString(),
                Activo = Convert.ToBoolean(reader["Activo"].ToString()),
                Nivel_1 = Convert.ToInt32(reader["Nivel_1"].ToString()),
                Nivel_2 = Convert.ToInt32(reader["Nivel_2"].ToString()),
                Nivel_3 = Convert.ToInt32(reader["Nivel_3"].ToString()),
                Nivel_4 = Convert.ToInt32(reader["Nivel_4"].ToString()),
                Total_Defectos = Convert.ToInt32(reader["Total_Defectos"].ToString())
            };
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
        public DatosRollosInsertDTOs getDatosRollosInsert(SqlDataReader reader)
        {
            return new DatosRollosInsertDTOs()
            {
                Id_Rollo = Convert.ToInt32(reader["Id_Rollo"].ToString()),
            };
        }
        public InsertarAnchoYardasDTO getInsertAnchoYardas(SqlDataReader reader)
        {
            return new InsertarAnchoYardasDTO()
            {
                Id_Rollo = Convert.ToInt32(reader["Id"].ToString()),
            };
        }
        public ActualizarRollosDTO postactualizarRollos(SqlDataReader reader)
        {
            return new ActualizarRollosDTO()
            {
                id = Convert.ToInt32(reader["id"].ToString()),
                Id_Pieza = reader["Id_Pieza"].ToString(),
                Numero_Rollo_Proveedor = reader["Numero_Rollo_Proveedor"].ToString(),
                Observaciones = reader["Observaciones"].ToString()
            };
        }
        public ObtenerDetalleRolloDTO getObtenerDetalleRollo(SqlDataReader reader)
        {
            return new ObtenerDetalleRolloDTO()
            {
                id = Convert.ToInt32(reader["id"].ToString()),
                Id_Pieza = reader["Id_Pieza"].ToString(),
                Numero_Rollo_Proveedor = reader["Numero_Rollo_Proveedor"].ToString(),
                
            };
        }
        public async Task<List<ObtenerDatosDefectosDTO>> GetDatosDefectos(int idRollo)
        {
            var response = new List<ObtenerDatosDefectosDTO>();

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDefectosTelas]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@idrollo", idRollo));

                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(GetDatosDefectos(reader));
                        }
                    }
                }
                return response;
            }
        }

        public Task<List<ObtenerDetalleRolloDTO>> postobtenerDetalleRollo(List<ObtenerDetalleRolloDTO> datos)
        {
            throw new NotImplementedException();
        }

        public async Task<List<InsertarAnchoYardasDTO>> getDatosRollo(string Id_Pieza,string Numero_Rollo_Proveedor)
        {
            var response = new List<InsertarAnchoYardasDTO>();

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDetalleRolloYardas]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Numero_Rollo_Proveedor", Numero_Rollo_Proveedor));
                    cmd.Parameters.Add(new SqlParameter("@Id_Pieza", Id_Pieza));


                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getDetalleRolloYardas(reader));
                        }
                    }
                }
                return response;
            }
        }

        public InsertarAnchoYardasDTO getDetalleRolloYardas(SqlDataReader reader)
        {
            return new InsertarAnchoYardasDTO()
            {
                Id_Rollo = Convert.ToInt32(reader["id"].ToString()),
                Ancho_1 = Convert.ToDecimal(reader["Ancho_1"].ToString()),
                Ancho_2 = Convert.ToDecimal(reader["Ancho_2"].ToString()),
                Ancho_3 = Convert.ToDecimal(reader["Ancho_3"].ToString()),
                Yardas_Reales = Convert.ToDecimal(reader["Yardas_Reales"].ToString()),
                Yardas_Proveedor = Convert.ToDecimal(reader["Yardas_Proveedor"].ToString()),
                Diferencia_Yardas = Convert.ToDecimal(reader["Diferencia_Yardas"].ToString()),
                Observaciones = reader["Observaciones"].ToString()
            };
        }
    }
}

