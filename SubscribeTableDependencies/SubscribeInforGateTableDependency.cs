using DigitalSignageSevice.Hubs;
using DigitalSignageSevice.Models;
using DigitalSignageSevice.Repositories;
using Microsoft.AspNetCore.SignalR;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace DigitalSignageSevice.SubscribeTableDependencies
{
    public class SubscribeInforGateTableDependency
    {
        SqlTableDependency<AHT_GateInformation> tableDependency;
        DashboardHub dashboardHub;
        DigitalSignarlRepository digitalSignarlRepository;
        GateInformationRepository gateInformationRepository;
        public SubscribeInforGateTableDependency(DashboardHub dashboardHub, IConfiguration configuration)
        {
            this.dashboardHub = dashboardHub;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            digitalSignarlRepository = new DigitalSignarlRepository(connectionString);
            gateInformationRepository = new GateInformationRepository(connectionString);
        }
        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<AHT_GateInformation>(connectionString, tableName: "AHT_GateInformation", mapper: null, includeOldValues: true);
            tableDependency.OnChanged += TableDependency_Onchanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }
        private async void TableDependency_Onchanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<AHT_GateInformation> args)
        {
            var changeType = args.ChangeType;
            var newData = args.Entity;
            var oldData = args.EntityOldValues;

            if (changeType == ChangeType.Update && oldData != null && newData != null)
            {
                var oldGate = oldData?.Gate;
                var newGate = newData?.Gate;
                await dashboardHub.SendToServer();

                var flight = gateInformationRepository.GetFlightsByGate(newGate);
                if (flight != null)
                {
                    Console.WriteLine($"Assigned flight to gate {newGate} | Flight: {flight.LineCode + flight.Number}");
                    await digitalSignarlRepository.UpdateFlightToGate(flight);

                    await sendToClient(newGate);
                    //await dashboardHub.Clients.Group("G"+newGate).SendAsync("SendToClient", flight);
                }
                else
                {
                    Console.WriteLine($"No flight found for gate {newGate}, sending ClearGate");
                }


                if (!string.Equals(oldGate, newGate, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Flight {newData.LineCode} moved from {oldGate} to {newGate}");

                    if (!string.IsNullOrEmpty(oldGate))
                    {
                        var flightnew = gateInformationRepository.GetFlightsByGate(oldGate);
                        if (flight != null)
                        {
                            Console.WriteLine($"Assigned flight to gate {oldGate} | Flight: {flight.LineCode + flight.Number}");
                            await digitalSignarlRepository.UpdateFlightToGate(flightnew);
                            await sendToClient(oldGate);
                            //await dashboardHub.Clients.Group("G" + oldGate).SendAsync("SendToClient", flightnew);
                        }
                        else
                        {
                            Console.WriteLine($"No flight found for gate {oldGate}, sending ClearGate");
                        }
                    }

                    //await dashboardHub.Clients.Group(newGate).SendAsync("UpdateGate", newData);
                }


            }
        }

        private async Task sendToClient(string gate)
        {
            var flights = digitalSignarlRepository.GetConnectionId(gate); 

            if (flights == null || flights.Count == 0)
            {
                Console.WriteLine($"No flights found for gate {gate} to send.");
                return;
            }

            foreach (var flight in flights)
            {
                if (!string.IsNullOrEmpty(flight.ConnectionId))
                {
                    try
                    {
                        await dashboardHub.Clients.Client(flight.ConnectionId).SendAsync("SendToClient", flight);
                        Console.WriteLine($"Sent flight {flight.Name} to client {flight.ConnectionId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send flight {flight.Name} to {flight.ConnectionId}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping flight {flight.Name}, ConnectionId is null or empty.");
                }
            }

        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
