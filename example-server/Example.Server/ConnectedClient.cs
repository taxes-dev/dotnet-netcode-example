using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

using Example.Messages;

namespace Example.Server
{
    /// <summary>
    /// Processes connection data from a connected client.
    /// </summary>
    public class ConnectedClient : ISubscriber
    {
        #region Private fields
        private const int POLL_WAIT = 500;
        private const int BUFFER_SIZE = 2048;
        private const int QUEUE_SIZE = 1000;
        private Socket socket;
        private byte[] readBuffer, writeBuffer;
        private Queue<IPacket> queuedPackets;
        private Queue<Msg> queuedMessages;
        #endregion

        /// <summary>
        /// Creates a new ConnectedClient on the specified socket.
        /// </summary>
        /// <param name="socket">A <see cref="Socket"/> object with an open connection to the client.</param>
        /// <param name="clientID">The unique client identifier for this connection.</param>
        public ConnectedClient(Socket socket, int clientID)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            this.ClientID = clientID;
            this.socket = socket;
            this.readBuffer = new byte[BUFFER_SIZE];
            this.writeBuffer = new byte[BUFFER_SIZE];
            this.queuedPackets = new Queue<IPacket>(QUEUE_SIZE);
            this.queuedMessages = new Queue<Msg>(QUEUE_SIZE);
        }

        /// <summary>
        /// Gets the unique client identifier for this connection.
        /// </summary>
        public int ClientID
        {
            get;
            private set;
        }

        /// <summary>
        /// Processes messages on the connected client socket. Blocks until the socket is closed.
        /// </summary>
        public void Process()
        {
            // Subscribe to the client message queue
            MessageBroker.Clients.Subscribe(this);

            while (true)
            {
                try
                {
                    // 1. Check for data to be read
                    if (this.socket.Poll(POLL_WAIT, SelectMode.SelectRead))
                    {
                        int received = this.socket.Receive(readBuffer, BUFFER_SIZE, SocketFlags.None);
                        if (received > 0)
                        {
                            int offset = 0;
                            while (offset < received)
                            {
                                // If we have data, attempt to parse it into a packet
                                IPacket packet;
                                int processed = Framing.ReadPacket(readBuffer, received - offset, offset, out packet);
                                if (packet != null)
                                {
                                    switch (packet.PacketHeader)
                                    {
                                        case Framing.HDR_PACKET:
                                            Debug.WriteLine("Got a message packet with " + ((Packet)packet).Count + " messages");
                                            HandleMessagePacket((Packet)packet);
                                            break;
                                        case Framing.HDR_MONSTER:
                                            Debug.WriteLine("[ConnectedClient.Process] Client shouldn't be sending Monster packets");
                                            break;
                                        default:
                                            Debug.WriteLine("[ConnectedClient.Process] Unhandled packet type: " + packet.PacketHeader);
                                            break;
                                    }
                                }
                                offset += processed;
                            }
                        }
                        else
                        {
                            // client disconnected
                            break;
                        } // if (received > 0...
                    } // if (this.socket.Poll...
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine("[ConnectedClient.Process] Socket error: " + ex.Message + " " + ex.NativeErrorCode);
                    break;
                }

                // 2. Gather up any messages to be sent
                QueueMessagesToPacket();

                // 3. Send data to be written
                if (!SendQueuedPackets())
                {
                    // client disconnected
                    break;
                }
            } // while (true...

            // Unsubscribe from the client message queue when we're done
            MessageBroker.Clients.Unsubscribe(this);
        }

        /// <summary>
        /// Queues a packet to be sent to this client.
        /// </summary>
        /// <param name="packet">An <see cref="IPacket"/> object.</param>
        public void QueuePacket(IPacket packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");
            this.queuedPackets.Enqueue(packet);
        }

        /// <summary>
        /// Iterates through messages received from the connected client.
        /// </summary>
        /// <param name="packet">A <see cref="Packet"/> object containing zero or more messages.</param>
        private void HandleMessagePacket(Packet packet)
        {
            foreach (Msg msg in packet)
            {
                // Sanity check, client-only messages shouldn't be received in this manner
                if (msg.IsServer())
                {
                    MessageBroker.Broadcast(msg);
                }
            }
        }

        /// <summary>
        /// Moves all of the queued messages for this client into a single packet and places
        /// it in the packet queue.
        /// </summary>
        private void QueueMessagesToPacket()
        {
            if (queuedMessages.Count > 0)
            {
                var packet = new Packet();
                while (queuedMessages.Count > 0)
                {
                    packet.Add(queuedMessages.Dequeue());
                }
                this.QueuePacket(packet);
            }
        }

        /// <summary>
        /// Iterates through any queued messages and sends them to the client.
        /// </summary>
        /// <returns><c>True</c> if the end operation succeeded, otherwise <c>false</c>.</returns>
        private bool SendQueuedPackets()
        {
            while (this.queuedPackets.Count > 0)
            {
                // Get a packet and attempt to serialize it to the send buffer
                IPacket packet = this.queuedPackets.Dequeue();
                int written = Framing.WritePacket(this.writeBuffer, BUFFER_SIZE, 0, packet);
                Debug.Assert(written > 0, "[ConnectedClient.SendQueuedPackets] No data in packet");

                // Write the buffer to the socket
                if (this.socket.Poll(1, SelectMode.SelectWrite))
                {
                    int sent = this.socket.Send(this.writeBuffer, written, SocketFlags.None);
                    if (sent == 0)
                    {
                        return false;
                    }
                    Debug.WriteLine(String.Format("[ConnectedClient.SendQueuedPackets] Sent a packet of type {0} with {1} byte(s)", packet.PacketHeader, sent));
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            if (message.IsClient() && (message.client_id == this.ClientID || message.client_id == Msg.ALL_CLIENTS))
            {
                // queues up any messages broadcast to this client so they can be packaged up later
                this.queuedMessages.Enqueue(message);
            }
        }

        #endregion

    }
}
