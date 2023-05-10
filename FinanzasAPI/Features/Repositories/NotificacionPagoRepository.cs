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

namespace FinanzasAPI.Features.Repositories
{
    public class NotificacionPagoRepository : INotificacionPagoProveedorRepository
    {
        private readonly string _connectionString;
        private readonly AxContext _context;
        private List<ComprobanteRetencionDTO> comprobanteRetencions = new List<ComprobanteRetencionDTO>();
        public NotificacionPagoRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionString = configuracion.GetConnectionString("MicrosoftDynamicsAX_PRO");
        }
        public async Task<List<NotificacionPagoDTO>> getNotificacionPago(string numerolote, int enviar, string empresa)
        {

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
                        await getListaRetencion(numerolote, empresa);
                        enviarCorreo(response,numerolote,empresa);
                    }
  
                    return response;
                }
            } 
        }

        public NotificacionPagoDTO getListaPagos(SqlDataReader reader)
        {
            return new NotificacionPagoDTO()
            {
                correo = reader["correo"].ToString(),
                proveedorNum = reader["proveedorNum"].ToString(),
                NombreProveedor = reader["NombreProveedor"].ToString(),
                Nombrebanco = reader["banco"].ToString(),
                Cuenta = reader["cuenta"].ToString(),
                NumeroFactura = reader["NumeroFactura"].ToString(),
                divisa = reader["divisa"].ToString(),
                monto = (double)reader["monto"],
                empresa = reader["empresa"].ToString()
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
                footer03 = reader["footer03"].ToString()
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
                        while(await reader.ReadAsync())
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

        public void enviarCorreo(List<NotificacionPagoDTO> datos, string numerolote, string empresa)
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
                    comprobantes.Add(comprobante);
                    comprobante = new ComprobanteRetencionPDF_DTO();

                    
                }

                
                //llamar al reporte
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

            proveedores.ForEach(tmp =>
            {
                //campos que llevara la tabla
                List<NotificacionPagoDTO> data = new List<NotificacionPagoDTO>();
                List<ComprobanteRetencionPDF_DTO> pdf = new List<ComprobanteRetencionPDF_DTO>();
                decimal sumaImporte = 0;
                decimal sumaRetension = 0;

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

                if (data[0].correo != null && data[0].correo != "")
                {
                    string correoDestino = data[0].correo;//cambiar a correo destino despues
                    string correoOrigen = "sistema@intermoda.com.hn";
                    string contrasena = "Intermod@2022#";
                    string asunto = "Notificacion de pago";

                    //Cuerpo del correo
                    string html = $"<h3>Estimados señores de: {data[0].NombreProveedor}</h3>" +
                                  $"<p>De parte de {data[0].empresa}, se le notifica que se realizo el pago en las siguientes facturas:</p>" +
                                  "<table border = '1'>" +
                                        "<thead>" +
                                            "<tr>" +
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
                        html += "<tr>" +
                                    $"<td style='padding: 0px 5px;'> {x.Nombrebanco}</td>" +
                                    $"<td style='padding: 0px 5px;'>{x.Cuenta}</td>" +
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
                                "<td style='font-weight: bold;'>Total</td>" +
                                $"<td style='text-align: right;font-weight: bold;'>{total.ToString("0,0.00", CultureInfo.InvariantCulture)} </td>" +
                            "</tr>" +
                        "</body>" +
                    "</table>";

                    MailMessage mailMessage = new MailMessage(correoOrigen, correoDestino, asunto, html);
                    mailMessage.IsBodyHtml = true;

                    //Enviar PDF de Comporbante de Retencion
                    if (pdf.Count() > 0)
                    {
                        Reporting_Services generar = new Reporting_Services();
                        MemoryStream ms = new MemoryStream(generar.GenerateReport_ComprobanteRetencionProveedor(pdf));
                        mailMessage.Attachments.Add(new Attachment(ms, "Comprobante_Retencion.pdf"));
                        byte[] pdfData = ms.ToArray();
                        DateTime date = DateTime.Now;
                        string path = @"\\AppServer\Intermoda\RETENCION AX\RETENCIONES "+ date.Year + @"\" + mes(date.Month);
                        try
                        {
                            
                            File.WriteAllBytes(path + @"\"+ empresa + "-" + pdf[0].comprobante.Substring(11, 8) + "-"+ pdf[0].proveedorNum + ".pdf", pdfData);
                        }
                        catch (Exception ex)
                        {
                            Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + @"\" + empresa + "-" + pdf[0].comprobante.Substring(11, 8) + "-" + pdf[0].proveedorNum + ".pdf", pdfData);
                        }
                        
                    }

                    SmtpClient smtpClient = new SmtpClient();

                    smtpClient.Host = "smtp.office365.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;

                    NetworkCredential networkCredential = new NetworkCredential(correoOrigen, contrasena);

                    smtpClient.Credentials = networkCredential;

                    smtpClient.Send(mailMessage);
                    smtpClient.Dispose();  
                }
            });
        }
    }
}
