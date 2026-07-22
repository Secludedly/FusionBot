using System;
using System.Net.Sockets;

namespace SysBot.Base;

/// <summary>
/// Abstract class representing the communication over a Wi-Fi socket.
/// </summary>
public abstract class SwitchSocket : IConsoleConnection
{
    protected readonly IWirelessConnectionConfig Info;

    private readonly ProtocolType Protocol;

    private readonly SocketType Type;

    protected SwitchSocket(IWirelessConnectionConfig wi, SocketType type = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp)
    {
        Type = type;
        Protocol = protocol;
        Connection = new Socket(type, protocol);
        Info = wi;
        Name = Label = wi.IP;
    }

    public int BaseDelay { get; set; } = 64;

    public bool Connected => Connection.Connected;

    /// <summary>
    /// Probes the underlying socket to determine whether it is genuinely usable, rather than
    /// trusting <see cref="Socket.Connected"/> alone. That flag only reflects the state as of the
    /// last I/O operation and can keep reporting <c>true</c> long after the remote host has dropped
    /// the link (e.g. a slow console causing SocketException 10054, "forcibly closed by remote host").
    /// A dead-but-still-"Connected" socket is what leaves a bot showing as online while the routine
    /// silently sits idle, because <see cref="Connect"/> would otherwise skip re-establishing it.
    /// </summary>
    /// <returns><c>true</c> only if the socket is connected and not in a closed/reset state.</returns>
    protected bool IsConnectionAlive()
    {
        var socket = Connection;
        if (!socket.Connected)
            return false;

        try
        {
            // Poll (timeout in microseconds) reports SelectRead when the socket has data waiting OR
            // when the peer has closed/reset the connection. If it is readable but nothing is
            // Available to read, the peer has gone away and the socket is dead.
            bool readableButEmpty = socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0;
            return !readableButEmpty;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public int DelayFactor { get; set; } = 256;

    public string Label { get; set; }

    public int MaximumTransferSize { get; set; } = 0x1C0;

    public string Name { get; }

    protected Socket Connection { get; private set; }

    public abstract void Connect();

    public abstract void Disconnect();

    public void InitializeSocket() => Connection = new Socket(Type, Protocol);

    public void Log(string message) => LogInfo(message);

    public void LogError(string message) => LogUtil.LogError(Label, message);

    public void LogInfo(string message) => LogUtil.LogInfo(Label, message);

    public abstract void Reset();
}
