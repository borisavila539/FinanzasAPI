using Core.DTOs;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class PantsQualityRepository : IPantsQualityRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringCubo;


        private readonly AxContext _context;
        private List<OrdenesInicialdasDTO> ordenesInicialdas = new List<OrdenesInicialdasDTO>();

        private Application application;
        private Workbook workbook;
        private Worksheet worksheet;

        public PantsQualityRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
            _connectionStringCubo = configuracion.GetConnectionString("IMDesarrollos");

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

        public async Task<List<MedidasInsertDTOs>> getModificarExcel(List<MedidasInsertDTOs> datos)
        {
            var response = new List<MaestroOrdenes>();

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMasterOrdenId]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id", datos[0].idMasterOrden));


                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(getListaMasterOrden(reader));
                        }
                    }
                    sql.Close();
                }
            }
            using (SqlConnection sql2 = new SqlConnection(_connectionStringCubo))
            { 

                var Tallas = await GetItemTallas(response[0].itemid);
                application = new Application();

                string ruta = @"\\10.100.0.41\Auditoria\" + response[0].prodmasterid.Replace(" ", "-") + ".xlsx";
                if (File.Exists(ruta))
                {

                    try
                    {


                        application.Workbooks.Open(@"\\10.100.0.41\Auditoria\" + response[0].prodmasterid.Replace(" ", "-") + ".xlsx");
                        workbook = application.Workbooks.Item[1];
                        worksheet = workbook.ActiveSheet;
                        ((Worksheet)worksheet.Application.ActiveWorkbook.Sheets[1]).Select();



                        datos.ForEach(async (x) =>
                        {
                            var response2 = new List<MedidasDTOs>();

                            using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMedidasCalidadID]", sql2))
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@id", x.idMedida));

                                sql2.Open();


                                using (var reader = cmd.ExecuteReader())
                                {

                                    while (reader.Read())
                                    {
                                        response2.Add(getListaMedidas(reader));
                                    }
                                }
                                sql2.Close();

                            }

                            int fila = 1;
                            for (int i = 14; i <= 100; i++)
                            {
                                var cellvalue = (string)(worksheet.Cells[i, 1] as Microsoft.Office.Interop.Excel.Range).Value;
                                if (response2[0].Nombre == cellvalue)
                                {
                                    fila = i + 1;
                                    i = 500;
                                }

                            }

                            int columna = 1;

                            if (x.lavadoID == 0)
                            {
                                for (int i = 1; i <= 100; i++)
                                {
                                    var cellvalue = (string)(worksheet.Cells[14, 1] as Microsoft.Office.Interop.Excel.Range).Value;
                                    if (cellvalue == "MEDIDAS")
                                    {
                                        columna = i;
                                        i = 500;
                                    }
                                }
                            }
                            else
                            {
                                int cont = 0;
                                for (int i = 1; i <= 100; i++)
                                {
                                    var cellvalue = "";
                                    try
                                    {
                                        cellvalue = (string)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value;
                                    }
                                    catch (Exception)
                                    {
                                        cellvalue = Convert.ToString((double)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value);
                                    }
                                    if (cellvalue == "MEDIDAS")
                                    {
                                        if (cont > 0)
                                        {
                                            columna = i;
                                            i = 500;
                                        }
                                        cont++;
                                    }
                                }
                            }

                            worksheet.Cells[fila, GetColumna(x.idTalla, columna, fila)] = (Convert.ToDouble(x.Medida) + Convert.ToDouble(x.MedidaNumerador.Length > 0 ? x.MedidaNumerador : 0) / 16).ToString();
                        });
                        application.ActiveWorkbook.Save();

                        workbook.Close();


                        application.Quit();


                        Marshal.ReleaseComObject(workbook);
                        Marshal.ReleaseComObject(worksheet);
                        Marshal.ReleaseComObject(application);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        var resp = new List<MedidasInsertDTOs>();
                        resp = datos;
                        return resp;
                    }
                    catch (Exception err)
                    {
                        //Se quito el comentario del catch
                        application.ActiveWorkbook.Save();
                        workbook.Close();

                        application.Quit();


                        Marshal.ReleaseComObject(workbook);
                        Marshal.ReleaseComObject(worksheet);
                        Marshal.ReleaseComObject(application);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        return null;
                    }
                    finally
                    {
                        sql2.Close();
                    }
                }
                else
                {
                    return null;
                }
                
            }

        }

        public int GetColumna(string talla, int columna, int fila)
        {
            for (int i = columna; i <= 100; i++)
            {
                var cellvalue = "";
                try
                {
                    cellvalue = (string)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value;
                }
                catch (Exception)
                {
                    cellvalue = Convert.ToString((double)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value);
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

        public async Task<List<ItemTallasDTOS>> GetItemTallas(string itemid)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerTallasOrdenes]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@itemid", itemid));


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
                rolid = Convert.ToInt32(reader["rolid"].ToString())

            };
        }

        public async Task<List<MedidasDTOs>> GetMedidas()
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMedidasCalidad]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;


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
            var resp = await getModificarExcel(datos);
            if(resp == null)
            {
                return null;
            }
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
                id = Convert.ToInt32(reader["ID"].ToString())
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

        public async Task<List<DatosMedidasDtos>> getDatosMedidas(string prodmasterid, string talla, int lavado)
        {
            var medidas = await GetMedidas();
            var lista = new List<DatosMedidasDtos>();
            string ruta = @"\\10.100.0.41\Auditoria\" + prodmasterid.Replace(" ", "-") + ".xlsx";
            if(File.Exists(ruta))
            {

            try
            {
                application = new Application();

                application.Workbooks.Open(@"\\10.100.0.41\Auditoria\" + prodmasterid.Replace(" ", "-") + ".xlsx");
                workbook = application.Workbooks.Item[1];
                worksheet = workbook.ActiveSheet;
                ((Worksheet)worksheet.Application.ActiveWorkbook.Sheets[1]).Select();

                int tolerancia = 1;
                for (int i = 1; i <= 100; i++)
                {
                    var cellvalue = (string)(worksheet.Cells[13, i] as Microsoft.Office.Interop.Excel.Range).Value;
                    if (cellvalue == "Tolerancia")
                    {
                        tolerancia = i;
                        i = 500;
                    }
                }

                foreach (var medida in medidas)
                {
                    int fila = 1;
                    for (int i = 14; i <= 100; i++)
                    {
                        var cellvalue = (string)(worksheet.Cells[i, 1] as Microsoft.Office.Interop.Excel.Range).Value;
                        if (medida.Nombre == cellvalue)
                        {
                            fila = i;
                            i = 500;
                        }

                    }
                    int columna = 1;

                    if (lavado == 0)
                    {
                        for (int i = 1; i <= 100; i++)
                        {
                            var cellvalue = (string)(worksheet.Cells[14, 1] as Microsoft.Office.Interop.Excel.Range).Value;
                            if (cellvalue == "MEDIDAS")
                            {
                                columna = i;
                                i = 500;
                            }
                        }
                    }
                    else
                    {
                        int cont = 0;
                        for (int i = 1; i <= 100; i++)
                        {
                            var cellvalue = "";
                            try
                            {
                                cellvalue = (string)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value;
                            }
                            catch (Exception)
                            {
                                cellvalue = Convert.ToString((double)(worksheet.Cells[14, i] as Microsoft.Office.Interop.Excel.Range).Value);
                            }
                            if (cellvalue == "MEDIDAS")
                            {
                                if (cont > 0)
                                {
                                    columna = i;
                                    i = 500;
                                }
                                cont++;
                            }
                        }
                    }

                    DatosMedidasDtos datos = new DatosMedidasDtos();
                    datos.id = medida.id;
                    datos.Nombre = medida.Nombre;
                    datos.Link = medida.Link;
                    datos.activo = medida.activo;

                    var response = new List<MedidaDoubleDtos>();

                    using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                    {
                        using (SqlCommand cmd = new SqlCommand("[IM_ObtenerMedidaDetalle]", sql))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@MasterId", prodmasterid));
                            cmd.Parameters.Add(new SqlParameter("@medida", medida.id));
                            cmd.Parameters.Add(new SqlParameter("@talla", talla));
                            cmd.Parameters.Add(new SqlParameter("@lavado", lavado));



                            await sql.OpenAsync();

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    response.Add(getListaMedidaDetalle(reader));
                                }
                            }
                        }
                    }

                    try
                    {
                        datos.tolerancia1 = Convert.ToString(Math.Round((double)(worksheet.Cells[fila, tolerancia] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }
                    catch (Exception)
                    {
                        datos.tolerancia1 = (string)(worksheet.Cells[fila, tolerancia] as Microsoft.Office.Interop.Excel.Range).Value;

                    }

                    try
                    {
                        datos.tolerancia2 = Convert.ToString(Math.Round((double)(worksheet.Cells[fila, tolerancia+1] as Microsoft.Office.Interop.Excel.Range).Value, 4));

                    }
                    catch (Exception)
                    {
                        datos.tolerancia2 = (string)(worksheet.Cells[fila, tolerancia+1] as Microsoft.Office.Interop.Excel.Range).Value;

                    }

                    try
                    {
                        datos.medida = response[0].medida.ToString().Length > 0 ? response[0].medida.ToString() : "";

                    }catch(Exception ex)
                    {
                        datos.medida = "";
                    }

                    try
                    {
                        datos.MedidaNumerador = response[0].medidaNumerador.ToString().Length > 0 ? response[0].medidaNumerador.ToString() : "";

                    }
                    catch (Exception ex)
                    {
                        datos.MedidaNumerador = "";
                    }

                    try
                    {
                        datos.intruccion1 = (string)(worksheet.Cells[fila, 2] as Microsoft.Office.Interop.Excel.Range).Value;

                    }
                    catch (Exception ex)
                    {
                        datos.intruccion1 = Convert.ToString(Math.Round((double)(worksheet.Cells[fila, 2] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }

                    try
                    {
                        datos.intruccion2 = (string)(worksheet.Cells[fila + 1, 2] as Microsoft.Office.Interop.Excel.Range).Value;

                    }
                    catch (Exception ex)
                    {
                        datos.intruccion2 = Convert.ToString(Math.Round((double)(worksheet.Cells[fila + 1, 2] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }

                    try
                    {
                        datos.intruccion3 = (string)(worksheet.Cells[fila + 2, 2] as Microsoft.Office.Interop.Excel.Range).Value;

                    }
                    catch (Exception ex)
                    {
                        datos.intruccion3 = Convert.ToString(Math.Round((double)(worksheet.Cells[fila + 2, 2] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }

                    try
                    {
                        datos.specs = (string)(worksheet.Cells[fila, GetColumna(talla, columna, fila)] as Microsoft.Office.Interop.Excel.Range).Value;

                    }
                    catch (Exception ex)
                    {
                        datos.specs = Convert.ToString(Math.Round((double)(worksheet.Cells[fila, GetColumna(talla, columna, fila)] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }

                    try
                    {
                        datos.referencia = (string)(worksheet.Cells[15, GetColumna(talla, columna, fila)] as Microsoft.Office.Interop.Excel.Range).Value;

                    }
                    catch (Exception ex)
                    {
                        datos.referencia = Convert.ToString(Math.Round((double)(worksheet.Cells[15, GetColumna(talla, columna, fila)] as Microsoft.Office.Interop.Excel.Range).Value,4));

                    }

                    lista.Add(datos);
                }
               

                workbook.Close();
        

                application.Quit();
             

                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(application);

                GC.Collect();
                GC.WaitForPendingFinalizers();


                return lista;
            }
            catch (Exception ex)
            {

                 //workbook.Close();


                 application.Quit();


                 Marshal.ReleaseComObject(workbook);
                 Marshal.ReleaseComObject(worksheet);
                 Marshal.ReleaseComObject(application);

                 GC.Collect();
                 GC.WaitForPendingFinalizers();
                var l = new List<DatosMedidasDtos>();
                DatosMedidasDtos m = new DatosMedidasDtos();
                return null;
            }
            }
            else
            {
                return null;
            }
            
        }
        public MedidaDoubleDtos getListaMedidaDetalle(SqlDataReader reader)
        {
            return new MedidaDoubleDtos()
            {
                id = Convert.ToInt32(reader["id"].ToString()),

                medida = Convert.ToDouble(reader["medida"].ToString()),
                medidaNumerador = Convert.ToDouble(reader["medidaNumerador"].ToString()),
            };
        }
    }
}
