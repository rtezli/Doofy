using System.Threading.Tasks;
using Pixills.Net.Goofy.Modules;
using Pixills.Tools.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Pixills.Net.Goofy
{
    public class Server
    {
        public event Action<LogEventArgs> LogEvent;
        public event Action<ConnectionEventArgs> ClientConnectedEvent;
        public event Action<ConnectionEventArgs> ClientDisconnectedConnectedEvent;
        public event Action<ConnectionEventArgs> ClientConnectionTeardownEvent;

        private bool _isListening;
        private TcpListener _listener;
        private readonly List<Socket> _connections = new List<Socket>();
        private IModule _module;

        public async Task Start(ushort port, ushort backlog)
        {
            Log(LogLevel.Info, "Starting server");
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            _isListening = true;
            _listener.Start(backlog);

            Log(LogLevel.Info, "Loading Modules ");
            _module = new HttpProxyModule.Module();
            _module.LogEvent += Log;
            Log(LogLevel.Info, "Module " + _module.ModuleName + " Loaded");
            Log(LogLevel.Info, "Start listening on port " + port + ". Backlog is " + backlog + ".");
            while (_isListening)
            {
                var socket = _listener.AcceptSocket();
                Log(LogLevel.Info, "Client " + socket.RemoteEndPoint + " connected.");
                await HandleConnection(socket);
            }
        }

        public void Stop()
        {
            _isListening = false;
            _listener.Stop();
        }

        async Task HandleConnection(object o)
        {
            var connectionIsAlive = true;
            var s = o as Socket;
            if (s != null)
                Log(LogLevel.Info, "Start handling client " + s.RemoteEndPoint);
            try
            {

                var inBuffer = new byte[1024];
                _connections.Add(s);
                while (_isListening && connectionIsAlive)
                {
                    using (var ms = new MemoryStream())
                    {
                        int bytes;
                        SocketError socketError;
                        if(s == null)
                            continue;
                        while ((bytes = s.Receive(inBuffer, 0, inBuffer.Length, SocketFlags.None, out socketError)) != 0)
                        {
                            Log(LogLevel.Info, "Read : " + bytes + " bytes");
                            ms.Write(inBuffer, 0, bytes);
                            if (bytes < inBuffer.Length)
                                break;
                        }

                        if (ms.Length > 0)
                        {
                            var result = _module.ProcessRequest(ms);
                            if (!SendingResponseSuccees(s, result))
                            {
                                Log(LogLevel.Error, "Could not send response to client. Closing Connection");
                                break;
                            }

                        }
                        else
                        {
                            Log(LogLevel.Error, "Received 0 bytes. Closing Connection");
                            connectionIsAlive = false;
                            if (socketError != SocketError.Success)
                                break;
                        }

                    }
                }
            }
            catch (SocketException exc)
            {
                Log(LogLevel.Error, "Socket exception: " + exc.Message);
            }
            catch (Exception exc)
            {
                Log(LogLevel.Error, "Exception: " + exc.Message);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                    lock (_connections)
                        _connections.Remove(s);
                }
            }

        }

        private bool SendingResponseSuccees(Socket s, byte[] b)
        {
            var status = SocketError.Success;
            try
            {
                if (b != null && b.Length > 0)
                {
                    s.Send(b, 0, b.Length, SocketFlags.None, out status);
                    return true;
                }
                Log(LogLevel.Error, "Send called without any byte ");
                return false;
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, "Sending response error " + e.Message);
                switch (status)
                {
                    case SocketError.Shutdown:
                        break;
                }
                return false;
            }
        }

        private void Log(LogEventArgs e)
        {
            Log(e.Level, e.Message);
        }

        private void Log(LogLevel l, string s)
        {
            if (LogEvent != null)
                LogEvent(new LogEventArgs(l, s));
        }
    }
}
