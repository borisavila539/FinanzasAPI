using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.DTOs;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using IM_Auditelas_WS;

namespace FinanzasAPI.Features.Repositories
{
    public class AXRepository : IAX
    {
        private readonly string _connectionStringCubo;
        private readonly AxContext _context;

        public AXRepository(AxContext context, IConfiguration configuracion)
        {
            _context = context;
            _connectionStringCubo = configuracion.GetConnectionString("IMDesarrollos");
        }

        public string InsertDefectos(int id)
        {
            object obj = new object();
            DEFECTOSHEADER HEADER = new DEFECTOSHEADER();
            List<DEFECTOSLINE> LIST = new List<DEFECTOSLINE>();
            List<ObtenerDatosDefectosDTO> detalle = GetDatosDefectos(id);
            List<ObtenerDetalleRolloDTO> encabezado = getObtenerDetalleRollo(id);
            

            detalle.ForEach(x =>
            {
                DEFECTOSLINE line = new DEFECTOSLINE();
                line.APVENDROLL = encabezado[0].Numero_Rollo_Proveedor;
                line.ROLLID = encabezado[0].Id_Pieza;
                line.DEFECTOID = x.Id.ToString();
                line.LEVEL1 = x.Nivel_1.ToString();
                line.LEVEL2 = x.Nivel_2.ToString();
                line.LEVEL3 = x.Nivel_3.ToString();
                line.LEVEL4 = x.Nivel_4.ToString();
                line.OBSERVACION = encabezado[0].Observaciones;
                LIST.Add(line);

            });

            HEADER.LINES = LIST.ToArray();
            string defectosLines = SerializationService.Serialize(HEADER);
            CallContext context = new CallContext { Company = "IMHN" };
            var serviceClient = new M_AudiTelasClient(GetBinding(),GetEndpointAddr());

            serviceClient.ClientCredentials.Windows.ClientCredential.UserName = "servicio_ax";
            serviceClient.ClientCredentials.Windows.ClientCredential.Password = "Int3r-M0d@.aX$3Rv";
            
            try {
                string dataValidation = string.Format("<INTEGRATION><COMPANY><CODE>{0}</CODE><USER>{1}</USER></COMPANY></INTEGRATION>", context.Company, "servicio_ax");
                var resp = serviceClient.initAsync(context,dataValidation, defectosLines);
       
                return resp.Result.response;
            }
            catch (Exception ex)
            {
                return null;
            }




            

        }

        public  List<ObtenerDatosDefectosDTO> GetDatosDefectos(int idRollo)
        {
            var response = new List<ObtenerDatosDefectosDTO>();

            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDefectosTelas]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@idrollo", idRollo));

                    sql.Open();
                    using (var reader =  cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(GetDatosDefectos(reader));
                        }
                    }
                }
                return response;
            }
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
        public List<ObtenerDetalleRolloDTO> getObtenerDetalleRollo(int Id)
        {
            using (SqlConnection sql = new SqlConnection(_connectionStringCubo))
            {
                using (SqlCommand cmd = new SqlCommand("[IM_ObtenerDetalleRollo]", sql))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Id", Id));

                    var response = new List<ObtenerDetalleRolloDTO>();

                    sql.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(getObtenerDetalleRollo(reader));
                        }
                    }
                    return response;
                }
            }
        }
        public ObtenerDetalleRolloDTO getObtenerDetalleRollo(SqlDataReader reader)
        {
            return new ObtenerDetalleRolloDTO()
            {
                id = Convert.ToInt32(reader["id"].ToString()),
                Id_Pieza = reader["Id_Pieza"].ToString(),
                Numero_Rollo_Proveedor = reader["Numero_Rollo_Proveedor"].ToString(),
                Observaciones = reader["Observaciones"].ToString()
            };
        }
        private NetTcpBinding GetBinding()
        {
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.Name = "NetTcpBinding_IM_WMSCreateJournalServices";
            netTcpBinding.MaxBufferSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            return netTcpBinding;
        }

        private EndpointAddress GetEndpointAddr()
        {
            string url = "net.tcp://gim-dev-aos:8201/DynamicsAx/Services/IM_AudiTelasGP";
            string user = "sqladmin@intermoda.com.hn";

            var uri = new Uri(url);
            var epid = new UpnEndpointIdentity(user);
            var addrHdrs = new AddressHeader[0];
            var endpointAddr = new EndpointAddress(uri, addrHdrs); //, epid, addrHdrs);
            return endpointAddr;
        }
        
    }
    public static class SerializationService
    {
        public static string Serialize<T>(this T toSerialize)
        {
            var serializer = new XmlSerializer(toSerialize.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T DeSerialize<T>(string datos)
        {
            T type;
            var serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(datos))
            {
                type = (T)serializer.Deserialize(reader);
            }

            return type;
        }
    }
}
