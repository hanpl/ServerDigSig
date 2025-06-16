using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using System.Xml.Linq;
using DigitalSignageSevice.Repositories;
using System.Threading.Tasks;
using DigitalSignageSevice.Models;
using static Dapper.SqlMapper;

namespace DigitalSignageSevice.Hubs
{
    public class DashboardHub : Hub
    {
        DigitalSignarlRepository digitalSignarlRepository;
        public DashboardHub(IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            digitalSignarlRepository = new DigitalSignarlRepository(connectionString);
        }


        string serverGroup = "Server";
        public override async Task OnConnectedAsync()
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var clientIpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()?.Trim();

                if (string.IsNullOrEmpty(clientIpAddress))
                {
                    Console.WriteLine("Client IP Address is null or empty. Connection aborted.");
                    return;
                }

                Console.WriteLine($"{connectionId} connected from {clientIpAddress}");

                var locationflight = digitalSignarlRepository.GetFlight(clientIpAddress);
                var location = locationflight.Name + "," + locationflight.LeftRight;
                Console.WriteLine("Location: " + location);
                if (!string.IsNullOrEmpty(location))
                {
                    if (location.Contains("ahtbg", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Client identified as AHTBG");
                        var gateSuffix = locationflight.Name?.Replace("AHTBG", "", StringComparison.OrdinalIgnoreCase)?.Trim();

                        bool updated = digitalSignarlRepository.UpdateOnConnectionId(clientIpAddress, connectionId);
                        if (updated)
                        {
                            Console.WriteLine("UpdateNewFlight ok" + location);

                            if (!string.IsNullOrEmpty(connectionId))
                            {
                                Console.WriteLine($"Sending flight info to client: {connectionId}");
                                await Clients.Client(connectionId).SendAsync("SendToClient", locationflight);
                                await SendToServer();
                            }
                        }
                    }
                    else if (location.Contains("server", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Client identified as Server");
                        await Groups.AddToGroupAsync(connectionId, serverGroup);
                        Console.WriteLine($"Added client to group: {serverGroup}");

                        bool updated = digitalSignarlRepository.UpdateOnConnectionId(clientIpAddress, connectionId);
                        if (updated)
                        {
                            Console.WriteLine("Updated ConnectionId for server in database.");
                            var data = digitalSignarlRepository.GetClients();
                            await Clients.Group(serverGroup).SendAsync("SendToServer", data);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Location is null or empty, skipping further processing.");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var clientIp = Context.GetHttpContext()?.Connection?.RemoteIpAddress?.ToString()?.Trim();

            Console.WriteLine($"{connectionId} Disconnected from {clientIp}");
            await Groups.RemoveFromGroupAsync(connectionId, "serverGroup");

            if (!string.IsNullOrEmpty(clientIp))
            {
                if (digitalSignarlRepository.UpdateDisConnectionId(clientIp))
                {
                    Console.WriteLine("✅ Updated disconnect info in DB");
                    await SendToServer();
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReloadServerDashboard()
        {
            var data = digitalSignarlRepository.GetConnectionId("Admin");
            if (data != null)
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    if (data[i].ConnectionId != "")
                    {
                        await Clients.Client(data[i].ConnectionId).SendAsync("ReceivedServerReload", data[i].ConnectionId, "Reload");
                    }
                }
            }
        }

        public async Task UpdateModeAutoSync(string ip, string auto, string connectionId)
        {
            var data = digitalSignarlRepository.UpdateAutoAndGateFlight(ip, auto);
            if (data != null && data.ConnectionId != null)
            {
                await Clients.Client(data.ConnectionId).SendAsync("SendToClient", data);
            }
        }


        public async Task ReSendToClients(string name, string leftRingt, string connectionId, string ip)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Client Send");

            }
            else
            {
                Console.WriteLine("Location is null or empty, skipping further processing.");
            }
        }

        public async Task SendToServer()
        {
            var data = digitalSignarlRepository.GetClients();
            await Clients.Group(serverGroup).SendAsync("SendToServer", data);
        }
        public async Task SendToClient(AHT_DigitalSignageV2 entity)
        {

            if (!string.IsNullOrEmpty(entity.ConnectionId))
            {
                Console.WriteLine(entity.ConnectionId);
                await Clients.Client(entity.ConnectionId).SendAsync("SendToClient", entity);

            }
        }

    }
}
