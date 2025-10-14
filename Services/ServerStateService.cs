using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
//using static BlazorControlCenter.Components.Pages.Home;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorControlCenter.Services
{
    public class ServerStateService
    {

       // private readonly DbContext _dbContext;

        //public ServerStateService(DbContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}


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
            var arduinoClient = new ArduinoClient(client, id);
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

        public async Task<bool> SendCommandAsync(int clientId, string command, CancellationToken ct = default)
        {
            TcpClient? tcp = null;
            lock (_lock)
            {
                if (!Clients.TryGetValue(clientId, out var arduinoClient))
                {
                    AddLogMessage($"Send failed: client {clientId} not found.");
                    return false;
                }
                tcp = arduinoClient.TcpClient;
            }

            try
            {
                if (tcp == null || !tcp.Connected)
                {
                    AddLogMessage($"Send failed: client {clientId} not connected.");
                    return false;
                }

                var stream = tcp.GetStream();
                var bytes = Encoding.UTF8.GetBytes(command.Trim() + "\n");
                await stream.WriteAsync(bytes, 0, bytes.Length, ct);
                await stream.FlushAsync(ct);

                AddLogMessage($"Sent to Client {clientId}: {command}");
                return true;
            }
            catch (ObjectDisposedException)
            {
                AddLogMessage($"Send failed: client {clientId} socket disposed.");
                return false;
            }
            catch (IOException ex)
            {
                AddLogMessage($"Send failed (IO) to client {clientId}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                AddLogMessage($"Send failed to client {clientId}: {ex.Message}");
                return false;
            }
        }

        public void ProcessClientData(int clientId, string data)
        {
            var client = Clients.FirstOrDefault(c => c.Value.Id == clientId).Value;
            if (client == null) return;

            string[] dataArray = data.Split(";"); //data coming in the form SCD4X;co2ppm;temperature(c);humidity;hum_relay;fan_relay
            if (dataArray[0] == "MAC")
            {

            }
            else if (dataArray.Length == 6 && dataArray[0] == "SCD4X")
            {
                decimal.TryParse(dataArray[1], out decimal co2);
                float.TryParse(dataArray[2], out float temp);
                float.TryParse(dataArray[3], out float rh);
                string hum_relay = dataArray[4];
                string fan_relay = dataArray[5];
                temp = (temp * (9.0F / 5.0F)) + 32; //C to F

                Scd4xDataPoint dataPoint = new Scd4xDataPoint(co2, temp, rh, clientId);

                AddLogMessage($"Received from Client {clientId}:  " + data);

                OnScd4xDataReady?.Invoke(dataPoint);

            } else
            {
                AddLogMessage($"Unrecognized data from Client {clientId}:  " + data);
            }
        }
    }

    public class ArduinoClient
    {
        public int Id { get; set; }
        public TcpClient TcpClient { get; }
        public string Endpoint { get; }

        public bool humState = false;
        public bool fanState = false;

        public ArduinoClient(TcpClient tcpClient, int id)
        {
            TcpClient = tcpClient;
            Id = id;
            Endpoint = TcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        }
        public string Name => $"Client {Id} ({Endpoint})";
    }

    public class Scd4xDataPoint
    {
        public int clientId { get; set; }
        public decimal CO2 { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("hh:mm");
        public Scd4xDataPoint(decimal cO2, float temperature, float humidity, int id)
        {
            CO2 = cO2;
            Temperature = temperature;
            Humidity = humidity;
            clientId = id;
        }
    }
}