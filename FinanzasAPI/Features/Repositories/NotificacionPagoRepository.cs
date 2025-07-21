using Core.DTOs;
using Core.Interfaces;
using FinanzasAPI.Reporting;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using ReportingServices.DTOs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using WMS_API.Features.Utilities;

namespace FinanzasAPI.Features.Repositories
{
    public class NotificacionPagoRepository : INotificacionPagoProveedorRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringCubo;
        /*Commented by spineda on july/11/2025 - Begin*/
        private readonly IFacturasProveedorRepository _facturasProveedorRepository;
        /*Commented by spineda on july/11/2025 - End*/

        private readonly AxContext _context;
        private List<ComprobanteRetencionDTO> comprobanteRetencions = new List<ComprobanteRetencionDTO>();
        public NotificacionPagoRepository(AxContext context, IConfiguration configuracion, IFacturasProveedorRepository facturasProveedorRepository)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
            _connectionStringCubo = configuracion.GetConnectionString("IMFinanzas");
            /*Commented by spineda on july/11/2025 - Begin*/
            _facturasProveedorRepository = facturasProveedorRepository;
            /*Commented by spineda on july/11/2025 - End*/

        }
        public async Task<List<NotificacionPagoDTO>> getNotificacionPago(string numerolote, int enviar, string empresa, string correoCopia)
        {
            if (correoCopia == "-")
                correoCopia = "";
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("IMPagoProveedores", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@NumeroLoteDiario", numerolote));
                    cmd.Parameters.Add(new SqlParameter("@dataareaid", empresa));
                    var response = new List<NotificacionPagoDTO>();
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            response.Add(getListaPagos(reader)) ;
                        }
                    }
                    if(enviar == 1)
                    {
                        if(empresa == "IMHN")
                        {
                            await getListaRetencion(numerolote, empresa);
                        }
                        await enviarCorreo(response,numerolote,empresa,correoCopia);
                    }
  
                    return response;
                }
            } 
        }

        public NotificacionPagoDTO getListaPagos(SqlDataReader reader)
        {
            return new NotificacionPagoDTO()
            {
                fecha = Convert.ToDateTime(reader["fecha"].ToString()),
                correo = reader["correo"].ToString(),
                proveedorNum = reader["proveedorNum"].ToString(),
                NombreProveedor = reader["NombreProveedor"].ToString(),
                Nombrebanco = reader["banco"].ToString(),
                Cuenta = reader["cuenta"].ToString(),
                NumeroFactura = reader["NumeroFactura"].ToString(),
                divisa = reader["divisa"].ToString(),
                monto = (double)reader["monto"],
                empresa = reader["empresa"].ToString(),
                paymmode = reader["paymmode"].ToString()
            };
        }

        public ComprobanteRetencionDTO getRetencion(SqlDataReader reader)
        {
            return new ComprobanteRetencionDTO()
            {
                proveedorNum = reader["proveedorNum"].ToString(),
                header02 = (decimal)reader["header02"],
                header03 = reader["header03"].ToString(),
                telefono = reader["Telefono"].ToString(),
                correoElectronico = reader["CorreoElectronico"].ToString(),
                WebSite = reader["WebSite"].ToString(),
                comprobante = reader["comprobante"].ToString(),
                empresaCAI = reader["empresaCAI"].ToString(),
                rango = reader["Rango"].ToString(),
                fechaEmision = (DateTime)reader["FechaEmision"],
                fechaLimiteEmision = (DateTime)reader["FechaLimiteEmision"],
                middle01NombreEmpresa = reader["middle01Name"].ToString(),
                middle01RTNEmpresa = reader["middle01RTN"].ToString(),
                middle02 = reader["middle02"].ToString(),
                middle03 = (decimal)reader["middle03"],
                proveedor = reader["Proveedor"].ToString(),
                proveedorRTN = reader["proveedorRTN"].ToString(),
                fechaFactura = (DateTime)reader["FechaFactura"],
                numeroFactura = reader["numeroFactura"].ToString(),
                proveedorCAI = reader["proveedorCAI"].ToString(),
                importe = (decimal)reader["Importe"],
                retencionISR = (decimal)reader["RetencionISR"],
                footer01 = reader["footer01"].ToString(),
                footer02 = reader["footer02"].ToString(),
                footer03 = reader["footer03"].ToString(),
                /*Commented by spineda on july/11/2025 - Begin*/
                exchRate = (decimal)reader["ExchRate"],
                currencyTxt = reader["CurrencyTxt"].ToString(),
                currencyCode = reader["CurrencyCode"].ToString()
                /*Commented by spineda on july/11/2025 - End*/
            };
        }

        public async Task getListaRetencion(string numeroLote, string empresa)
        {
            using (SqlConnection sql = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("IMComprobanteRetencion", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@numeroLote", numeroLote));
                    cmd.Parameters.Add(new SqlParameter("@empresa",empresa));
                    
                    await sql.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            comprobanteRetencions.Add(getRetencion(reader));
                        }
                    }
                }
            }
        }

        public string mes(int num)
        {
            string nombre = "";
            switch (num)
            {
                case 1: nombre = "ENERO"; break;
                case 2: nombre = "FEBRERO"; break;
                case 3: nombre = "MARZO"; break;
                case 4: nombre = "ABRIL"; break;
                case 5: nombre = "MAYO"; break;
                case 6: nombre = "JUNIO"; break;
                case 7: nombre = "JULIO"; break;
                case 8: nombre = "AGOSTO"; break;
                case 9: nombre = "SEPTIEMBRE"; break;
                case 10: nombre = "OCTUBRE"; break;
                case 11: nombre = "NOVIEMBRE"; break;
                case 12: nombre = "DICIEMBRE"; break;
            }
            return nombre;
        }

        public async Task<string> enviarCorreo(List<NotificacionPagoDTO> datos, string numerolote, string empresa, string correoCopia)
        {
            List<ComprobanteRetencionPDF_DTO> comprobantes = new List<ComprobanteRetencionPDF_DTO>();
            ComprobanteRetencionPDF_DTO comprobante = new ComprobanteRetencionPDF_DTO();

            byte[] logo = null;
            byte[] firmaSello = null;
            if (comprobanteRetencions.Count()>0)
            {
                logo = _context.Companyimage.First(x => x.Dataareaid == empresa).Image;
                firmaSello = _context.Taxwithholdsignatures.First().Taxwithholdsignature;

                
                foreach (var item in comprobanteRetencions)
                {
                    comprobante.proveedorNum = item.proveedorNum;
                    comprobante.header02 = item.header02.ToString();
                    comprobante.header03 = item.header03;
                    comprobante.telefono = item.telefono;
                    comprobante.correoElectronico = item.correoElectronico;
                    comprobante.WebSite = item.WebSite;
                    comprobante.comprobante = item.comprobante;
                    comprobante.empresaCAI = item.empresaCAI;
                    comprobante.rango = item.rango;
                    comprobante.fechaEmision = item.fechaEmision.ToString("dd/MM/yyyy");
                    comprobante.fechaLimiteEmision = item.fechaLimiteEmision.ToString("dd/MM/yyyy");
                    comprobante.middle01NombreEmpresa = item.middle01NombreEmpresa;
                    comprobante.middle01RTNEmpresa = item.middle01RTNEmpresa;
                    comprobante.middle02 = item.middle02;
                    comprobante.middle03 = item.middle03.ToString();
                    comprobante.proveedor = item.proveedor;
                    comprobante.proveedorRTN = item.proveedorRTN;
                    comprobante.fechaFactura = item.fechaFactura.ToString("dd/MM/yyyy");
                    comprobante.numeroFactura = item.numeroFactura;
                    comprobante.proveedorCAI = item.proveedorCAI;
                    comprobante.importe = item.importe.ToString("0,0.00", CultureInfo.InvariantCulture);
                    comprobante.retencionISR = item.retencionISR.ToString("0,0.00", CultureInfo.InvariantCulture);
                    comprobante.footer01 = item.footer01;
                    comprobante.footer02 = item.footer02;
                    comprobante.footer03 = item.footer03;
                    /*Commented by spineda on july/11/2025 - Begin*/
                    comprobante.exchRate = item.exchRate;
                    comprobante.currencyTxt = item.currencyTxt;
                    comprobante.currencyCode = item.currencyCode;
                    /*Commented by spineda on july/11/2025 - End*/

                    comprobantes.Add(comprobante);
                    comprobante = new ComprobanteRetencionPDF_DTO();                    
                }
            }
            List<string> proveedores = new List<string>();
            proveedores.Add(datos[0].proveedorNum);
            datos.ForEach(tmp => {
                var existe = false;
                proveedores.ForEach(tmp2 =>
                {
                    if(tmp2 == tmp.proveedorNum)
                    {
                        existe = true;
                    }
                });
                if (!existe)
                {
                    proveedores.Add(tmp.proveedorNum);
                }
            });

            foreach (var tmp in proveedores)
            {
                //validar si el correo ya ha sido enviado

                //campos que llevara la tabla
                List<NotificacionPagoDTO> data = new List<NotificacionPagoDTO>();
                List<ComprobanteRetencionPDF_DTO> pdf = new List<ComprobanteRetencionPDF_DTO>();
                decimal sumaImporte = 0;
                decimal sumaRetension = 0;

                /*Commented by spineda on july/14/2025 - Begin*/
                List<Archivos> files = new();
                /*Commented by spineda on july/14/2025 - End*/

                datos.ForEach(tmp2 =>
                {
                    if (tmp2.proveedorNum == tmp)
                    {
                        data.Add(tmp2);
                    }
                });
                
                comprobantes.ForEach(tmp2 =>
                { 
                   if (tmp2.proveedorNum == tmp)
                   {
                       pdf.Add(tmp2);
                       sumaImporte += Convert.ToDecimal(tmp2.importe);
                       sumaRetension += Convert.ToDecimal(tmp2.retencionISR);
                   }
                });

                if (pdf.Count() > 0)
                {
                    ComprobanteRetencionPDF_DTO tmp4 = new ComprobanteRetencionPDF_DTO();
                    tmp4.proveedorCAI = "Totales";
                    tmp4.importe = sumaImporte.ToString("0,0.00", CultureInfo.InvariantCulture);
                    tmp4.retencionISR = sumaRetension.ToString("0,0.00", CultureInfo.InvariantCulture);
                    pdf.Add(tmp4);
                    tmp4 = new ComprobanteRetencionPDF_DTO();
                }
                Task<string> enviado = CorreoEnviado(empresa, data[0].proveedorNum, numerolote);
                string FueEnviado = enviado.Result.ToString();
                if (data[0].correo != null && data[0].correo != "" && FueEnviado == "NO")
                {
                    string correoDestino = data[0].correo;
                    string correoOrigen = VariablesGlobales.Correo;
                    string contrasena = VariablesGlobales.Correo_Password;
                    string asunto = "Notificacion de pago";
                    if (empresa == "IMGT")
                    {
                        correoDestino += (correoCopia.Length > 0 ? "," + correoCopia : "");
                    }

                    //Cuerpo del correo
                    string html = $"<h3>Estimados señores de: {data[0].NombreProveedor}</h3>" +
                                  $"<p>De parte de {data[0].empresa},";
                    if (data[0].paymmode == "CHEQUE")
                    {
                        html += $"Hemos emitido un cheque por el pago de la(s) factura(s) detallada(s), favor organizar la recolecta para la próxima semana, en horario y sitio habitual.</p>";
                        if (empresa == "IMHN")
                        {
                            html += $"<p>Dia de entrega: miercoles.</p>";

                        }
                        else
                        {
                            html += $"<p>Dia de entrega: viernes.</p>";

                        }
                    }
                    else
                    {
                        html += "se le notifica que se realizo el pago en las siguientes facturas:</p>";
                    }

                    html +=
                                  "<table border = '1'>" +
                                        "<thead>" +
                                            "<tr>" +
                                                "<th>Fecha de pago</th>" +
                                                "<th>Banco</th>" +
                                                "<th>Numero de cuenta</th>" +
                                                "<th>Numero de Factura</th>" +
                                                "<th>Divisa</th>" +
                                                "<th>Monto</th>" +
                                            "</tr>" +
                                        "</thead>" +
                                    "<body>"
                    ;

                    var total = 0.00;

                    data.ForEach(x =>
                    {
                        string banco = "";
                        string cuenta = "";
                        if (x.paymmode != "CHEQUE")
                        {
                            banco = x.Nombrebanco;
                            cuenta = x.Cuenta;
                        }

                        html += "<tr>" +
                                    $"<td style='padding: 0px 5px;'> {x.fecha.ToString("MM/dd/yyyy")}</td>" +
                                    $"<td style='padding: 0px 5px;'> {banco}</td>" +
                                    $"<td style='padding: 0px 5px;'>{cuenta}</td>" +
                                    $"<td style='padding: 0px 5px;'>{x.NumeroFactura}</td>" +
                                    $"<td style='text-align:center;padding: 0px 5px;'>{x.divisa}</td>" +
                                    $"<td style='text-align:right;padding: 0px 5px;'>{x.monto.ToString("0,0.00", CultureInfo.InvariantCulture)}</td>" +
                                "</tr>";
                        total += x.monto;
                    });

                    html += "<tr>" +
                                "<td></td>" +
                                "<td></td>" +
                                "<td></td>" +
                                "<td></td>" +
                                "<td style='font-weight: bold;'>Total</td>" +
                                $"<td style='text-align: right;font-weight: bold;'>{total.ToString("0,0.00", CultureInfo.InvariantCulture)} </td>" +
                            "</tr>" +
                        "</body>" +
                    "</table>";

                    if (empresa == "IMGT")
                    {
                        html += "<p>Se le solicita confirme de recibido y el envio del recibo de caja a nuestras oficinas en hora de de atencion a proveedores, martes de 8:00 am a 12:00pm.</p>";
                    }

                    /*Commented by spineda on july/14/2025 - Begin
                     MailMessage mailMessage = new MailMessage(correoOrigen, correoDestino, asunto, html);
                     mailMessage.IsBodyHtml = true;
                     Commented by spineda on july/14/2025 - End*/

                    //Enviar PDF de Comporbante de Retencion
                    if (pdf.Count() > 0)
                    {
                        Reporting_Services generar = new Reporting_Services();

                        var grupos = pdf.GroupBy(x => x.header02);

                        var listacampos = grupos.Select(x => x.Key);
                        foreach (var campo in listacampos)
                        {
                            var comp = new List<ComprobanteRetencionPDF_DTO>();

                            pdf.ForEach(x =>
                            {
                                if (x.header02 == campo && campo != null)
                                {
                                    comp.Add(x);
                                }
                            });

                            ComprobanteRetencionPDF_DTO tmp4 = new ComprobanteRetencionPDF_DTO();
                            tmp4.proveedorCAI = "Totales";
                            tmp4.importe = comp.Sum(x => Convert.ToDecimal(x.importe)).ToString("0,0.00", CultureInfo.InvariantCulture);
                            tmp4.retencionISR = comp.Sum(x => Convert.ToDecimal(x.retencionISR)).ToString("0,0.00", CultureInfo.InvariantCulture);
                            comp.Add(tmp4);

                            if (comp.Count() > 0 && comp[0].proveedorNum != null)
                            {
                                /*Commented by spineda on july/14/2025 - Begin*/
                                Archivos file = new()
                                {
                                    Nombre = $"Comprobante_Retencion {comp.First().header02}.pdf",
                                    Archivo = generar.GenerateReport_ComprobanteRetencionProveedor(comp)
                                };

                                files.Add(file);
                                //MemoryStream ms = new MemoryStream(generar.GenerateReport_ComprobanteRetencionProveedor(comp));
                                //mailMessage.Attachments.Add(new Attachment(ms, "Comprobante_Retencion " + comp.First().header02 + " .pdf"));
                                /*Commented by spineda on july/14/2025 - End*/

                            }

                        }

                        /*byte[] pdfData = ms.ToArray();
                        DateTime date = DateTime.Now;
                        string path = @"\\AppServer\Intermoda\RETENCION AX\RETENCIONES " + date.Year + @"\" + mes(date.Month);
                        try
                        {

                            File.WriteAllBytes(path + @"\" + empresa + "-" + pdf[0].comprobante.Substring(11, 8) + "-" + pdf[0].proveedorNum + ".pdf", pdfData);
                        }
                        catch (Exception ex)
                        {
                            Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + @"\" + empresa + "-" + pdf[0].comprobante.Substring(11, 8) + "-" + pdf[0].proveedorNum + ".pdf", pdfData);
                        }*/

                    }

                    /*Commented by spineda on july/14/2025 - Begin*/
                    await _facturasProveedorRepository.enviarCorreo(correoDestino, asunto, html, files, null);

                    //SmtpClient smtpClient = new SmtpClient();

                    //smtpClient.Host = "smtp.office365.com";
                    //smtpClient.Port = 587;
                    //smtpClient.EnableSsl = true;
                    //smtpClient.UseDefaultCredentials = false;

                    //NetworkCredential networkCredential = new NetworkCredential(correoOrigen, contrasena);

                    //smtpClient.Credentials = networkCredential;

                    //smtpClient.Send(mailMessage);
                    //smtpClient.Dispose();
                    /*Commented by spineda on july/14/2025 - End*/

                    data.ForEach(x =>
                    {
                        // sendHisttory(empresa, data[0].proveedorNum, data[0].correo, numerolote, x.NumeroFactura, x.fecha, true);

                    });
                }
                else
                {
                    if (FueEnviado == "NO")
                    {
                        data.ForEach(x =>
                        {
                            sendHisttory(empresa, data[0].proveedorNum, data[0].correo, numerolote, x.NumeroFactura, x.fecha, false);

                        });
                    }
                }
            };

            return "";
        }
        public  async void sendHisttory(string empresa,string CodigoProveedor, string CorreoProveedor, string numeroLote,string factura, DateTime fecha, bool enviado)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_InsertNotificacionPago_HN_GT]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Empresa", empresa));
                    cmd.Parameters.Add(new SqlParameter("@Codigo_Proveedor", CodigoProveedor));
                    cmd.Parameters.Add(new SqlParameter("@Correo_Proveedor", CorreoProveedor));
                    cmd.Parameters.Add(new SqlParameter("@Numero_Lote", numeroLote));
                    cmd.Parameters.Add(new SqlParameter("@Factura", factura));
                    cmd.Parameters.Add(new SqlParameter("@Fecha_Factura", fecha));
                    cmd.Parameters.Add(new SqlParameter("@Enviado", (enviado ? "SI" : "NO")));


                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            //response.Add(getListaOrdenesIniciadas(reader));
                        }
                    }
                    sql.Close();
                }
            }
        }
        public async Task<string> CorreoEnviado(string empresa, string CodigoProveedor,string numeroLote)
        {
            string response = "";
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_GetCorreoEnviadoPagoProveedor]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Empresa", empresa));
                    cmd.Parameters.Add(new SqlParameter("@proveedor", CodigoProveedor));
                    cmd.Parameters.Add(new SqlParameter("@numerolote", numeroLote));
                    


                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response = reader["Enviado"].ToString();
                        }
                    }
                    sql.Close();
                }
            }
            return response;
        }

    }
}
