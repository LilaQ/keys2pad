using System.IO.Pipes;
using System.Text;

namespace Keys2Pad.Services;

public sealed class CommandServer : IDisposable
{
    public const string PipeName = "Keys2Pad.Command.v1";
    private readonly Func<string, Task<string>> _handler;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public CommandServer(Func<string, Task<string>> handler)
    {
        _handler = handler;
        _loop = Task.Run(() => ListenAsync(_cts.Token));
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await using NamedPipeServerStream pipe = new(
                    PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
                using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                await using StreamWriter writer = new(pipe, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = true };
                string command = await reader.ReadLineAsync(token).ConfigureAwait(false) ?? string.Empty;
                string result = await _handler(command).ConfigureAwait(false);
                await writer.WriteLineAsync(result).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (IOException)
            {
                // A CLI client can disappear at any point; accept the next one.
            }
        }
    }

    public static async Task<string?> TrySendAsync(string command, int timeoutMs = 1500)
    {
        try
        {
            await using NamedPipeClientStream pipe = new(
                ".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            using CancellationTokenSource timeout = new(timeoutMs);
            await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);
            await using StreamWriter writer = new(pipe, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = true };
            using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            await writer.WriteLineAsync(command).ConfigureAwait(false);
            return await reader.ReadLineAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _loop.Wait(TimeSpan.FromSeconds(1)); } catch (AggregateException) { }
        _cts.Dispose();
    }
}
