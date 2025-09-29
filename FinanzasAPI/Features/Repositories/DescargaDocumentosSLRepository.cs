using Core.DTOs;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class DescargaDocumentosSLRepository: IDescargaDocumentosSL
    {
        private readonly string _connectionString;
        public DescargaDocumentosSLRepository(AxContext context, IConfiguration configuracion)
        {
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
        }

        public async Task<string> DescargarDocumentos()
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_SL_DocumentosNoDescargados]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var response = new List<DescargaDocuemntosDTO>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(GetDatos(reader));
                        }
                    }
                    foreach(var invoice in response)
                    {
                       string resp = DescargarArchivo(invoice.PDF, invoice.INVOICEID, invoice.DESCRIPTION, invoice.INVOICEDATE);
                        if(resp == "OK")
                        {
                            ActualizarDocumento(invoice.RECID);
                        }
                    }
                }
            }

            return "OK";
        }

        public DescargaDocuemntosDTO GetDatos(SqlDataReader reader)
        {
            return new DescargaDocuemntosDTO()
            {
                
                INVOICEID = reader["INVOICEID"].ToString(),
                PDF = reader["PDF"].ToString(),
                DESCRIPTION =reader["DESCRIPTION"].ToString(),
                INVOICEDATE = Convert.ToDateTime(reader["INVOICEDATE"].ToString()),
                RECID = Convert.ToInt64(reader["RECID"].ToString()),

            };
        }

        public string DescargarArchivo( string urlPDF, string InvoiceID, string InvoiceType, DateTime InvoiceDate)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");                
                string month = new DateTime(2025, InvoiceDate.Month, 1).ToString("MMMM");
                TextInfo textInfo = new CultureInfo("es-ES", false).TextInfo;
                string savePath = $@"\\10.100.0.41\boveda de documentos\Facturacion\Modinter S.A de C.V\{InvoiceDate.Year}\{InvoiceDate.Month}. {textInfo.ToTitleCase(month)}\{InvoiceType}";
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                savePath = $@"{savePath}\{InvoiceID}.pdf";
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(urlPDF, savePath);
                }

                return "OK";

            }
            catch (Exception ex)
            {
                return $"Error al descargar el Archivo: {ex.ToString()}";
            }



        }

        public async void ActualizarDocumento(Int64 recid)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_SL_ActualizarDocumentoDescargado]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@recid", recid));
                    var response = new List<DescargaDocuemntosDTO>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            
                        }
                    }
                }
            }
        }
    }
}
