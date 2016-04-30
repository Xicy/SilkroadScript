using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Shared.Core.Security;

namespace Shared.Core.Network
{
    internal class Server
    {
        public void Connect(string ip, ushort port)
        {
            if (_remoteSocket != null)
            {
                Disconnect();
            }

            //Create objects 
            _remoteRecvBuffer = new TransferBuffer(8192, 0, 0);
            _remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Thread Management
            if (_thPacketProcessor == null)
            {
                _thPacketProcessor = new Thread(ThreadedPacketProcessing)
                {
                    Name = "Proxy.Network.Server.PacketProcessor",
                    IsBackground = true
                };
                _thPacketProcessor.Start();
            }

            try
            {
                //Recreate the Security
                _remoteSecurity = new Security.Security();

                //Connect
                _remoteSocket.Connect(ip, port);

                if (!_remoteSocket.Connected) return;
                Connected?.Invoke(ip, port);
                _doPacketProcess = true;
                _remoteSocket.BeginReceive(_remoteRecvBuffer.Buffer, 0, 8192, SocketFlags.None, WaitForServerData,
                    _remoteSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            _doPacketProcess = false;
            _isClosing = true;

            if (_remoteSocket != null)
            {
                if (_remoteSocket.Connected)
                {
                    _remoteSocket.Shutdown(SocketShutdown.Both);
                }
                _remoteSocket.Close();
                _remoteSocket = null;
            }

            _isClosing = false;
        }

        private void WaitForServerData(IAsyncResult ar)
        {
            if (_isClosing || !_doPacketProcess) return;
            Socket worker = null;
            try
            {
                worker = (Socket) ar.AsyncState;
                var rcvdBytes = worker.EndReceive(ar);
                if (rcvdBytes > 0)
                {
                    _remoteRecvBuffer.Size = rcvdBytes;
                    _remoteSecurity.Recv(_remoteRecvBuffer);
                }
                else
                {
                    //RaiseEvent
                    if (Kicked != null)
                    {
                        Kicked();
                        worker = null;
                    }
                    else
                    {
                        Console.WriteLine("You have been kicked by the Security Software.");
                    }
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset) //Disconnected
                {
                    Console.WriteLine("You have been disconnected from the Server.");

                    //RaiseEvent
                    Disconnected?.Invoke();

                    //Mark worker as null to stop reciving
                    worker = null;
                }
                else
                {
                    Console.WriteLine(se.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                worker?.BeginReceive(_remoteRecvBuffer.Buffer, 0, 8192, SocketFlags.None, WaitForServerData,
                    worker);
            }
        }

        private void Send(byte[] buffer)
        {
            if (_isClosing || !_doPacketProcess) return;
            if (!_remoteSocket.Connected) return;
            try
            {
                _remoteSocket.Send(buffer);
                if (buffer.Length == 0)
                {
                    Console.WriteLine("buffer.Length == 0");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Send(Packet packet)
        {
            _remoteSecurity.Send(packet);
        }

        #region Events

        public delegate void ConnectedEventHandler(string ip, ushort port);

        public event ConnectedEventHandler Connected;

        public delegate void DisconnectedEventHandler();

        public event DisconnectedEventHandler Disconnected;

        // Not sure about this
        public delegate void KickedEventHandler();

        public event KickedEventHandler Kicked;

        public delegate void PacketReceivedEventHandler(Packet packet);

        public event PacketReceivedEventHandler PacketReceived;

        public delegate void PacketSendEventHandler(Packet packet);

        public event PacketSendEventHandler PacketSend;

        #endregion

        #region Fields

        private Socket _remoteSocket;

        private Security.Security _remoteSecurity;

        //Used for Transfare and Processing
        private TransferBuffer _remoteRecvBuffer;
        private List<Packet> _remoteRecvPackets;
        private List<KeyValuePair<TransferBuffer, Packet>> _remoteSendBuffers;

        private Thread _thPacketProcessor;

        //Used to Provide secure connect/disconnect while changing from Gateway to Agent
        private bool _isClosing;
        private bool _doPacketProcess;

        #endregion

        #region PacketProcessor

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
                _processServerPackets();
                Thread.Sleep(1);
            }

            if (_isClosing)
            {
                return; //Jump out.
            }

            goto begin; //goto begin and wait until we should process again
        }

        private void _processServerPackets()
        {
            if (_isClosing || !_doPacketProcess) return;

            #region Handle Recive

            _remoteRecvPackets = _remoteSecurity.TransferIncoming();
            if (_remoteRecvPackets != null)
            {
                foreach (var packet in _remoteRecvPackets.Where(packet => packet.Opcode != 0x5000 && packet.Opcode != 0x9000))
                {
                    //RaiseEvent if event is assigned.
                    PacketReceived?.Invoke(packet);
                }
            }

            #endregion

            #region Handle Send

            _remoteSendBuffers = _remoteSecurity.TransferOutgoing();
            if (_remoteSendBuffers == null) return;
            foreach (var buffer in _remoteSendBuffers)
            {
                PacketSend?.Invoke(buffer.Value);
                Send(buffer.Key.Buffer);
            }

            #endregion
        }

        #endregion
    }
}