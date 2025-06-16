using DigitalSignageSevice.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DigitalSignageSevice.Repositories
{
    public class GateInformationRepository
    {
        string connectionString;
        public GateInformationRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        #region Get All # Done 
        public AHT_GateInformation? GetFlightsByGate(string gate)
        {
            var data = GetFlightByGate(gate);
            foreach (DataRow row in data.Rows)
            {
                return new AHT_GateInformation
                {
                    Id = row["Id"]?.ToString(),
                    Adi = row["Adi"]?.ToString(),
                    LineCode = row["LineCode"]?.ToString(),
                    Number = row["Number"]?.ToString(),
                    ScheduledDate = row["ScheduledDate"]?.ToString(),
                    Schedule = ParseDateTime(row["Schedule"]),
                    Estimated = ParseDateTime(row["Estimated"]),
                    Actual = ParseDateTime(row["Actual"]),
                    Status = row["Status"]?.ToString(),
                    Gate = row["Gate"]?.ToString(),
                    Remark = row["Remark"]?.ToString(),
                    Mcdt = row["Mcdt"]?.ToString()
                };
            }

            return null;
        }

        private DateTime? ParseDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (DateTime.TryParse(value.ToString(), out var result))
                return result;

            return null;
        }
        public DataTable GetFlightByGate(string gate)
        {
            string query = " SELECT TOP 1 *FROM [MSMQFLIGHT].[dbo].[AHT_GateInformation] WHERE Gate = '"+gate+"' AND TRY_CAST(Mcdt AS DATETIME) IS NOT NULL " +
                             " AND GETDATE() BETWEEN DATEADD(MINUTE, -1500, TRY_CAST(Mcdt AS DATETIME)) " +
                             " AND DATEADD(MINUTE,  20, TRY_CAST(Mcdt AS DATETIME)) "+
                             " AND ISNULL(Remark, '') NOT IN ('Departed', 'Cancelled', 'Gate closed', '') "+
                             " ORDER BY ABS(DATEDIFF(MINUTE, TRY_CAST(Mcdt AS DATETIME), GETDATE()))";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }
        #endregion

    }
}
