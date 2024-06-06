

using Core.DTOs;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;


namespace FinanzasAPI.Features.Repositories
{
    public class PantsQualityRepository : IPantsQualityRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringCubo;
        private readonly string _connectionStringPLM;


        private readonly AxContext _context;
        private List<OrdenesInicialdasDTO> ordenesInicialdas = new List<OrdenesInicialdasDTO>();
        
        ExcelWorksheet worksheet = null;

        public PantsQualityRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
            _connectionStringCubo = configuracion.GetConnectionString("IMFinanzas");
            _connectionStringPLM = configuracion.GetConnectionString("PLM_PRO");

        }
        public async Task<List<OrdenesInicialdasDTO>> GetOrdenesInicialdas(int page, int size, string filtro)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerOrdenesIniciadas]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@page", page));
                    cmd.Parameters.Add(new SqlParameter("@size", size));
                    cmd.Parameters.Add(new SqlParameter("@filtro", filtro));


                    var response = new List<OrdenesInicialdasDTO>();
                    await sql.OpenAsync();

                    using(var reader = await cmd.ExecuteReaderAsync())
                    {
                        while( await reader.ReadAsync())
                        {
                            response.Add(getListaOrdenesIniciadas(reader));
                        }
                    }
                    return response;
                }
            }
        }

        public OrdenesInicialdasDTO getListaOrdenesIniciadas(SqlDataReader reader)
        {
            return new OrdenesInicialdasDTO()
            {
                PRODMASTERREFID = reader["PRODMASTERREFID"].ToString(),
                PRODMASTERID = reader["PRODMASTERID"].ToString(),
                ITEMID = reader["ITEMID"].ToString(),
                LOWESTSTATUS = reader["LOWESTSTATUS"].ToString()
            };
        }

        public int GetColumna(string talla, int columna, int fila)
        {
            for (int i = columna; i <= 100; i++)
            {
                var cellvalue = "";
                try
                {
                    cellvalue = (string)worksheet.Cells[fila, i].Value;
                }
                catch (Exception)
                {
                    cellvalue = Convert.ToString((double)worksheet.Cells[fila, i].Value);
                }

                if (cellvalue == talla)
                {
                    return i;
                    i = 500;
                }

            }
            return 1;
        }

        public async Task<List<MaestroOrdenes>> GetOrdenInicialda(string prodmasterrefid, string prodmasterid, string itemid, int estado)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMasterOrden]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@prodmasterrefid", prodmasterrefid));
                    cmd.Parameters.Add(new SqlParameter("@prodmasterid", prodmasterid));
                    cmd.Parameters.Add(new SqlParameter("@itemid", itemid));
                    cmd.Parameters.Add(new SqlParameter("@estado", estado));



                    var response = new List<MaestroOrdenes>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaMasterOrden(reader));
                        }
                    }
                    return response;
                }
            }
        }
        public MaestroOrdenes getListaMasterOrden(SqlDataReader reader)
        {
            return new MaestroOrdenes()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                prodmasterid = reader["prodmasterid"].ToString(),
                prodmasterrefid = reader["prodmasterrefid"].ToString(),
                itemid = reader["itemid"].ToString(),
                posted = Convert.ToInt32(reader["posted"].ToString()),
                userid = Convert.ToInt32(reader.IsDBNull("usuarioid") ? 0 : reader["usuarioid"].ToString())
            };
        }

        public async Task<List<ItemTallasDTOS>> GetItemTallas(string itemid, string prodmasterrefid)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerTallasOrdenes]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@itemid", itemid));
                    cmd.Parameters.Add(new SqlParameter("@Orden", prodmasterrefid));




                    var response = new List<ItemTallasDTOS>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaItemtallas(reader));
                        }
                    }                    
                    
                    return response;
                }
            }
        }

        public ItemTallasDTOS getListaItemtallas(SqlDataReader reader)
        {
            return new ItemTallasDTOS()
            {
                ITEMID = reader["ITEMID"].ToString(),
                SET_TALLAS = reader["SET_TALLAS"].ToString(),
                SIZECHARTID = reader["SIZECHARTID"].ToString(),
                SIZEID = reader["SIZEID"].ToString()

            };
        }

        public async Task<List<usuariosDTOs>> GetUsuarios(string user, string pass)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ValidarUsuario]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@user", user));
                    cmd.Parameters.Add(new SqlParameter("@password", pass));


                    var response = new List<usuariosDTOs>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaUsuarios(reader));
                        }
                    }
                    return response;
                }
            }
        }
        public usuariosDTOs getListaUsuarios(SqlDataReader reader)
        {
            return new usuariosDTOs()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                usuario = reader["usuario"].ToString(),
                password = reader["password"].ToString(),
                activo = Convert.ToBoolean(reader["activo"].ToString()),
                rol = Convert.ToString(reader["rol"].ToString())

            };
        }

        public async Task<List<MedidasDTOs>> GetMedidas(int Tipo)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMedidasCalidad]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Tipo", Tipo));

                    var response = new List<MedidasDTOs>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaMedidas(reader));
                        }
                    }
                    return response;
                }
            }
        }
        public MedidasDTOs getListaMedidas(SqlDataReader reader)
        {
            return new MedidasDTOs()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                Nombre = reader["nombre"].ToString(),
                Link = reader["Link"].ToString(),
                activo = Convert.ToBoolean(reader["activo"].ToString()),
            };
        }

        public async Task<List<MedidasInsertDTOs>> postMedidasCalidad(List<MedidasInsertDTOs> datos)
        {
            var response = new List<MedidasInsertDTOs>();

            for (int x = 0; x < datos.Count; x++)
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[IM_InsertarMedidasCalidad]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@masterID", datos[x].idMasterOrden));
                        cmd.Parameters.Add(new SqlParameter("@LavadoID", datos[x].lavadoID));
                        cmd.Parameters.Add(new SqlParameter("@MedidaId", datos[x].idMedida));
                        cmd.Parameters.Add(new SqlParameter("@Usuario", datos[x].usuarioID));
                        cmd.Parameters.Add(new SqlParameter("@IdTalla", datos[x].idTalla));
                        cmd.Parameters.Add(new SqlParameter("@medida", datos[x].Medida));
                        cmd.Parameters.Add(new SqlParameter("@medidaNumerador", datos[x].MedidaNumerador.Length > 0 ? datos[x].MedidaNumerador : 0));
                        cmd.Parameters.Add(new SqlParameter("@moduloId", datos[x].moduloId));


                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getListaMedidaInsert(reader));
                            }
                        }

                    }
                }
            }
            
            return response;


        }

        public MedidasInsertDTOs getListaMedidaInsert(SqlDataReader reader)
        {
            return new MedidasInsertDTOs()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                version = Convert.ToInt32(reader["version"].ToString()),


            };
        }

        public async Task<List<MaestroOrdenes>> GetPostedMaestroOrdenes(int id, int userid, int estado)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_UpdateMasterOrden]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.Parameters.Add(new SqlParameter("@userid", userid));
                    cmd.Parameters.Add(new SqlParameter("@estado", estado));


                    var response = new List<MaestroOrdenes>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaMaestroOrdenes(reader));
                        }
                    }
                    return response;
                }
            }
        }

        public MaestroOrdenes getListaMaestroOrdenes(SqlDataReader reader)
        {
            return new MaestroOrdenes()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                prodmasterid = reader["prodmasterid"].ToString(),
                prodmasterrefid = reader["prodmasterrefid"].ToString(),
                itemid = reader["itemid"].ToString(),
                posted = Convert.ToInt32(reader["posted"].ToString()),
                userid = Convert.ToInt32(reader["usuarioID"].ToString()),
            };
        }

        public async Task<List<DatosMedidasDtos>> getDatosMedidas(string prodmasterid, string talla, int lavado, int TipoMedida)
        {
            

            var response = new List<DatosMedidasDtos>();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerPantsQualityData]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@orden", prodmasterid));
                    cmd.Parameters.Add(new SqlParameter("@talla", talla));
                    cmd.Parameters.Add(new SqlParameter("@lavado", lavado));
                    cmd.Parameters.Add(new SqlParameter("@TipoMedida", TipoMedida));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getOntenerPantsqualityData(reader));
                        }
                    }

                }

            }
            return response;

        }
        public DatosMedidasDtos getOntenerPantsqualityData(SqlDataReader reader)
        {
            return new DatosMedidasDtos()
            {
                id = Convert.ToInt32(reader["ID"].ToString()),
                Nombre = Convert.ToString(reader["Nombre"].ToString()),
                intruccion1 = Convert.ToString(reader["Intruccion_1"].ToString()),
                intruccion2 = Convert.ToString(reader["Intruccion_2"].ToString()),
                intruccion3 = Convert.ToString(reader["Intruccion_3"].ToString()),
                specs = Convert.ToString(reader["Spec"].ToString()),
                medida = Convert.ToString(reader["medida"].ToString()),
                MedidaNumerador = Convert.ToString(reader["MedidaNumerador"].ToString()),
                referencia = Convert.ToString(reader["Altura_Asiento"].ToString()),
                tolerancia1 = Convert.ToString(reader["Tolerancia_1"].ToString()),
                tolerancia2 = Convert.ToString(reader["Tolerancia_2"].ToString()),
                version = Convert.ToInt32(reader["Version"].ToString()),
                activo = true

            };
        }
        public MedidaDoubleDtos getListaMedidaDetalle(SqlDataReader reader)
        {
            return new MedidaDoubleDtos()
            {
                id = Convert.ToInt32(reader["id"].ToString()),

                medida = Convert.ToDouble(reader["medida"].ToString()),
                medidaNumerador = Convert.ToDouble(reader["medidaNumerador"].ToString()),
                version = Convert.ToInt32(reader["version"].ToString())
            };
        }

        public async Task<List<ComentarioDTO>> postComentario(List<ComentarioDTO> comentario)
        {
            var response = new List<ComentarioDTO>();

            for (int i = 0; i < comentario.Count; i++)
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[IM_InsertarComentario]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@masterID", comentario[i].MasterID));
                        cmd.Parameters.Add(new SqlParameter("@usuario", comentario[i].usuario));
                        cmd.Parameters.Add(new SqlParameter("@comentario", comentario[i].comentario));

                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getListaComentarioInsert(reader));
                            }
                        }

                    }

                }

            }
            return response;
        }
        public ComentarioDTO getListaComentarioInsert(SqlDataReader reader)
        {
            return new ComentarioDTO()
            {
                MasterID = Convert.ToInt32(reader["MasterID"].ToString()),
            };
        }

        public async Task<List<ComentariosDTO>> getComentarios(int masterId)
        {
            var response = new List<ComentariosDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerComentarioOrden]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@masterID", masterId));

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaComentarios(reader));
                        }
                    }

                }

            }
            return response;
        }

        public ComentariosDTO getListaComentarios(SqlDataReader reader)
        {
            return new ComentariosDTO()
            {
                MasterID = Convert.ToInt32(reader["MasterID"].ToString()),
                usuario = Convert.ToString(reader["nombre"].ToString()),
                comentario = Convert.ToString(reader["comentario"].ToString()),
                fecha = Convert.ToDateTime(reader["fecha"].ToString()),
            };
        }

        public async Task<List<ModulosCalidadDTO>> GetModulosCalidad()
        {
            var response = new List<ModulosCalidadDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerModulosCalidad]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaModulos(reader));
                        }
                    }

                }

            }
            return response;
        }
        public ModulosCalidadDTO getListaModulos(SqlDataReader reader)
        {
            return new ModulosCalidadDTO()
            {
                ID = Convert.ToInt32(reader["ID"].ToString()),
                Modulo = Convert.ToString(reader["Modulo"].ToString()),
                Activo = Convert.ToBoolean(reader["Activo"].ToString())
                
            };
        }

        public async Task<List<HistoricoEstdoOrdenDTO>> GetHistoricoEstdoOrden(int masterId)
        {
            var response = new List<HistoricoEstdoOrdenDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerEstadoOrdenes]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id", masterId));

                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaHistoricoEstado(reader));
                        }
                    }

                }

            }
            return response;
        }
        public HistoricoEstdoOrdenDTO getListaHistoricoEstado(SqlDataReader reader)
        {
            return new HistoricoEstdoOrdenDTO()
            {
               ID = Convert.ToInt32(reader["ID"].ToString()),
               usuario = Convert.ToString(reader["Nombre"].ToString()),
               Modulo = Convert.ToString(reader["Modulo"].ToString()),
                Estado = Convert.ToBoolean(reader["Estado"].ToString()),
                Fecha = Convert.ToDateTime(reader["Fecha"].ToString())

            };
        }

        public async  Task<string> getDatosExcel(string prodmasterid,string itemid)
        {

            var orden = "";
            if (prodmasterid.StartsWith("OP-")) 
            {
               orden = prodmasterid.Substring(0, 11);
            }
            else
            {
                orden = prodmasterid;
            }
            string ruta = @"\\10.100.0.41\Auditoria\" + orden + ".xlsx";

            var Tipo = await getTipoMedidas();
            string texto = "inicio: " + (DateTime.UtcNow.TimeOfDay).ToString();

            DatosExcelDTO excel = new DatosExcelDTO();
            excel.Master_ID = orden;
            if (File.Exists(ruta))
            {

                foreach (var tipo in Tipo)
                {
                    //la primera hoja lleva antes y desues de lavado por eso no se hace los mismo para las demas hojas
                    if (tipo.Hoja == 0)
                    {
                        int tolerancia = 1;
                        var medidas = await GetMedidas(tipo.ID);

                        try
                        {
                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using (ExcelPackage package = new ExcelPackage(ruta))
                            {
                                worksheet = package.Workbook.Worksheets[tipo.Hoja];
                                //Buscar donde se encuentran los campos de tolerancia
                                for (int i = 15; i < 100; i++)
                                {
                                    var cellvalue = (string)worksheet.Cells[13, i].Value;
                                    if (cellvalue == "Tolerancia")
                                    {
                                        tolerancia = i;
                                        break;
                                    }

                                }

                                for (int lavado = 0; lavado <= 1; lavado++)
                                {
                                    int fila = 14;
                                    foreach (var medida in medidas)
                                    {
                                        //buscar la fila donde se encuentra la medida 
                                        excel.Medida_ID = medida.id;

                                        for (int i = fila + 2; i <= 100; i++)
                                        {
                                            var cellvalue = (string)worksheet.Cells[i, 1].Value;
                                            if (medida.Nombre == cellvalue)
                                            {
                                                fila = i;
                                                break;
                                            }

                                        }
                                        int columna = 1;
                                        //en caso de ser desdepues del lavado buscar donde estan posicionadas esas medidas
                                        if (lavado == 1)
                                        {
                                            for (int i = 10; i <= 100; i++)
                                            {
                                                var cellvalue = "";
                                                try
                                                {
                                                    cellvalue = (string)worksheet.Cells[14, i].Value;
                                                }
                                                catch
                                                {
                                                    cellvalue = Convert.ToString((double)worksheet.Cells[14, i].Value);
                                                }
                                                if (cellvalue == "MEDIDAS")
                                                {
                                                    columna = i;
                                                    break;
                                                }
                                            }


                                        }

                                        //obtener los mensajes
                                        excel.Lavado = lavado;

                                        try
                                        {
                                            excel.Intruccion_1 = (string)worksheet.Cells[fila, columna + 1].Value;
                                        }
                                        catch (Exception ex)
                                        {
                                            excel.Intruccion_1 = Convert.ToString(Math.Round((double)worksheet.Cells[fila, columna + 1].Value, 4));
                                        }
                                        try
                                        {
                                            excel.Intruccion_2 = (string)worksheet.Cells[fila + 1, columna + 1].Value;
                                        }
                                        catch (Exception ex)
                                        {
                                            excel.Intruccion_2 = Convert.ToString(Math.Round((double)worksheet.Cells[fila + 1, columna + 1].Value, 4));
                                        }
                                        try
                                        {
                                            excel.Intruccion_3 = (string)worksheet.Cells[fila + 2, columna + 1].Value;
                                        }
                                        catch (Exception ex)
                                        {
                                            excel.Intruccion_3 = Convert.ToString(Math.Round((double)worksheet.Cells[fila + 2, columna + 1].Value, 4));
                                        }

                                        //obtener las tolerancias
                                        excel.Tolerancia_1 = (string)worksheet.Cells[fila, tolerancia].Value;
                                        excel.Tolerancia_2 = (string)worksheet.Cells[fila, tolerancia + 1].Value;

                                        List<ItemTallasDTOS> tallas = new List<ItemTallasDTOS>();
                                        if (prodmasterid.StartsWith("OP-"))
                                        {
                                            tallas = await GetItemTallas(itemid, orden);
                                        }
                                        else
                                        {
                                            var tallaEstilos = await getTallasEstilo(orden);
                                            tallaEstilos.ForEach(element =>
                                            {
                                                ItemTallasDTOS tmp = new ItemTallasDTOS();
                                                tmp.SIZEID = element.Tallas;
                                                tallas.Add(tmp);
                                            });
                                        }
                                        
                                        int columnaNueva = columna;
                                        foreach (var talla in tallas)
                                        {
                                            //talla del articulo
                                            excel.Talla = talla.SIZEID;

                                            //altura del asiento
                                            int col = GetColumna(talla.SIZEID, columnaNueva, 14);
                                            columnaNueva = col + 1;
                                            if (col != columna)
                                            {
                                                try
                                                {
                                                    excel.Altura_Asiento = (string)worksheet.Cells[15, col].Value;

                                                }
                                                catch (Exception ex)
                                                {
                                                    excel.Altura_Asiento = Convert.ToString(Math.Round((double)worksheet.Cells[15, col].Value, 4));

                                                }




                                                //obtener spec
                                                try
                                                {
                                                    excel.Spec = (string)worksheet.Cells[fila, col].Value;
                                                }
                                                catch (Exception ex)
                                                {
                                                    excel.Spec = Convert.ToString(Math.Round((double)worksheet.Cells[fila, col].Value, 4));
                                                }

                                            }
                                            else
                                            {
                                                excel.Altura_Asiento = "";
                                                excel.Spec = "";
                                            }


                                            //Enviar Datos medidas
                                            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                                            {
                                                using (SqlCommand cmd = new SqlCommand("[IM_InsertPantsQualityData]", sql))
                                                {
                                                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                                    cmd.Parameters.Add(new SqlParameter("@Master_ID", excel.Master_ID));
                                                    cmd.Parameters.Add(new SqlParameter("@Lavado", excel.Lavado));
                                                    cmd.Parameters.Add(new SqlParameter("@Medida_ID", excel.Medida_ID));
                                                    cmd.Parameters.Add(new SqlParameter("@Talla", excel.Talla));
                                                    cmd.Parameters.Add(new SqlParameter("@Spec", excel.Spec));
                                                    cmd.Parameters.Add(new SqlParameter("@Tolerancia_1", excel.Tolerancia_1));
                                                    cmd.Parameters.Add(new SqlParameter("@Tolerancia_2", excel.Tolerancia_2));
                                                    cmd.Parameters.Add(new SqlParameter("@Intruccion_1", excel.Intruccion_1 != null ? excel.Intruccion_1 : ""));
                                                    cmd.Parameters.Add(new SqlParameter("@Intruccion_2", excel.Intruccion_2 != null ? excel.Intruccion_2 : ""));
                                                    cmd.Parameters.Add(new SqlParameter("@Intruccion_3", excel.Intruccion_3 != null ? excel.Intruccion_3 : ""));
                                                    cmd.Parameters.Add(new SqlParameter("@Altura_Asiento", excel.Altura_Asiento));

                                                    await sql.OpenAsync();

                                                    using (var reader = await cmd.ExecuteReaderAsync())
                                                    {
                                                        while (await reader.ReadAsync())
                                                        {
                                                            //response.Add(getListaHistoricoEstado(reader));
                                                        }
                                                    }

                                                }

                                            }


                                        }
                                    }
                                }
                            }

                        }
                        catch (Exception error)
                        {

                            return "no: " + error.Message;
                        }
                    }
                    else
                    {
                        int tolerancia = 1;
                        var medidas = await GetMedidas(tipo.ID);

                        try
                        {
                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using (ExcelPackage package = new ExcelPackage(ruta))
                            {
                                worksheet = package.Workbook.Worksheets[tipo.Hoja];
                                //Buscar donde se encuentran el nombre del tipo de medida
                                int TipoMedida = 1;
                                for (int i = 8; i < 100; i++)
                                {
                                    var cellvalue = "";
                                    try
                                    {
                                        cellvalue = (string)worksheet.Cells[i, 3].Value;
                                    }
                                    catch
                                    {
                                        cellvalue = Convert.ToString((double)worksheet.Cells[i, 3].Value);
                                    }
                                    if (cellvalue == tipo.Nombre)
                                    {
                                        TipoMedida = i;
                                        break;
                                    }

                                }

                                //Buscar donde se encuentran los campos de tolerancia
                                for (int i = 10; i < 100; i++)
                                {
                                    var cellvalue = (string)worksheet.Cells[TipoMedida, i].Value;
                                    if (cellvalue == "Tolerancia")
                                    {
                                        tolerancia = i;
                                        break;
                                    }

                                }

                                int fila = 1;
                                foreach (var medida in medidas)
                                {
                                    //buscar la fila donde se encuentra la medida 
                                    excel.Medida_ID = medida.id;

                                    for (int i = TipoMedida + 2; i <= 100; i++)
                                    {
                                        var cellvalue = (string)worksheet.Cells[i, 1].Value;
                                        if (medida.Nombre == cellvalue)
                                        {
                                            fila = i;
                                            break;
                                        }

                                    }

                                    //obtener los mensajes
                                    excel.Lavado = 0;

                                    try
                                    {
                                        excel.Intruccion_1 = (string)worksheet.Cells[fila, 2].Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        excel.Intruccion_1 = Convert.ToString(Math.Round((double)worksheet.Cells[fila, 2].Value, 4));
                                    }

                                    try
                                    {
                                        excel.Intruccion_2 = (string)worksheet.Cells[fila + 1, 2].Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        excel.Intruccion_2 = Convert.ToString(Math.Round((double)worksheet.Cells[fila + 1, 2].Value, 4));
                                    }

                                    try
                                    {
                                        excel.Intruccion_3 = (string)worksheet.Cells[fila + 2, 2].Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        excel.Intruccion_3 = Convert.ToString(Math.Round((double)worksheet.Cells[fila + 2, 2].Value, 4));
                                    }
                                    //obtener las tolerancias
                                    excel.Tolerancia_1 = (string)worksheet.Cells[fila, tolerancia].Value;
                                    excel.Tolerancia_2 = (string)worksheet.Cells[fila, tolerancia + 1].Value;

                                    List<ItemTallasDTOS> tallas = new List<ItemTallasDTOS>();
                                    if (prodmasterid.StartsWith("OP-"))
                                    {
                                        tallas = await GetItemTallas(itemid, orden);
                                    }
                                    else
                                    {
                                        var tallaEstilos = await getTallasEstilo(orden);
                                        tallaEstilos.ForEach(element =>
                                        {
                                            ItemTallasDTOS tmp = new ItemTallasDTOS();
                                            tmp.SIZEID = element.Tallas;
                                            tallas.Add(tmp);
                                        });
                                    }
                                    int columnaNueva = 1;
                                    foreach (var talla in tallas)
                                    {
                                        //talla del articulo
                                        excel.Talla = talla.SIZEID;

                                        //altura del asiento
                                        int col = GetColumna(talla.SIZEID, columnaNueva, TipoMedida+1);
                                        columnaNueva = col + 1;
                                        if (col != 1)
                                        {

                                            excel.Altura_Asiento = "";

                                            //obtener spec
                                            try
                                            {
                                                excel.Spec = (string)worksheet.Cells[fila, col].Value;
                                            }
                                            catch (Exception ex)
                                            {
                                                excel.Spec = Convert.ToString(Math.Round((double)worksheet.Cells[fila, col].Value, 4));
                                            }

                                        }
                                        else
                                        {
                                            excel.Altura_Asiento = "";
                                            excel.Spec = "";
                                        }


                                        //Enviar Datos medidas
                                        using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                                        {
                                            using (SqlCommand cmd = new SqlCommand("[IM_InsertPantsQualityData]", sql))
                                            {
                                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                                cmd.Parameters.Add(new SqlParameter("@Master_ID", excel.Master_ID));
                                                cmd.Parameters.Add(new SqlParameter("@Lavado", excel.Lavado));
                                                cmd.Parameters.Add(new SqlParameter("@Medida_ID", excel.Medida_ID));
                                                cmd.Parameters.Add(new SqlParameter("@Talla", excel.Talla));
                                                cmd.Parameters.Add(new SqlParameter("@Spec", excel.Spec));
                                                cmd.Parameters.Add(new SqlParameter("@Tolerancia_1", excel.Tolerancia_1));
                                                cmd.Parameters.Add(new SqlParameter("@Tolerancia_2", excel.Tolerancia_2));
                                                cmd.Parameters.Add(new SqlParameter("@Intruccion_1", excel.Intruccion_1 != null ? excel.Intruccion_1 : ""));
                                                cmd.Parameters.Add(new SqlParameter("@Intruccion_2", excel.Intruccion_2 != null ? excel.Intruccion_2 : ""));
                                                cmd.Parameters.Add(new SqlParameter("@Intruccion_3", excel.Intruccion_3 != null ? excel.Intruccion_3 : ""));
                                                cmd.Parameters.Add(new SqlParameter("@Altura_Asiento", excel.Altura_Asiento));

                                                await sql.OpenAsync();

                                                using (var reader = await cmd.ExecuteReaderAsync())
                                                {
                                                    while (await reader.ReadAsync())
                                                    {
                                                        //response.Add(getListaHistoricoEstado(reader));
                                                    }
                                                }

                                            }

                                        }


                                    }

                                }


                            }
                        }
                        catch (Exception error)
                        {
                            return "no: " + error.Message;
                        }
                    }
                }
            };
            
             texto += " Final: " + (DateTime.UtcNow.TimeOfDay).ToString();

            return texto;
        }

        public async Task<List<TiposMedidaDTO>> getTipoMedidas()
        {
            var response = new List<TiposMedidaDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerTipoMedida]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getTiposMedidas(reader));
                        }
                    }
                }
            }
            return response;
        }

        public TiposMedidaDTO getTiposMedidas(SqlDataReader reader)
        {
            return new TiposMedidaDTO()
            {
                ID = Convert.ToInt32(reader["ID"].ToString()),
                Nombre = Convert.ToString(reader["Nombre"].ToString()),
                Hoja = Convert.ToInt32(reader["Hoja"].ToString()),


            };
        }

        public async Task<List<EstiloDTO>> getEstilos(string filtro)
        {
            var response = new List<EstiloDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringPLM))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_GETSTYLE_MEASUREAPP]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ESTILO", filtro));
                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getEstilo(reader));
                        }
                    }
                }
            }
            return response;
        }
        public EstiloDTO getEstilo(SqlDataReader reader)
        {
            return new EstiloDTO()
            {

                Estilo = Convert.ToString(reader["Estilo"].ToString())

            };
        }

        public async Task<List<TallasEstiloDTO>> getTallasEstilo(string estilo)
        {
            var response = new List<TallasEstiloDTO>();
            using (SqlConnection sql = new SqlConnection(_connectionStringPLM))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_GETSIZE_MEASUREAPP]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ESTILO", estilo));
                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getTallaEstilo(reader));
                        }
                    }
                }
            }
            return response;
        }

        public TallasEstiloDTO getTallaEstilo(SqlDataReader reader)
        {
            return new TallasEstiloDTO()
            {

                Estilo = Convert.ToString(reader["Estilo"].ToString()),
                Set = Convert.ToString(reader["Set"].ToString()),
                Tallas = Convert.ToString(reader["Tallas"].ToString()),

            };
        }
    }
}
