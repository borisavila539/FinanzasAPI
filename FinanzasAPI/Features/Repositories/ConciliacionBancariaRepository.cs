using Core.DTOs.ConciliacionBancaria;
using Core.Interfaces;
using Infraestructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinanzasAPI.Features.Repositories
{
    public class ConciliacionBancariaRepository : IConciliacionBancaria
    {
        private readonly string _ConnectionString;

        public ConciliacionBancariaRepository(AxContext context, IConfiguration configuracion)
        {
            _ConnectionString = configuracion.GetConnectionString("IMFinanzas");

        }

        public async Task<string> CreatereConciliacionFolders(string path, string date, string dataAreaId)
        {

            List<string> banksAccount = new();

            try
            {
                var response = new List<ConciliacionBancariaDTO>();

                using (SqlConnection sql = new SqlConnection(_ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("[Conciliaciones].[GetBanks]", sql))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Date", date));
                        cmd.Parameters.Add(new SqlParameter("@DataAreaId", dataAreaId));


                        await sql.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Add(getConciliacionBancaria(reader));
                            }
                        }
                    }
                }

                List<ConciliacionBancariaDTO> banks = response;
                //List<Bank> banks = _unitOfWork.Repository<Bank>().GetSP<Bank>("[Conciliaciones].[GetBanks]", parameters).ToList();

                foreach (ConciliacionBancariaDTO bank in banks)
                {
                    string folderPath = $@"{path}{bank.WeekNumber} {bank.CurrencyCode}\{bank.AccountId}";

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        banksAccount.Add(bank.AccountId);
                    }
                }

            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

            return "Se crearón las carpetas de los bancos: " + string.Join(", ", banksAccount);
        }
        public ConciliacionBancariaDTO getConciliacionBancaria(SqlDataReader reader)
        {
            return new ConciliacionBancariaDTO()
            {
                AccountId = reader["AccountId"].ToString(),
                CurrencyCode = reader["CurrencyCode"].ToString(),
                WeekNumber = reader["WeekNumber"].ToString(),
            };
        }
    }
}
