using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Example.Server.Console
{
    using Console = System.Console;

    /// <summary>
    /// Represents the root console application.
    /// </summary>
    sealed class Program
    {
        #region Private fields
        private bool listenLoop;
        private ManualResetEvent done;
        private ServerProcess serverProcess;
        private int nextClientID;
        #endregion

        /// <summary>
        /// Creates a new instance of the application.
        /// </summary>
        private Program()
        {
            this.done = new ManualResetEvent(false);
            this.listenLoop = true;
            this.nextClientID = -1;
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            try {
                var program = new Program();
                program.ShowBanner();
                program.ProcessArguments(args);
                program.Start();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[FATAL] Uncaught exception.");
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Starts the process of listening for server connections.
        /// </summary>
        private void Start()
        {
            // 1. Set up processor for Ctrl-C and Ctrl-Break to end server.
            Console.CancelKeyPress += Console_CancelKeyPress;

            // 2. Create new server process instance and start it
            this.serverProcess = new ServerProcess();

            Console.WriteLine("Starting server game loop (press Ctrl-C to end) ...");
            this.serverProcess.Start();

            // 3. Set up listening socket to handle incoming client connections
            // If you're not familiar with socket programming, you may want to get a book. This is about as simple
            // an example as I can make.
            var endpoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.IPAddress), Properties.Settings.Default.IPPort);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            socket.Listen(Properties.Settings.Default.MaxConnections);
            Console.WriteLine("Listening on {0}:{1} ...", Properties.Settings.Default.IPAddress, Properties.Settings.Default.IPPort);

            while (listenLoop)
            {
                done.Reset();
                socket.BeginAccept(new AsyncCallback(SocketAccept), socket);
                done.WaitOne();
            }

            Console.WriteLine("Shutting down ...");
            socket.Close();
            this.serverProcess.Stop();
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Handles Ctrl+C and Ctrl+Break.
        /// </summary>
        /// <param name="sender">An <see cref="System.Object"/>.</param>
        /// <param name="e">The data for the event.</param>
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            listenLoop = false;
            Console.WriteLine("Server interrupted!");
            done.Set();
        }

        /// <summary>
        /// Handles new connections on the listen port.
        /// </summary>
        /// <param name="result">An <see cref="IAsyncResult"/> object.</param>
        private void SocketAccept(IAsyncResult result)
        {
            done.Set();

            Socket socket = (Socket)result.AsyncState;
            Socket handler;
            try
            {
                handler = socket.EndAccept(result);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed (probably server shutdown)
                return;
            }

            int port = ((IPEndPoint)handler.LocalEndPoint).Port;
            Console.WriteLine("Received connection on port {0}.", port);

            // Once we actually have a connection, assign it a unique client ID and start
            // a ConnectedClient object to handle communication.
            int clientID = Interlocked.Increment(ref this.nextClientID);
            var client = new ConnectedClient(handler, clientID);
            this.serverProcess.Connect(client);
            client.Process();
            this.serverProcess.Disconnect(client);

            Console.WriteLine("Closing port {0}.", port);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        /// <summary>
        /// Displays identifying information.
        /// </summary>
        private void ShowBanner()
        {
            Console.WriteLine("Example Server Console v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
        }

        /// <summary>
        /// Parses command line arguments and alters state accordingly.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void ProcessArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--serializers")
                {
                    GenerateSerializers();
                }
                else
                {
                    Console.Error.WriteLine("Unkown command line argument: {0}", args[i]);
                    Environment.Exit(1);
                }
            }
        }

        /// <summary>
        /// Generates the serializers needed for protobuf serialization of message types, then exits.
        /// </summary>
        private void GenerateSerializers()
        {
            Console.WriteLine("Compiling serializers ...");

            SerializerGenerator.Generate();

            Console.WriteLine("Done.");
            Environment.Exit(0);
        }
    }
}
