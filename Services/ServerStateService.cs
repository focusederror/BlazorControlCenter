using System.Net.Sockets;
using static BlazorControlCenter.Components.Pages.Home;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorControlCenter.Services
{
    public class ServerStateService
    {

        private readonly object _lock = new object();
        public Dictionary<int, ArduinoClient> Clients { get; } = new Dictionary<int, ArduinoClient>();
        public List<string> LogMessages { get; } = new List<string>();
        public event Action<Scd4xDataPoint>? OnScd4xDataReady;

        //Events
        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        //state-modifying methods
        public void AddClient(TcpClient client, int id)
        {
            var arduinoClient = new ArduinoClient { Id = id, TcpClient = client };
            lock (_lock)
            {
                Clients[id] = arduinoClient;

                AddLogMessage($"Client connected from {client.Client.RemoteEndPoint}, it has been assigned the ID: {id}");
            }
        }

        public void RemoveClient(int id)
        {
            bool removed = false;

            lock (_lock)
            {
                removed = Clients.Remove(id);
            }

            if (removed)
            {
                AddLogMessage($"Client with ID: {id} has been removed.");
            }
            else
            {
                AddLogMessage($"Attempted to remove non-existent client with ID: {id}");
            }

        }

        public void AddLogMessage(string message)
        {
            string timedMessage = $"[{DateTime.Now:HH:mm:ss}]: {message}";
            lock (_lock)
            {
                LogMessages.Add(timedMessage);
                if (LogMessages.Count() > 40)
                {
                    LogMessages.RemoveAt(0);
                }
            }
            NotifyStateChanged();
        }

        public void ProcessClientData(int clientId, string data)
        {
            var client = Clients.FirstOrDefault(c => c.Value.Id == clientId).Value;
            if (client == null) return;

            string[] dataArray = data.Split(";"); //data coming in the form SCD4X;co2ppm;temperature(c);humidity
            if (dataArray.Length == 4 && dataArray[0] == "SCD4X")
            {
                if (decimal.TryParse(dataArray[1], out decimal co2) &&
                float.TryParse(dataArray[2], out float temp) &&
                float.TryParse(dataArray[3], out float rh))
                {
                    temp = (temp * (9.0F / 5.0F)) + 32; //C to F

                    Scd4xDataPoint dataPoint = new Scd4xDataPoint(co2, temp, rh, clientId);

                    AddLogMessage($"Received from Client {clientId}: {co2} ppm | {temp.ToString("n2")}°F | {rh}%");
                    
                    //Fire event
                    OnScd4xDataReady?.Invoke(dataPoint);
                }
            }
        }


    }

    public class ArduinoClient
    {
        public int Id { get; set; }

        public TcpClient TcpClient { get; set; } = null!;
        public string Name => $"Client {Id} ({TcpClient.Client.RemoteEndPoint})";

        //public ApexCharts chart 
    }

    public class Scd4xDataPoint
    {
        public int clientId { get; set; }
        public decimal CO2 { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }

        public long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public Scd4xDataPoint(decimal cO2, float temperature, float humidity, int id)
        {
            CO2 = cO2;
            Temperature = temperature;
            Humidity = humidity;
            clientId = id;
        }

    }
}