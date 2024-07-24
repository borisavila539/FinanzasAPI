using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Core.DTOs;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Net;
using Core.Utilities;
using System.Net.Mime;
using System.Net.Http;
using System.Globalization;
using WMS_API.Features.Utilities;

namespace FinanzasAPI.Features.Repositories
{
    public class FacturasProveedorRepository : IFacturasProveedorRepository
    {
        private readonly string _connectionStringCubo;

        public FacturasProveedorRepository(IConfiguration configuracion)
        {
            _connectionStringCubo = configuracion.GetConnectionString("IMFinanzas");
        }

        public async Task<string> enviarRetencion(string dataAreaId, string fechaReporte, string correos, string copiaCorreos)
        {
            Mes mes = new();
            DateTime fecha = DateTime.Parse(fechaReporte);

            List<string> correosRecibido = correos.Split(',').ToList();
            List<string> correosCopia = copiaCorreos.Split(',').ToList();
            correosRecibido = correosRecibido.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            correosCopia = correosCopia.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            string /*nombreMes = mes.nombreMes(fecha.Month),*/ responseFromSend = "",
                   response = "", regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov|hn|gt)$",
                   retencionesPath = $@"\\10.100.1.35\\Modinter Guatemala\\";
            /*retencionesISRPath = $@"\\10.100.1.35\\Modinter Guatemala\\{fecha.Year}\\RETENCIONES ISR\\{nombreMes}",
            retencionesIVAPath = $@"\\10.100.1.35\\Modinter Guatemala\\{fecha.Year}\\RETENCIONES IVA\\{nombreMes}";*/

            int cantidadCorreosEnviados = 0;

            List<FacturasProveedor> facturasProveedor = new();
            List<string> proveedoresSinCorreo = new();
            List<string> correosNoEnviados = new();
            List<string> correosDeArchivosNoEncontrados = new();
            List<Envios> envios_CorreoPendiente = new();
            List<Envios> envios_RetencionesPendientes = new();
            List<Envios> envios_Error = new();

