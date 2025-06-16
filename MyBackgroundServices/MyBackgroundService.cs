using DigitalSignageSevice.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace DigitalSignageSevice.MyBackgroundServices
{
    public class MyBackgroundService : BackgroundService
    {
        private readonly string connectionString;
        GateInformationRepository gateInformationRepository;
        DigitalSignarlRepository digitalSignarlRepository;

        public MyBackgroundService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            gateInformationRepository = new GateInformationRepository(connectionString);
            digitalSignarlRepository = new DigitalSignarlRepository(connectionString);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Kiểm tra nếu bị hủy khởi động
            if (stoppingToken.IsCancellationRequested)
                return;

            // Chỉ chạy một lần khi app start
            Console.WriteLine("MyBackgroundService started...");

            // Gọi logic khởi tạo, ví dụ: khởi tạo dữ liệu cho 10 gate
            for (int i = 1; i <= 10; i++)
            {
                string gate = $"G{i}";
                Console.WriteLine($"Initializing gate {gate}...");
                await AssignBestFlightToGate(i.ToString());
            }

            // Khi hoàn tất, dừng service (nếu cần)
            Console.WriteLine("Gate initialization completed.");
        }

        private async Task AssignBestFlightToGate(string gate)
        {
            await Task.Delay(100);
            Console.WriteLine($"Assigned flight to gate {gate}");
            var flight = gateInformationRepository.GetFlightsByGate(gate);
            if (flight != null)
            {
                Console.WriteLine($"Assigned flight to gate {gate} | Flight: {flight.LineCode + flight.Number}");
                await digitalSignarlRepository.UpdateFlightToGate(flight);
            }
            else
            {
                Console.WriteLine($"No flight found for gate {gate}, sending ClearGate");
            }
        }
    }
}
