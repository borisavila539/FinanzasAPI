﻿using Core.DTOs;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class AuditelasRepository : IAudiTelasRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringCubo;
        private readonly AxContext _context;

        public AuditelasRepository()
        {
            
        }
        public AuditelasRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
            _connectionStringCubo = configuracion.GetConnectionString("IMFinanzas");
        }
        public async Task<List<ObtenerRollosAuditarDTO>> GetRollosAuditar(string RollID, string ApVendRoll, string importacion, string tela, int page, int size)
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
                    cmd.Parameters.Add(new SqlParameter("@importacion", importacion));
                    cmd.Parameters.Add(new SqlParameter("@tela", tela));
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
                NameAlias = reader["NameAlias"].ToString(),
                INVENTBATCHID = reader["INVENTBATCHID"].ToString()
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

        public async Task<List<InsertarAnchoYardasDTO>> getDatosRollo(string Id_Pieza, string Numero_Rollo_Proveedor)
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

        public async Task<List<PruebaCalidadDTO>> postPruebasCalidad(List<PruebaCalidadDTO> datos)
        {
            var response = new List<PruebaCalidadDTO>();
            using (SqlConnection sql1 = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd1 = new SqlCommand("[IM_InsertPruebaCalidadTela]", sql1))
                {
                    var rollo = new List<ObtenerDetalleRolloDTO>();
                    rollo = await getObtenerDetalleRollo(datos[0].Id_Rollo);
                    cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd1.Parameters.Add(new SqlParameter("@rollid", rollo[0].Id_Pieza));
                    cmd1.Parameters.Add(new SqlParameter("@trama1", datos[0].Trama1));
                    cmd1.Parameters.Add(new SqlParameter("@trama2", datos[0].Trama2));
                    cmd1.Parameters.Add(new SqlParameter("@trama3", datos[0].Trama3));
                    cmd1.Parameters.Add(new SqlParameter("@undimbre1", datos[0].undimbre1));
                    cmd1.Parameters.Add(new SqlParameter("@undimbre2", datos[0].undimbre2));
                    cmd1.Parameters.Add(new SqlParameter("@undimbre3", datos[0].undimbre3));
                    cmd1.Parameters.Add(new SqlParameter("@torsionAC", datos[0].Torsion_AC));
                    cmd1.Parameters.Add(new SqlParameter("@torsionBD", datos[0].Torsion_BD));

                    await sql1.OpenAsync();
                    using (var reader1 = await cmd1.ExecuteReaderAsync())
                    {
                        while (await reader1.ReadAsync())
                        {
                            //response.Add(getListaPruebasCalidad(reader));
                        }
                    }

                }
            }

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_InsertPruebaCalidad]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@rolloid", datos[0].Id_Rollo));
                    cmd.Parameters.Add(new SqlParameter("@usuarioID", datos[0].UsuarioID));
                    cmd.Parameters.Add(new SqlParameter("@trama1", datos[0].Trama1));
                    cmd.Parameters.Add(new SqlParameter("@trama2", datos[0].Trama2));
                    cmd.Parameters.Add(new SqlParameter("@trama3", datos[0].Trama3));
                    cmd.Parameters.Add(new SqlParameter("@undimbre1", datos[0].undimbre1));
                    cmd.Parameters.Add(new SqlParameter("@undimbre2", datos[0].undimbre2));
                    cmd.Parameters.Add(new SqlParameter("@undimbre3", datos[0].undimbre3));
                    cmd.Parameters.Add(new SqlParameter("@torsionAC", datos[0].Torsion_AC));
                    cmd.Parameters.Add(new SqlParameter("@torsionBD", datos[0].Torsion_BD));

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaPruebasCalidad(reader));
                        }
                    }
                }
            }
            return response;

        }
        public PruebaCalidadDTO getListaPruebasCalidad(SqlDataReader reader)
        {
            return new PruebaCalidadDTO()
            {
                Id_Rollo = Convert.ToInt32(reader["Id_Rollo"].ToString()),
                Trama1 = Convert.ToDouble(reader["Trama1"].ToString()),
                Trama2 = Convert.ToDouble(reader["Trama2"].ToString()),
                Trama3 = Convert.ToDouble(reader["Trama3"].ToString()),
                undimbre1 = Convert.ToDouble(reader["undimbre1"].ToString()),
                undimbre2 = Convert.ToDouble(reader["undimbre2"].ToString()),
                undimbre3 = Convert.ToDouble(reader["undimbre3"].ToString()),
                Torsion_AC = Convert.ToDouble(reader["Torsion_AC"].ToString()),
                Torsion_BD = Convert.ToDouble(reader["Torsion_BD"].ToString()),
                UsuarioID = Convert.ToInt32(reader["UsuarioID"].ToString())
            };

        }

        public async Task<List<PruebaCalidadDTO>> getPruebasCalidad(int id_rollo)
        {
            var response = new List<PruebaCalidadDTO>();

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_Obtener_Prueba_Calidad]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@rolloid", id_rollo));

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaPruebasCalidad(reader));
                        }
                    }
                }
            }
            return response;
        }

        public async Task<List<AnchoRolloDTO>> setAnchoRollo(string Rollid, decimal Width)
        {
            var response = new List<AnchoRolloDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ActualizarAnchoRollo]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@RollId", Rollid));
                    cmd.Parameters.Add(new SqlParameter("@width", Width));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaAnchoRollo(reader));
                        }
                    }
                }
            }

            return response;
        }
        public AnchoRolloDTO getListaAnchoRollo(SqlDataReader reader)
        {
            return new AnchoRolloDTO()
            {
                Rollid = reader["ROLLID"].ToString(),
                Width = Convert.ToDecimal(reader["WIDTH"].ToString())
            };
        }

        public async  Task<List<RollosImporteLoteDTO>> getRollosImporteLote(string Importacion, string Lote,string tela)
        {
            var response = new List<RollosImporteLoteDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_BuscarRollosImportLote]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@importacion", Importacion));
                    cmd.Parameters.Add(new SqlParameter("@lote", Lote));
                    cmd.Parameters.Add(new SqlParameter("@tela", tela));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaRolloImporteLote(reader));
                        }
                    }
                }
            }

            return response;
        }
        public RollosImporteLoteDTO getListaRolloImporteLote(SqlDataReader reader)
        {
            return new RollosImporteLoteDTO()
            {
                ROLLID = reader["ROLLID"].ToString(),
                APVENDROLL = reader["APVENDROLL"].ToString(),
                NAMEALIAS = reader["NAMEALIAS"].ToString(),
                INVENTBATCHID = reader["INVENTBATCHID"].ToString(),

            };
        }

        public async Task<List<PruebaCalidadLoteDTO>> postPruebaCalidadLote(List<PruebaCalidadLoteDTO> datos)
        {
            var response = new List<PruebaCalidadLoteDTO>();
            int cont = 0;
            try
            {
                datos.ForEach(async (x) => {
                    using (SqlConnection sql1 = new SqlConnection(_connectionString))
                    {
                        using (SqlCommand cmd1 = new SqlCommand("[IM_InsertPruebaCalidadTela]", sql1))
                        {

                            cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd1.Parameters.Add(new SqlParameter("@rollid", x.Id_Rollo));
                            cmd1.Parameters.Add(new SqlParameter("@trama1", x.Trama1));
                            cmd1.Parameters.Add(new SqlParameter("@trama2", x.Trama2));
                            cmd1.Parameters.Add(new SqlParameter("@trama3", x.Trama3));
                            cmd1.Parameters.Add(new SqlParameter("@undimbre1", x.undimbre1));
                            cmd1.Parameters.Add(new SqlParameter("@undimbre2", x.undimbre2));
                            cmd1.Parameters.Add(new SqlParameter("@undimbre3", x.undimbre3));
                            cmd1.Parameters.Add(new SqlParameter("@torsionAC", x.Torsion_AC));
                            cmd1.Parameters.Add(new SqlParameter("@torsionBD", x.Torsion_BD));

                            await sql1.OpenAsync();
                            using (var reader1 = await cmd1.ExecuteReaderAsync())
                            {
                                while (await reader1.ReadAsync())
                                {
                                    cont++;
                                }
                            }
                            await sql1.CloseAsync();


                        }
                    }
                });


                datos.ForEach(x =>
                {
                    response.Add(x);
                });
                return response;
            }
            catch (Exception err)
            {
                return null;
            }         
            
            
        }

        public async Task<IM_ObtenerDatosRollo> Get_ObtenerDatosRollo(string Rollo)
        {
            var response = new IM_ObtenerDatosRollo();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDatosRollo]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Rollo", Rollo));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response = Get_ObtenerDatosRolloLine(reader);
                        }
                    }
                }
            }

            return response;
        }
        public IM_ObtenerDatosRollo Get_ObtenerDatosRolloLine(SqlDataReader reader)
        {
            return new IM_ObtenerDatosRollo()
            {
                Ancho_1 = Convert.ToDecimal(reader["Ancho_1"].ToString()),
                Ancho_2 = Convert.ToDecimal(reader["Ancho_2"].ToString()),
                Ancho_3 = Convert.ToDecimal(reader["Ancho_3"].ToString()),
                Yardas_Proveedor = Convert.ToDecimal(reader["Yardas_Proveedor"].ToString()),
                Yardas_Reales = Convert.ToDecimal(reader["Yardas_Reales"].ToString())
            };
        }

        public async Task<string> GetimprimirEtiquetaRollo(string Rollo)
        {
            var data = new IM_Auditela_Etiqueta_Rollo();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_Auditela_Etiqueta_Rollo]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Rollo", Rollo));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                           data = GetimprimirEtiquetaRolloLines(reader);
                        }
                    }
                }
            }
            string etiqueta = "";

            etiqueta += "^XA^CF0,22^BY3,2,100";
            etiqueta += $"^FO200,50^BC^FD{data.INVENTSERIALID}^FS";
            etiqueta += $"^FO50,190^FDProveedor: {data.Proveedor}^FS";
            etiqueta += $"^FO50,220^FDNo. Rollo Proveedor: {data.APVENDROLL}^FS";
            etiqueta += $"^FO500,220^FDCantidad: {Math.Round(data.AVAILPHYSICAL,2)} {(data.ITEMID.Substring(0, 2) == "45" ? "lb" : "yd")}^FS";
            etiqueta += $"^FO50,250^FDTela: {data.ITEMID}^FS";
            etiqueta += $"^FO500,250^FDColor: {data.COLOR}^FS";
            etiqueta += $"^FO50,280^FDLote: {data.INVENTBATCHID}^FS";
            etiqueta += $"^FO500,280^FDTela: {data.Tela}^FS";
            etiqueta += $"^FO50,310^FDAuditor: {data.Auditor}^FS";
            etiqueta += $"^FO500,310^FDFecha: {DateTime.Now.ToString("dd/MM/yyyy")}^FS";
            etiqueta += $"^FO50,340^FDPPM2: {Math.Round(data.APPMts, 2)}^FS";
            etiqueta+= $"^FO50,370^FDComentario: {data.Comentario}^FS^PQ2^XZ";

            try
            {
                using (TcpClient client = new TcpClient("10.1.1.176", 9100))//176
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(etiqueta);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                return "OK";
            }
            catch (Exception err)
            {
                return err.ToString();
            }
            return "OK";
        }
        public IM_Auditela_Etiqueta_Rollo GetimprimirEtiquetaRolloLines(SqlDataReader reader)
        {
            return new IM_Auditela_Etiqueta_Rollo()
            {
                INVENTSERIALID = reader["INVENTSERIALID"].ToString(),
                APVENDROLL = reader["APVENDROLL"].ToString(),
                AVAILPHYSICAL = Convert.ToDecimal(reader["AVAILPHYSICAL"].ToString()),
                ITEMID = reader["ITEMID"].ToString(),
                COLOR = reader["COLOR"].ToString(),
                INVENTBATCHID = reader["INVENTBATCHID"].ToString(),
                CONFIGID = reader["CONFIGID"].ToString(),
                Auditor = reader["Auditor"].ToString(),
                APPMts = Convert.ToDecimal(reader["APPMts"].ToString()),
                Comentario = reader["Comentario"].ToString(),
                Proveedor = reader["Proveedor"].ToString(),
                Tela = reader["Tela"].ToString()

            };
        }
    }

}

