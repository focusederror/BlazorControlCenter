// In: /Services/TcpServerService.cs

using BlazorControlCenter.Services;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorControlCenter.Services;
public sealed class TcpServerService : BackgroundService
{
    private readonly ServerStateService _state;
    private readonly ILogger<TcpServerService> _log;
    private TcpListener? _listener;
    private int _nextClientId = 0;
    private readonly ConcurrentDictionary<int, TcpClient> _clients = new();

    private readonly IPAddress _bindAddress = IPAddress.Any;
    private readonly int _port = 8080;
    public TcpServerService(ServerStateService state, ILogger<TcpServerService> log)
    {
        _state = state;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = IPAddress.Parse("192.168.12.188");
            var port = 8080;
            _listener = new TcpListener(ipAddress, port);
            _listener.Start();
            _state.AddLogMessage($"TCP Server Started. Listening on *:{port}");

            // Listen for clients until the application requests shutdown
            while (!cancellationToken.IsCancellationRequested)
            {
                _state.AddLogMessage("...waiting for a new client connection...");
                // 3. Use the async version to accept clients without blocking
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
                int clientId = Interlocked.Increment(ref _nextClientId);

                // 4. Tell the state service about the new client
                _state.AddClient(client, clientId);

                // 5. Handle this client's communication in its own Task, so we can immediately listen for another
                _ = HandleClientCommAsync(client, clientId, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _state.AddLogMessage("TCP Server is shutting down.");
        }
        catch (Exception ex)
        {
            _state.AddLogMessage($"TCP Server error: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        return Task.CompletedTask;
    }

    private async Task HandleClientCommAsync(TcpClient tcpClient, int clientId, CancellationToken token)
    {

        _state.AddLogMessage($"Handler created for Client {clientId}.");

        try
        {
            await using var stream = tcpClient.GetStream();
            var buffer = new byte[4096];

            while (!token.IsCancellationRequested && tcpClient.Connected)
            {
                // 6. Use ReadAsync to wait for data without blocking a thread
                int bytesRead = await stream.ReadAsync(buffer, token);

                // If ReadAsync returns 0, it means the client has closed the connection
                if (bytesRead == 0) break;

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                // 7. Report incoming data to the state service
                 
                _state.ProcessClientData(clientId, data);

                // Optional: Send acknowledgment
                var ackMessage = "ACK: " + data + "\n";
                byte[] ackBuffer = Encoding.ASCII.GetBytes(ackMessage);
                await stream.WriteAsync(ackBuffer, token);
            }
        }
        catch (OperationCanceledException) {}
        catch (System.IO.IOException) {}
        catch (Exception ex)
        {
            _state.AddLogMessage($"Error with client {clientId}: {ex.Message}");
        }
        finally
        {
            _state.AddLogMessage($"Closing connection and cleaning up for Client {clientId}.");
            
            // 8. Always ensure we remove the client from the state service on disconnect
            _state.RemoveClient(clientId);
            tcpClient.Close();
        }
    }
}