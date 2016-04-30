using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared.Core.Security;

namespace Shared.Core.Network
{
    internal class Client
    {
        //Listen for ClientConnection on port
        public void Listen(ushort port)
        {
            _localSecurity = new Security.Security();
            _localSecurity.GenerateSecurity(true, true, true);

            _localRecvBuffer = new TransferBuffer(8192, 0, 0);
            _localListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Thread Management
            _thPacketProcessor = new Thread(ThreadedPacketProcessing)
            {
                Name = "Proxy.Network.Client.PacketProcessor",
                IsBackground = true
            };
            _thPacketProcessor.Start();

            try
            {
                if (_localListener.IsBound == false)
                {
                    _localListener.Bind(new IPEndPoint(IPAddress.Loopback, port));
                    _localListener.Listen(1);
                }
                _localListener.BeginAccept(OnClientConnect, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Listen()
        {
            _doPacketProcess = false;

            if (_localSocket != null)
            {
                _localSocket.Shutdown(SocketShutdown.Both);
                _localSocket.Close();
            }

            _localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Accept a new Client.
            _localListener.BeginAccept(OnClientConnect, null);
        }

        public void Shutdown()
        {
            _doPacketProcess = false;
            _isClosing = true;

            _thPacketProcessor.Join();

            //Close Socket
            if (_localSocket != null)
            {
                _localSocket.Shutdown(SocketShutdown.Both);
                _localSocket.Close();
            }
            _localSocket = null;

            //Close listener
            if (_localListener != null)
            {
                _localListener.Close();
                _localListener = null;
            }

            _localSecurity = null;

            _localRecvBuffer = null;
            _localRecvPackets = null;
            _localSendBuffers = null;

            if (_thPacketProcessor != null)
            {
                _thPacketProcessor = null;
            }
        }

        private void OnClientConnect(IAsyncResult ar)
        {
            if (_isClosing) return;
            try
            {
                _doPacketProcess = true;
                _localSocket = _localListener.EndAccept(ar);
                _localSocket.BeginReceive(_localRecvBuffer.Buffer, 0, 8192, SocketFlags.None, WaitForData, _localSocket);

                _localSecurity = new Security.Security();
                _localSecurity.GenerateSecurity(false, false, false);

                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                throw new Exception("Network.Client.OnClientConnect: " + ex.Message, ex);
            }
        }

        private void WaitForData(IAsyncResult ar)
        {
            if (_isClosing || !_doPacketProcess) return;
            Socket worker = null;
            try
            {
                worker = (Socket) ar.AsyncState;
                var rcvdBytes = worker.EndReceive(ar);
                if (rcvdBytes > 0)
                {
                    _localRecvBuffer.Size = rcvdBytes;
                    _localSecurity.Recv(_localRecvBuffer);
                }
                else
                {
                    //Console.WriteLine("Client Disconnected");
                    Disconnected?.Invoke();
                    Listen();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset) //Client Disconnected
                {
                    //Console.WriteLine("Client Disconnected");
                    //RaisEvent
                    Disconnected?.Invoke();
                    Listen();

                    //Mark worker as null to stop reciveing.
                    worker = null;
                }
                else
                {
                    throw new Exception("Proxy.Network.Client.WaitForData: " + se.Message, se);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Proxy.Network.Client.WaitForData: " + ex.Message, ex);
            }
            finally
            {
                worker?.BeginReceive(_localRecvBuffer.Buffer, 0, 8192, SocketFlags.None, WaitForData, worker);
            }
        }

        private void Send(byte[] buffer)
        {
            if (_isClosing || !_doPacketProcess || !_localSocket.Connected) return;
            try
            {
                _localSocket.Send(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Send(Packet packet)
        {
            _localSecurity.Send(packet);
        }

        private void ThreadedPacketProcessing()
        {
            begin:

            //Wait until we should process packets
            while (!_doPacketProcess && !_isClosing)
            {
                Thread.Sleep(1);
            }

            while (_doPacketProcess && !_isClosing)
            {
                _processClientPackets();
                Thread.Sleep(1);
            }

            if (_isClosing)
            {
                return; //Jump out.
            }

            goto begin; //goto begin and wait until we should process again
        }

        private void _processClientPackets()
        {
            if (_isClosing || !_doPacketProcess) return;
            _localRecvPackets = _localSecurity.TransferIncoming();
            if (_localRecvPackets != null)
            {
                foreach (var packet in _localRecvPackets)
                {
                    if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000 || packet.Opcode == 0x2001)
                    {
                        continue;
                    }

                    //Send to PacketHandler
                    PacketReceived?.Invoke(packet);
                }
            }

            _localSendBuffers = _localSecurity.TransferOutgoing();
            if (_localSendBuffers == null) return;
            foreach (var buffer in _localSendBuffers)
            {
                PacketSend?.Invoke(buffer.Value);
                Send(buffer.Key.Buffer);
            }
        }

        #region Events

        public delegate void PacketReceivedEventHandler(Packet packet);

        public event PacketReceivedEventHandler PacketReceived;

        public delegate void PacketSendEventHandler(Packet packet);

        public event PacketSendEventHandler PacketSend;

        public delegate void ConnectedEventHandler();

        public event ConnectedEventHandler Connected;

        public delegate void DisconnectedEventHandler();

        public event DisconnectedEventHandler Disconnected;

        #endregion

        #region Fields

        private Socket _localListener;
        private Socket _localSocket;

        private Security.Security _localSecurity;

        private TransferBuffer _localRecvBuffer;
        private List<Packet> _localRecvPackets;
        private List<KeyValuePair<TransferBuffer, Packet>> _localSendBuffers;

        private Thread _thPacketProcessor;
        private bool _isClosing;
        private bool _doPacketProcess;

        #endregion
    }
}