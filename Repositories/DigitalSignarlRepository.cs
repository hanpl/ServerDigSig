using DigitalSignageSevice.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System;

namespace DigitalSignageSevice.Repositories
{
    public class DigitalSignarlRepository
    {
        string connectionString;
        public DigitalSignarlRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }


        public AHT_DigitalSignageV2? GetFlight(string ip)
        {
            AHT_DigitalSignageV2? aHT_DigitalSignageV2 = null;
            var data = GetFlightByIp(ip);
            foreach (DataRow row in data.Rows)
            {
                // Kiểm tra nếu Location trong dòng hiện tại khớp với locationToSearch UpdateAutoAndGateFlight
                if (row["Ip"]?.ToString() == ip)
                {
                    aHT_DigitalSignageV2 = new AHT_DigitalSignageV2
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = row["Name"]?.ToString(),
                        Ip = row["Ip"]?.ToString(),
                        Location = row["Location"]?.ToString(),
                        LeftRight = row["LeftRight"]?.ToString(),
                        Remark = row["Remark"]?.ToString(),
                        Status = row["Status"]?.ToString(),
                        Iata = row["Iata"]?.ToString(),
                        NameLineCode = row["NameLineCode"]?.ToString(),
                        GateChange = row["GateChange"]?.ToString(),
                        Mode = row["Mode"]?.ToString(),
                        Auto = row["Auto"]?.ToString(),
                        Work = row["Work"]?.ToString(),
                        TimeMcdt = row["TimeMcdt"]?.ToString(),
                        Live = row["Live"]?.ToString(),
                        LiveAuto = row["LiveAuto"]?.ToString(),
                        City = row["City"]?.ToString(),
                        MixCity = row["MixCity"]?.ToString(),
                        MixVideos = row["MixVideos"]?.ToString(),
                        ConnectionId = row["ConnectionId"]?.ToString(),
                        IdFlight = row["IdFlight"]?.ToString()
                    };
                    break;
                }
            }
            return aHT_DigitalSignageV2;
        }

        public DataTable GetFlightByIp(string ip)
        {
            string query = "SELECT * FROM [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] WHERE Ip = '" + ip + "'";
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


        #region Update Auto And gate Flight
        public AHT_DigitalSignageV2? UpdateAutoAndGateFlight(string ip, string auto)
        {
            if (UpdateAuto(ip, auto))
            {
                AHT_DigitalSignageV2? aHT_DigitalSignageV2 = null;
                var data = GetFlightByIp(ip);
                foreach (DataRow row in data.Rows)
                {
                    // Kiểm tra nếu Location trong dòng hiện tại khớp với locationToSearch UpdateAutoAndGateFlight
                    if (row["Ip"]?.ToString() == ip)
                    {
                        aHT_DigitalSignageV2 = new AHT_DigitalSignageV2
                        {
                            Id = Convert.ToInt32(row["ID"]),
                            Name = row["Name"]?.ToString(),
                            Ip = row["Ip"]?.ToString(),
                            Location = row["Location"]?.ToString(),
                            LeftRight = row["LeftRight"]?.ToString(),
                            Remark = row["Remark"]?.ToString(),
                            Status = row["Status"]?.ToString(),
                            Iata = row["Iata"]?.ToString(),
                            NameLineCode = row["NameLineCode"]?.ToString(),
                            GateChange = row["GateChange"]?.ToString(),
                            Mode = row["Mode"]?.ToString(),
                            Auto = row["Auto"]?.ToString(),
                            Work = row["Work"]?.ToString(),
                            TimeMcdt = row["TimeMcdt"]?.ToString(),
                            Live = row["Live"]?.ToString(),
                            LiveAuto = row["LiveAuto"]?.ToString(),
                            City = row["City"]?.ToString(),
                            MixCity = row["MixCity"]?.ToString(),
                            MixVideos = row["MixVideos"]?.ToString(),
                            ConnectionId = row["ConnectionId"]?.ToString(),
                            IdFlight = row["IdFlight"]?.ToString()
                        };
                        break;
                    }
                }
                return aHT_DigitalSignageV2;
            }
            else
            {
                return null;
            }
            
            
        }

        public bool UpdateAuto(string ip, string auto)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] SET Auto =@Auto WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Auto", auto);
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        #endregion



        public bool UpdateOnConnectionId(string ip, string connect)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] SET Status = 'True' , ConnectionId =@ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ConnectionId", connect);
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }





        public async Task UpdateFlightToGate (AHT_GateInformation flight)
        {

            DateTime? timeMcdt = flight.Actual.HasValue ? flight.Actual : flight.Estimated.HasValue ? flight.Estimated : flight.Schedule;

            string query = @"UPDATE [dbo].[AHT_DigitalSignageV2] SET Remark = @Remark, Iata = @Iata, NameLineCode = @LineCode, Mode = @Mode, TimeMcdt = @TimeMcdt, Live = @Live, IdFlight = @IdFlight Where Name = @Name";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Remark", flight.Remark ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Iata", flight.LineCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LineCode", (flight.LineCode+ flight.Number) ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Mode", GetMode(flight.LineCode, flight.Gate) ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TimeMcdt",
                    timeMcdt.HasValue
                        ? timeMcdt.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                        : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Live", (object?)flight.LineCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdFlight", (object?)flight.Id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", "AHTBG"+flight.Gate ?? (object)DBNull.Value);
                try
                {
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ UpdateFlightToGate error: " + ex.Message);
                    throw;
                }
            }

        }

        public string? GetMode(string? lineCode, string? gate)
        {
            if (string.IsNullOrEmpty(lineCode) || string.IsNullOrEmpty(gate))
                return null;

            string? result = null;

            string query = @"SELECT IsEgate FROM [MSMQFLIGHT].[dbo].[AHT_EGateForFlight] WHERE Name = @LineCode AND GateNumber = @GateNumber";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LineCode", lineCode);
                cmd.Parameters.AddWithValue("@GateNumber", gate);

                try
                {
                    conn.Open();
                    var isEgate = cmd.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(isEgate) && isEgate.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                    {
                        result = "Yes";
                    }
                    else
                    {
                        result = "No";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ GetMode error: {ex.Message}");
                    result = null;
                }
            }

            return result;
        }

        public bool UpdateDisConnectionId(string ip)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] SET Status = 'False' , ConnectionId = @ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ConnectionId", "");
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }


        //Http Get All
        #region Get All # Done 
        public List<AHT_DigitalSignageV2> GetClients()
        {
            List<AHT_DigitalSignageV2> aHT_DigitalSignages = new List<AHT_DigitalSignageV2>();
            AHT_DigitalSignageV2 aHT_DigitalSignage;
            var data = GetWorkOrderDetailsFromDb();
            foreach (DataRow row in data.Rows)
            {
                aHT_DigitalSignage = new AHT_DigitalSignageV2
                {
                    Id = Convert.ToInt32(row["ID"]),
                    Name = row["Name"]?.ToString(),
                    Ip = row["Ip"]?.ToString(),
                    Location = row["Location"]?.ToString(),
                    LeftRight = row["LeftRight"]?.ToString(),
                    Remark = row["Remark"]?.ToString(),
                    Status = row["Status"]?.ToString(),
                    Iata = row["Iata"]?.ToString(),
                    NameLineCode = row["NameLineCode"]?.ToString(),
                    GateChange = row["GateChange"]?.ToString(),
                    Mode = row["Mode"]?.ToString(),
                    Auto = row["Auto"]?.ToString(),
                    Work = row["Work"]?.ToString(),
                    TimeMcdt = row["TimeMcdt"]?.ToString(),
                    Live = row["Live"]?.ToString(),
                    LiveAuto = row["LiveAuto"]?.ToString(),
                    City = row["City"]?.ToString(),
                    MixCity = row["MixCity"]?.ToString(),
                    MixVideos = row["MixVideos"]?.ToString(),
                    ConnectionId = row["ConnectionId"]?.ToString(),
                    IdFlight = row["IdFlight"]?.ToString()
                };
                aHT_DigitalSignages.Add(aHT_DigitalSignage);
            }
            return aHT_DigitalSignages;
        }
        public DataTable GetWorkOrderDetailsFromDb()
        {
            string query = "SELECT * FROM [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] order by Location ASC";
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

        //Http Get data By Gate Name (get connetionID by gate name call from Subscribe)
        #region Get data By Gate Name
        public List<AHT_DigitalSignageV2> GetConnectionId(string gate)
        {
            List<AHT_DigitalSignageV2> aHT_DigitalSignages = new List<AHT_DigitalSignageV2>();
            AHT_DigitalSignageV2 aHT_DigitalSignage;
            var data = getConnectionId(gate);
            foreach (DataRow row in data.Rows)
            {
                aHT_DigitalSignage = new AHT_DigitalSignageV2
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = row["Name"].ToString(),
                    Ip = row["Ip"].ToString(),
                    Location = row["Location"].ToString(),
                    Live = row["Live"].ToString(),
                    Remark = row["Remark"].ToString(),
                    Status = row["Status"].ToString(),
                    LeftRight = row["LeftRight"].ToString(),
                    GateChange = row["GateChange"].ToString(),
                    Mode = row["Mode"].ToString(),
                    Auto = row["Auto"].ToString(),
                    Iata = row["Iata"].ToString(),
                    NameLineCode = row["NameLineCode"].ToString(),
                    TimeMcdt = row["TimeMcdt"].ToString(),
                    ConnectionId = row["ConnectionId"].ToString(),
                    LiveAuto = row["LiveAuto"].ToString(),
                    IdFlight = row["IdFlight"].ToString(),
                };
                aHT_DigitalSignages.Add(aHT_DigitalSignage);
            }
            return aHT_DigitalSignages;
        }
        public DataTable getConnectionId(string gate)
        {
            string query = "SELECT * FROM [MSMQFLIGHT].[dbo].[AHT_DigitalSignageV2] where Name = 'AHTBG"+gate+"' order by Location ASC";
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
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

        #region Update Data Gate
        public bool UpdateDisConnectionIdGate(string ip)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignage] SET Status =@Status, ConnectionId =@ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", "No");
                        command.Parameters.AddWithValue("@ConnectionId", "");
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }
        public bool UpdateWorkToNo(string ip)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignage] SET Work = 'No' WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        public bool UpdateOnConnectionIdGate(string ip, string connect)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignage] SET Status =@Status, ConnectionId =@ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", "Yes");
                        command.Parameters.AddWithValue("@ConnectionId", connect);
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }
        public bool UpdateToDB(string name, string lere, string column, string value)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignage] SET " + column+ "  = @Value WHERE Name = @Name AND LeftRight = @Lere";
            //Console.WriteLine(query);
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Value", value);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Lere", lere);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }
        public bool UpdateModeToDB(string name, string lere, string mode, string value)
        {
            string query = "UPDATE [MSMQFLIGHT].[dbo].[AHT_DigitalSignage] SET Auto = @mode, LiveAuto = @Value WHERE Name = @Name AND LeftRight = @Lere";
            //Console.WriteLine(query);
            DataTable dataTable = new DataTable();
            //string connectionString = "Data Source=172.17.2.38;Initial Catalog=MSMQFLIGHT;Persist Security Info=True;User ID=sa;Password=AHT@2019";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@mode", mode);
                        command.Parameters.AddWithValue("@Value", value);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Lere", lere);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }


        #endregion
    }
}
