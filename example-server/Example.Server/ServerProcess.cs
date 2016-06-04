using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Example.GameStructures;
using Example.GameStructures.Npc;
using Example.Messages;

namespace Example.Server
{
    /// <summary>
    /// This is the main server process which tracks the state of the world and handles shuttling state between the clients.
    /// </summary>
    /// <remarks>
    /// In this example, the server process is represented by a single monolithic class. If you extend this, you should
    /// consider breaking it up into smaller pieces to handle the different tasks of running the world state. In particular,
    /// handling messages should likely be broken up into smaller chunks.
    /// </remarks>
    public class ServerProcess : ISubscriber
    {
        #region Private fields
        private const int SERVER_FRAME_INTERVAL = 100; // ~10 fps in server processing

        private Dictionary<int, Monster> monsters;
        private Dictionary<int, ConnectedClient> clients;
        private object sync;
        private Thread serverThread;
        private Random random;
        private long updateTracker = 0L;
        #endregion

        public ServerProcess()
        {
            this.monsters = new Dictionary<int, Monster>();
            this.clients = new Dictionary<int, ConnectedClient>();
            this.sync = new object();
            this.random = new Random();
        }

        /// <summary>
        /// Starts the server process on a new thread.
        /// </summary>
        public void Start()
        {
            if (this.serverThread == null)
            {
                this.serverThread = new Thread(new ThreadStart(StartThread));
                this.serverThread.Start();
            }
        }