            try
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[Retenciones].[Get_FacturasConRetencionesPendientes]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@DATAAREAID", dataAreaId));
                        cmd.Parameters.Add(new SqlParameter("@FECHA", fecha));
                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                facturasProveedor.Add(getFacturasProveedor(reader));
                            }
                        }
                    }
                }

                List<string> groupedProveedores = facturasProveedor.Select(x => x.CuentaDelProveedor).Distinct().ToList();

                foreach (string proveedor in groupedProveedores)
                {
                    List<Archivos> archivos = new();
                    Envios envio = new();
                    FacturasProveedor facturaProveedor = facturasProveedor.Find(x => x.CuentaDelProveedor == proveedor);
                    bool esCorreo = Regex.IsMatch(facturaProveedor.Email, regex, RegexOptions.IgnoreCase);
                    int cantidadRetencionesProveedor = facturaProveedor.CantidadRetenciones;

                    if (esCorreo)
                    {
                        List<string> facturas = new();
                        List<DateTime> fechasFacturas = new();

                        facturas = facturasDelProveedor(facturasProveedor, proveedor);
                        List<Factura> facturasConFechas = facturasPorFecha(facturasProveedor);

                        foreach (string factura in facturas)
                        {
                            if (!fechasFacturas.Contains(facturasConFechas.Find(x => x.NumeroDeFactura == factura).FechaRetencion))
                            {
                                fechasFacturas.Add(facturasConFechas.Find(x => x.NumeroDeFactura == factura).FechaRetencion);
                            }

                            string nombreArchivo = proveedor + "-" + factura;
                            int anio = (facturasConFechas.Find(x => x.NumeroDeFactura == factura).FechaRetencion).Year;
                            string nombreMes = mes.nombreMes((facturasConFechas.Find(x => x.NumeroDeFactura == factura).FechaRetencion).Month);

                            string retencionesISRPath = retencionesPath + $@"{anio}\RETENCIONES ISR\\{nombreMes}";
                            string retencionesIVAPath = retencionesPath + $@"{anio}\RETENCIONES IVA\\{nombreMes}";

                            byte[] retencionISR = null, retencionIVA = null;
                            string pdfFilePathISR = Path.Combine(retencionesISRPath, nombreArchivo + ".pdf"),
                                   pdfFilePathIVA = Path.Combine(retencionesIVAPath, nombreArchivo + ".pdf");

                            if (File.Exists(pdfFilePathIVA) && (cantidadRetencionesProveedor == 1 || cantidadRetencionesProveedor == 2))
                            {
                                retencionIVA = File.ReadAllBytes(pdfFilePathIVA);
                                Archivos archivo = new()
                                {
                                    Nombre = $"Retención IVA {factura}.pdf",
                                    Archivo = retencionIVA
                                };

                                archivos.Add(archivo);
                            }

                            if (File.Exists(pdfFilePathISR) && cantidadRetencionesProveedor == 2)
                            {
                                retencionISR = File.ReadAllBytes(pdfFilePathISR);
                                Archivos archivo = new()
                                {
                                    Nombre = $"Retención ISR {factura}.pdf",
                                    Archivo = retencionISR
                                };

                                archivos.Add(archivo);
                            }
                        }
                        int cantidadFacturasProveedor = facturasConFechas.FindAll(x => x.CuentaDelProveedor == proveedor).Count();
                        int cantidadRetencionesAdjuntar = cantidadRetencionesProveedor * cantidadFacturasProveedor;

                        if (archivos.Count < cantidadRetencionesAdjuntar)
                        {
                            correosDeArchivosNoEncontrados.Add(facturaProveedor.CuentaDelProveedor);


                            envio.NumeroFactura = string.Join(", ", facturas);
                            envio.CuentaProveedor = facturaProveedor.CuentaDelProveedor;
                            envio.NombreProveedor = facturasProveedor.Where(x => x.CuentaDelProveedor == facturaProveedor.CuentaDelProveedor).Select(x => x.NombreDelProveedor).FirstOrDefault();
                            envios_RetencionesPendientes.Add(envio);
                        }
                        else
                        {
                            responseFromSend = composeEmail(fechasFacturas, facturas, facturaProveedor.Email, correosRecibido, archivos, correosCopia);
                            cantidadCorreosEnviados++;

                            if (responseFromSend != "")
                            {
                                cantidadCorreosEnviados--;
                                correosNoEnviados.Add(facturaProveedor.CuentaDelProveedor);
                                envios_Error.Add(envio);
                            }
                        }
                    }
                    else
                    {
                        List<string> facturas = new();
                        facturas = facturasDelProveedor(facturasProveedor, proveedor);

                        envio.CuentaProveedor = proveedor;
                        envio.NombreProveedor = facturasProveedor.Where(x => x.CuentaDelProveedor == facturaProveedor.CuentaDelProveedor).Select(x => x.NombreDelProveedor).FirstOrDefault();
                        envio.NumeroFactura = string.Join(", ", facturas);

                        envios_CorreoPendiente.Add(envio);
                        proveedoresSinCorreo.Add(facturaProveedor.CuentaDelProveedor);
                    }
                }

                response = "Se enviaron " + cantidadCorreosEnviados + " correos. ";

                if (proveedoresSinCorreo.Count > 0)
                {
                    response += "No se encontraron correos para los proveedores: " + string.Join(", ", proveedoresSinCorreo) + ". ";
                }

                if (correosNoEnviados.Count > 0)
                {
                    response += "No se pudieron enviar correos a los proveedores: " + string.Join(", ", correosNoEnviados) + ". Error: " + responseFromSend + ". ";
                }

                if (correosDeArchivosNoEncontrados.Count > 0)
                {
                    response += "No se encontrarón las retenciones de los proveedores: " + string.Join(", ", correosDeArchivosNoEncontrados);
                }

                foreach (var factura in facturasProveedor)
                {
                    bool enviado = true;
                    if (proveedoresSinCorreo.Exists(x => x == factura.CuentaDelProveedor) || correosNoEnviados.Exists(x => x == factura.CuentaDelProveedor) || correosDeArchivosNoEncontrados.Exists(x => x == factura.CuentaDelProveedor))
                    {
                        enviado = false;
                    }
                    int responseInsert = GuardarHistorico(factura.CuentaDelProveedor, factura.Email, factura.NumeroDeFactura, factura.LoteDeDiario, "imgt", enviado, factura.FechaRetencion).Result;

                    if (responseInsert <= 0)
                    {
                        response += "No se pudo guardar en el log la factura " + factura.NumeroDeFactura + " del proveedor " + factura.CuentaDelProveedor;
                    }
                }

                if (envios_Error.Count > 0 || envios_CorreoPendiente.Count > 0 || envios_RetencionesPendientes.Count > 0)
                {
                    foreach (string correo in correosCopia)
                    {
                        string responseEmail = composeEmail_RetencionesEnviadas(correo, fecha, envios_Error, envios_RetencionesPendientes, envios_CorreoPendiente);
                    }
                }
            }
            catch (Exception e)
            {
                response = e.Message;
            }

            return response;
        }

        public string composeEmail(List<DateTime> fechas, List<string> facturas, string correo, List<string> correosRecibido, List<Archivos> archivos, List<string> copiasCorreo)
        {
            CultureInfo culture = new CultureInfo("es-ES", false);
            string fechaWithFormat = "";

            if (fechas.Count > 1)
            {
                fechaWithFormat = fechas.Min().ToString(culture.DateTimeFormat.LongDatePattern, culture) + " hasta " + fechas.Max().ToString(culture.DateTimeFormat.LongDatePattern, culture);
            }
            else
            {
                fechaWithFormat = fechas[0].ToString(culture.DateTimeFormat.LongDatePattern, culture);
            }

            string response = "";
            string asunto = "Retenciones";
            string textBody = "<p>Buen dia,</p>" +
                              $"<p>Se adjuntan retenciones para las facturas: {string.Join(", ", facturas)} correspondientes a la fecha {fechaWithFormat}. Favor confirmar de recibido al siguiente correo: {string.Join(", ", correosRecibido)}</p>";

            response = enviarCorreo(correo, asunto, textBody, archivos, null, copiasCorreo);

            return response;
        }

        public string composeEmail_RetencionesEnviadas(string correo, DateTime fecha, List<Envios> envios_Error, List<Envios> envios_RetencionesPendientes, List<Envios> envios_CorreoPendientes)
        {
            string response = "";

            CultureInfo culture = new CultureInfo("es-ES", false);
            string fechaWithFormat = fecha.ToString(culture.DateTimeFormat.LongDatePattern, culture);

            string asunto = "Retenciones Pendientes", tablaHtml = "";
            string textBody = "<p>Buen dia,</p>" +
                              $"<p>El dia {fechaWithFormat} no se pudieron enviar las siguientes retenciones: </p>";

            if (envios_RetencionesPendientes.Count > 0)
            {
                textBody += "<p><strong>Retenciones No Encontradas</strong></p>";
                tablaHtml = "<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 600 + "><tr bgcolor='#4da6ff'>" +
                                  "<td><b>Cuenta del Proveedor</b></td>" +
                                  "<td><b>Nombre del Proveedor</b></td>" +
                                  "<td><b>Facturas</b></td>" +
                                  "</tr>";

                foreach (Envios envio in envios_RetencionesPendientes)
                {
                    tablaHtml += "<tr><td>" + envio.CuentaProveedor + "</td><td>" + envio.NombreProveedor + "</td><td>" + envio.NumeroFactura + "</td></tr>";
                }

                tablaHtml += "</table>";
                textBody += tablaHtml;
            }

            if (envios_CorreoPendientes.Count > 0)
            {
                textBody += "<p><strong>Correos No Encontrados</strong></p>";
                tablaHtml = "<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 600 + "><tr bgcolor='#4da6ff'>" +
                                  "<td><b>Cuenta del Proveedor</b></td>" +
                                  "<td><b>Nombre del Proveedor</b></td>" +
                                  "<td><b>Facturas</b></td>" +
                                  "</tr>";

                foreach (Envios envio in envios_CorreoPendientes)
                {
                    tablaHtml += "<tr><td>" + envio.CuentaProveedor + "</td><td>" + envio.NombreProveedor + "</td><td>" + envio.NumeroFactura + "</td></tr>";
                }

                tablaHtml += "</table>";
                textBody += tablaHtml;
            }

            if (envios_Error.Count > 0)
            {
                textBody += "<p><strong>Error en Envio</strong></p>";
                tablaHtml = "<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 600 + "><tr bgcolor='#4da6ff'>" +
                                  "<td><b>Cuenta del Proveedor</b></td>" +
                                  "<td><b>Nombre del Proveedor</b></td>" +
                                  "<td><b>Facturas</b></td>" +
                                  "</tr>";

                foreach (Envios envio in envios_Error)
                {
                    tablaHtml += "<tr><td>" + envio.CuentaProveedor + "</td><td>" + envio.NombreProveedor + "</td><td>" + envio.NumeroFactura + "</td></tr>";
                }

                tablaHtml += "</table>";
                textBody += tablaHtml;
            }

            response = enviarCorreo(correo, asunto, textBody, null, null, null);

            return response;
        }

        public string enviarCorreo(string correoDestino, string asunto, string textBody, List<Archivos> archivos = null, byte[] imagen = null, List<string> copiasCorreo = null)
        {
            string response = "";
            string htmlMessage = "<html><body>";
            htmlMessage += textBody;
            htmlMessage += "</body></html>";

            try
            {
                string emailOrigen =VariablesGlobales.Correo;
                string contrasena = VariablesGlobales.Correo_Password;

                MailMessage OMailMesage = new MailMessage(emailOrigen, correoDestino, asunto, textBody);
                OMailMesage.IsBodyHtml = true;

                if (archivos != null)
                {
                    foreach (var archivo in archivos)
                    {
                        MemoryStream ms = new MemoryStream(archivo.Archivo);
                        OMailMesage.Attachments.Add(new Attachment(ms, archivo.Nombre));
                    }
                }

                if (copiasCorreo != null)
                {
                    foreach (var correo in copiasCorreo)
                    {
                        OMailMesage.CC.Add(new MailAddress(correo));
                    }
                }

                if (imagen != null)
                {
                    //Agrega imagen al cuerpo del mensaje
                    htmlMessage = "<html><body>";
                    htmlMessage += textBody;
                    htmlMessage += "<img src='cid:image1' alt='Embedded Image'>";
                    htmlMessage += "</body></html>";

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, null, MediaTypeNames.Text.Html);

                    LinkedResource imageResource = new LinkedResource(new System.IO.MemoryStream(imagen), MediaTypeNames.Image.Jpeg);
                    imageResource.ContentId = "image1";

                    htmlView.LinkedResources.Add(imageResource);

                    OMailMesage.AlternateViews.Add(htmlView);
                }

                SmtpClient oSmtpClient = new SmtpClient();

                oSmtpClient.Host = "smtp.office365.com";
                oSmtpClient.Port = 587;
                oSmtpClient.EnableSsl = true;
                oSmtpClient.UseDefaultCredentials = false;

                NetworkCredential userCredential = new NetworkCredential(emailOrigen, contrasena);

                oSmtpClient.Credentials = userCredential;

                oSmtpClient.Send(OMailMesage);
                oSmtpClient.Dispose();
            }
            catch (SmtpFailedRecipientsException recipientsException)
            {
                response = $"Failed recipients: {string.Join(", ", recipientsException.InnerExceptions.Select(fr => fr.FailedRecipient))}";

                return response;
            }

            return response;
        }

        public List<string> facturasDelProveedor(List<FacturasProveedor> facturasProveedor, string cuentaProveedor)
        {
            List<string> facturas = new();
            List<string> retenDeFacturas = facturasProveedor.FindAll(x => x.CuentaDelProveedor == cuentaProveedor).Select(x => x.NumeroDeFactura).ToList();

            foreach (string factura in retenDeFacturas)
            {
                facturas.Add(removeTextFromHyphen(factura));
            }

            facturas = facturas.Distinct().ToList();

            return facturas;
        }

        public List<Factura> facturasPorFecha(List<FacturasProveedor> facturasProveedor)
        {
            List<Factura> facturas = new();

            foreach (FacturasProveedor fact in facturasProveedor)
            {
                Factura factura = new()
                {
                    CuentaDelProveedor = fact.CuentaDelProveedor,
                    NumeroDeFactura = removeTextFromHyphen(fact.NumeroDeFactura),
                    CantidadRetenciones = fact.CantidadRetenciones,
                    FechaRetencion = fact.FechaRetencion
                };

                if (!facturas.Exists(x => x.NumeroDeFactura == factura.NumeroDeFactura))
                {
                    facturas.Add(factura);
                }
            }

            facturas = facturas.Distinct().ToList();

            return facturas;
        }

        public string removeTextFromHyphen(string factura)
        {
            bool tieneGuion = factura.IndexOf('-') > 0;
            string modifiedString = factura;

            if (tieneGuion)
            {
                int hyphenIndex = factura.LastIndexOf('-');
                modifiedString = factura.Substring(0, hyphenIndex);
            }

            return modifiedString;
        }

        public async Task<int> GuardarHistorico(string cuenta, string email, string numFactura, string lote, string dataAreaId, bool estado, DateTime fechaRetencion)
        {
            int response = 0;
            try
            {
                using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
                {
                    using (SqlCommand cmd = new SqlCommand("[Retenciones].Insert_LogRetencionesEnviadas", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@cuentaProveedor", cuenta));
                        cmd.Parameters.Add(new SqlParameter("@email", email));
                        cmd.Parameters.Add(new SqlParameter("@numFactura", numFactura));
                        cmd.Parameters.Add(new SqlParameter("@lote", lote));
                        cmd.Parameters.Add(new SqlParameter("@fecha", DateTime.Now));
                        cmd.Parameters.Add(new SqlParameter("@dataAreaId", dataAreaId));
                        cmd.Parameters.Add(new SqlParameter("@estado", estado));
                        cmd.Parameters.Add(new SqlParameter("@fechaRetencion", fechaRetencion));
                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response = (int)reader["cantidad"];
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return response;
        }

        public FacturasProveedor getFacturasProveedor(SqlDataReader reader)
        {
            return new FacturasProveedor()
            {
                CuentaDelProveedor = reader["CuentaDelProveedor"].ToString(),
                NombreDelProveedor = reader["NombreDelProveedor"].ToString(),
                EstadoEnviado = (bool)reader["EstadoEnviado"],
                Email = reader["Email"].ToString(),
                NumeroDeFactura = reader["NumeroDeFactura"].ToString(),
                Debito = (decimal)reader["Debito"],
                LoteDeDiario = reader["LoteDeDiario"].ToString(),
                Asiento = reader["Asiento"].ToString(),
                Factura = reader["Factura"].ToString(),
                CantidadRetenciones = (int)reader["CantidadRetenciones"],
                FechaRetencion = (DateTime)reader["FechaRetencion"]
            };
        }
        public void descargarImagen(byte[] image)
        {
            byte[] imageBytes = image;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Specify the file path on the desktop
            string filePath = Path.Combine(desktopPath, "logo.png");

            // Save the image data to the specified file
            File.WriteAllBytes(filePath, imageBytes);
        }

        public class Envios
        {
            public string NombreProveedor { get; set; }
            public string CuentaProveedor { get; set; }
            public string NumeroFactura { get; set; }
            public string? RetencionPendiente { get; set; }
        }

    }
}
