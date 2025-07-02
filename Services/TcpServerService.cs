// In: /Services/TcpServerService.cs

using BlazorControlCenter.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BlazorControlCenter.Services;
public class TcpServerService : IHostedService
{
    private readonly ServerStateService _stateService;
    private TcpListener? _listener;
    private int _nextClientId = 0;

    // 1. Dependency Injection happens here
    public TcpServerService(ServerStateService stateService)
    {
        _stateService = stateService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 2. We use Task.Run to start our main listener loop on a background thread
        Task.Run(async () =>
        {
            try
            {
                var ipAddress = IPAddress.Parse("192.168.12.188");
                var port = 8080;
                _listener = new TcpListener(ipAddress, port);
                _listener.Start();
                _stateService.AddLogMessage($"TCP Server Started. Listening on *:{port}");

                // Listen for clients until the application requests shutdown
                while (!cancellationToken.IsCancellationRequested)
                {
                    _stateService.AddLogMessage("...waiting for a new client connection...");
                    // 3. Use the async version to accept clients without blocking
                    TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    int clientId = Interlocked.Increment(ref _nextClientId);

                    // 4. Tell the state service about the new client
                    _stateService.AddClient(client, clientId);

                    // 5. Handle this client's communication in its own Task, so we can immediately listen for another
                    _ = HandleClientCommAsync(client, clientId, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _stateService.AddLogMessage("TCP Server is shutting down.");
            }
            catch (Exception ex)
            {
                _stateService.AddLogMessage($"TCP Server error: {ex.Message}");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        return Task.CompletedTask;
    }

    private async Task HandleClientCommAsync(TcpClient tcpClient, int clientId, CancellationToken token)
    {

        _stateService.AddLogMessage($"Handler created for Client {clientId}.");

        try
        {
            // 'await using' ensures the stream is properly disposed
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

                _stateService.ProcessClientData(clientId, data);

                // Optional: Send acknowledgment
                var ackMessage = "ACK: " + data + "\n";
                byte[] ackBuffer = Encoding.ASCII.GetBytes(ackMessage);
                await stream.WriteAsync(ackBuffer, token);
            }
        }
        catch (OperationCanceledException) { /* Normal on shutdown */ }
        catch (System.IO.IOException) { /* Client disconnected forcibly */ }
        catch (Exception ex)
        {
            _stateService.AddLogMessage($"Error with client {clientId}: {ex.Message}");
        }
        finally
        {
            _stateService.AddLogMessage($"Closing connection and cleaning up for Client {clientId}.");
            
            // 8. Always ensure we remove the client from the state service on disconnect
            _stateService.RemoveClient(clientId);
            tcpClient.Close();
        }
    }
}