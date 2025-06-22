using System.Net.Sockets;

namespace BlazorControlCenter.Services
{
    public class ServerStateService
    {

        private readonly object _lock = new object();
        public Dictionary<int, ArduinoClient> Clients { get; } = new Dictionary<int, ArduinoClient>();
        public List<string> LogMessages { get; } = new List<string>();

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
            }
            NotifyStateChanged();
        }


    }

    public class ArduinoClient
    {
        public int Id { get; set; }
        public TcpClient TcpClient { get; set; } = null!;
        public string Name => $"Client {Id} ({TcpClient.Client.RemoteEndPoint})";
    }
}