        /// <summary>
        /// Thread entry point for the server process.
        /// </summary>
        private void StartThread()
        { 
            // Initial environment setup
            Init();
            var timer = Stopwatch.StartNew();
            long lastFrame = 0L;

            while (true)
            {
                try
                {
                    long elapsed = timer.ElapsedMilliseconds - lastFrame;
                    lastFrame = timer.ElapsedMilliseconds;

                    // 2. Update world objects
                    Update(elapsed);

                    // 3. Notify the server of any waiting messages
                    MessageBroker.Server.Notify();

                    // 4. Notify the client(s) of any waiting messages
                    MessageBroker.Clients.Notify();

                    // 5. Manage server frame rate
                    int toSleep = SERVER_FRAME_INTERVAL - (int)elapsed;
                    if (toSleep > 0)
                    {
                        Thread.Sleep(toSleep);
                    }
                    else if (elapsed > SERVER_FRAME_INTERVAL)
                    {
                        Console.Error.WriteLine("[Server Loop Warning] Server frame taking too long: {0}ms", elapsed);
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[Server Loop Error] " + ex.ToString());
                }
            }

            // Unsubscribe from the message queue
            MessageBroker.Server.Unsubscribe(this);
            this.serverThread = null;
        }

        /// <summary>
        /// Stops the current server process, if there is one.
        /// </summary>
        public void Stop()
        {
            if (this.serverThread != null)
            {
                this.serverThread.Abort();
            }
        }

        /// <summary>
        /// Assign a new conneced client to the server.
        /// </summary>
        /// <param name="client">A <see cref="ConnectedClient"/> object.</param>
        /// <remarks>
        /// This also ensures the <paramref name="client"/> receives a fresh copy of the world state.
        /// </remarks>
        public void Connect(ConnectedClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            lock (sync)
            {

                if (!this.clients.ContainsKey(client.ClientID))
                    this.clients.Add(client.ClientID, client);
            }
            Sync(client);
        }

        /// <summary>
        /// Removes a previously connected client from the server.
        /// </summary>
        /// <param name="client">A <see cref="ConnectedClient"/> object.</param>
        public void Disconnect(ConnectedClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            lock (sync)
            {
                if (this.clients.ContainsKey(client.ClientID))
                    this.clients.Remove(client.ClientID);
            }
        }

        /// <summary>
        /// Re-sends the current world data to a client.
        /// </summary>
        /// <param name="client">An active <see cref="ConnectedClient"/> object.</param>
        protected void Sync(ConnectedClient client)
        {
            foreach (Monster monster in this.monsters.Values) {
                Msg syncMsg = MsgBuilder.Server()
                    .Command(Msgs.CMD_SYNC)
                    .Subcommand(Msgs.SCMD_SYNC_MONSTER)
                    .ClientID(client.ClientID)
                    .Data(monster.ObjectID)
                    .Build();
                MessageBroker.Broadcast(syncMsg);
            }
            // TODO: info about other clients should be sent as well
        }

        /// <summary>
        /// Performs initial setup of the server environment.
        /// </summary>
        /// <remarks>
        /// This would be where world data would be read in from an external source and setup for the first time
        /// in preparation for clients to start connecting.
        /// </remarks>
        private void Init()
        {
            // Add the server to the message queue
            MessageBroker.Server.Subscribe(this);

            // DEBUG: create dummy monsters
            for (int i = 0; i < 10; i++)
            {
                var monster = new Monster() { MonsterTemplateID = 1, HP = 10, Level = 1, WorldLoc = new Vector3D(-10.0f + i, 1.75f, 15.0f), Facing = new Vector3D(1.0f, 0f, 0f) };
                WorldUtility.NextObjectID(monster);
                this.monsters.Add(monster.ObjectID, monster);
            }
        }

        /// <summary>
        /// Updates world objects.
        /// </summary>
        /// <param name="elapsed">The number of milliseconds since the previous update.</param>
        private void Update(long elapsed)
        {
            // Note these are based on the characteristics of "Test Ground" in the Unity client
            const float MIN_X = -29.5f, MAX_X = -0.5f;
            const float MIN_Z = 0.5f, MAX_Z = 29.5f;

            // DEBUG: We'll make the monsters dance about for fun every few seconds
            updateTracker += elapsed;
            if (updateTracker > 4000L)
            {
                updateTracker = 0L;
                foreach (Monster monster in this.monsters.Values)
                {
                    float newX = random.Next(-1, 2);
                    float newZ = random.Next(-1, 2);
                    monster.WorldLoc = new Vector3D(
                            (monster.WorldLoc.X + newX).Clamp(MIN_X, MAX_X),
                            0f,
                            (monster.WorldLoc.Z + newZ).Clamp(MIN_Z, MAX_Z)
                        );
                    Msg upd = MsgBuilder.Client()
                        .ClientID(Msg.ALL_CLIENTS)
                        .Command(Msgs.CMD_POS)
                        .Subcommand(Msgs.SCMD_POS_UPDATE)
                        .TargetID(monster.ObjectID)
                        .Vector(monster.WorldLoc)
                        .Build();
                    MessageBroker.Broadcast(upd);
                }
            }
        }

        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            if (message.cmd == Msgs.CMD_SYNC && message.subcmd == Msgs.SCMD_SYNC_MONSTER)
                Message_ResyncClient(message);
            else if (message.cmd == Msgs.CMD_POS)
                Message_HandlePositionUpdate(message);
            // Additional message handlers could go here
        }

        #endregion

        #region Message handlers

        /// <summary>
        /// [CMD_SYNC / SCMD_SYNC_MONSTER] Synchronizes a specific monster to a specific client.
        /// </summary>
        /// <param name="message">A <see cref="Msg"/> value.</param>
        private void Message_ResyncClient(Msg message)
        {
            ConnectedClient client = this.clients[message.client_id];
            Monster monster = this.monsters[message.data];
            if (client != null && monster != null)
            {
                MonsterPacket packet = new MonsterPacket();
                packet.MonsterInstance = monster;
                client.QueuePacket(packet);
            }
        }

        /// <summary>
        /// [CMD_POS] Handles positional messages received from clients.
        /// </summary>
        /// <param name="message"></param>
        private void Message_HandlePositionUpdate(Msg message)
        {
            if (message.subcmd == Msgs.SCMD_POS_UPDATE)
            { 
                Debug.WriteLine(String.Format("[ServerProcess.Message_HandlePositionUpdate] Received position update from client {0} ({1:0.00}, {2:0.00}, {3:0.00})",
                    message.client_id, message.vector.X, message.vector.Y, message.vector.Z));
            }
        }

        #endregion
    }
}
