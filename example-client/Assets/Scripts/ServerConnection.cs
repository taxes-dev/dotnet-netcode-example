using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// This class maintains the connection to the server and shuttles messages back and forth between the server and client.
    /// </summary>
    public class ServerConnection : MonoBehaviour, ISubscriber
    {
        #region Private fields
        private const int INITIAL_QUEUE_SIZE = 100;
        private const int BUFFER_SIZE = 2048;
        private const int POLL_TIMEOUT = 10;
        private Socket socket;
        private Queue<Msg> messages;
        private byte[] packetBuffer;
        #endregion

        /// <summary>
        /// Server IP to connect to.
        /// </summary>
        public string ServerIPAddress = "127.0.0.1";

        /// <summary>
        /// Server port to connect to.
        /// </summary>
        public int ServerIPPort = 59999;

        public ServerConnection()
        {
            this.messages = new Queue<Msg>(INITIAL_QUEUE_SIZE);
            this.packetBuffer = new byte[BUFFER_SIZE];
        }

        void Start()
        {
            DontDestroyOnLoad(this);
            var endpoint = new IPEndPoint(IPAddress.Parse(ServerIPAddress), ServerIPPort);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                this.socket.Connect(endpoint);
                Debug.Log("[ServerConnection] Connected to server.");
                MessageBroker.Instance.Subscribe(this, Msgs.CMD_ALL);
            }
            catch (SocketException ex)
            {
                this.socket = null;
                Debug.LogError("[ServerConnection] Unable to connect to server. " + ex.Message);
            }
        }

        void Update()
        {
            if (this.socket != null)
            {
                ReceiveMessages();
                SendQueuedMessages();
            }
        }

        /// <summary>
        /// Poll the socket for waiting data and parse.
        /// </summary>
        private void ReceiveMessages()
        {
            if (this.socket.Poll(POLL_TIMEOUT, SelectMode.SelectRead))
            {
                // Read waiting raw bytes.
                int received = this.socket.Receive(this.packetBuffer, BUFFER_SIZE, SocketFlags.None);
                if (received > 0)
                {
                    int offset = 0;
                    while (offset < received)
                    {
                        // While we have data to read, attempt to parse the bytes into a packet.
                        IPacket packet;
                        int processed = Framing.ReadPacket(this.packetBuffer, received - offset, offset, out packet);
                        if (packet != null)
                        {
                            // Generic message packet (enqueue in our local message broker
                            if (packet.PacketHeader == Framing.HDR_PACKET)
                            {
                                Packet msgPacket = (Packet)packet;
                                Debug.Log(String.Format("[ServerConnection.ReceiveMessages] Got message packet containing {0} messages at timestamp {1:g}",
                                    msgPacket.Count, DateTime.FromBinary(msgPacket.Timestamp)));
                                foreach (Msg msg in msgPacket)
                                {
                                    // Sanity check in case server messages somehow get sent to client
                                    if (msg.IsClient())
                                    {
                                        MessageBroker.Instance.Enqueue(msg);
                                    }
                                }
                            }
                            // Monster packet (tell the spawn manager to create/update new monster instance)
                            else if (packet.PacketHeader == Framing.HDR_MONSTER)
                            {
                                MonsterPacket monPacket = (MonsterPacket)packet;
                                Debug.Log(String.Format("[ServerConnection.ReceiveMessages] Got monster packet containing monster instance {0}", monPacket.MonsterInstance.ObjectID));
                                SpawnManager.Instance.QueueMonsterUpdate(monPacket.MonsterInstance);
                            }
                        }
                        offset += processed;
                    }
                }
                else
                {
                    Debug.LogWarning("[ServerConnection.ReceiveMessages] No data received. Connection lost?");
                }
            }
        }

        /// <summary>
        /// Send any waiting server messages via the socket.
        /// </summary>
        private void SendQueuedMessages()
        { 
            while (messages.Count > 0)
            {
                // Create a new packet and add waiting messages to it
                var packet = new Packet();
                while (packet.Count < Packet.MESSAGES_PER_PACKET)
                {
                    packet.Add(messages.Dequeue());
                    if (messages.Count == 0)
                        break;
                }

                // Write the serialized packet into the buffer
                int packetSize = Framing.WritePacket(packetBuffer, BUFFER_SIZE, 0, packet);

                try
                {
                    // Attempt to send the serialized packet to the server
                    if (this.socket.Poll(POLL_TIMEOUT, SelectMode.SelectWrite))
                    {
                        int sent = this.socket.Send(packetBuffer, packetSize, SocketFlags.None);
                        if (sent == 0)
                        {
                            Debug.LogWarning("[ServerConnection.SendQueuedMessages] 0 bytes written to socket. Connection lost?");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[ServerConnection.SendQueuedMessages] Unable to write to socket. Connection lost?");
                    }
                }
                catch (SocketException ex)
                {
                    Debug.LogError("[ServerConnection.SendQueuedMessages] " + ex.Message + " " + ex.NativeErrorCode);
                }
            }
            // TODO: send periodic heartbeat if no other messages queued
        }

        void Destroy()
        {
            if (this.socket != null)
            {
                MessageBroker.Instance.Unsubscribe(this);
                // TODO: send message to server to tell it client is disconnecting
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
                Debug.Log("[ServerConnection] Disconnected from server.");
                this.socket = null;
            }
        }

        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            if (message.IsServer())
            {
                this.messages.Enqueue(message);
            }
        }

        #endregion
    }
}
