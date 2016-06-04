using System;
using System.Collections.Generic;

using Example.Messages;

namespace Example.Server
{
    /// <summary>
    /// The MessageBroker class handles the queueing of messages for the server and any connected clients. Internally, there
    /// are actually two separate queues, one for the server and one for clients.
    /// </summary>
    public class MessageBroker
    {
        #region Private fields
        private const int INITIAL_SIZE = 1000;
        private Queue<Msg> messages;
        private List<ISubscriber> subscribers;
        private static MessageBroker _server = new MessageBroker();
        private static MessageBroker _clients = new MessageBroker();
        #endregion

        private MessageBroker()
        {
            this.messages = new Queue<Msg>(INITIAL_SIZE);
            this.subscribers = new List<ISubscriber>(INITIAL_SIZE);
        }

        /// <summary>
        /// Gets the client <see cref="MessageBroker"/> instance.
        /// </summary>
        public static MessageBroker Clients
        {
            get
            {
                return _clients;
            }
        }

        /// <summary>
        /// Gets the server <see cref="MessageBroker"/> instance.
        /// </summary>
        public static MessageBroker Server
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Shorthand method which allows a message to be queued base on its <see cref="MsgType"/>.
        /// </summary>
        /// <param name="msg">A <see cref="Msg"/> value.</param>
        public static void Broadcast(Msg msg)
        {
            if (msg.IsClient())
            {
                Clients.Enqueue(msg);
            }
            if (msg.IsServer())
            {
                Server.Enqueue(msg);
            }
        }

        /// <summary>
        /// Adds a message to the current queue.
        /// </summary>
        /// <param name="msg">A <see cref="Msg"/> value.</param>
        public void Enqueue(Msg msg)
        {
            this.messages.Enqueue(msg);
        }

        /// <summary>
        /// Delivers message to all subscribers in the queue.
        /// </summary>
        public void Notify()
        {
            Msg[] msgs = new Msg[this.messages.Count];
            this.messages.CopyTo(msgs, 0);
            this.messages.Clear();
            foreach (Msg current in msgs)
            {
                foreach (ISubscriber subscriber in this.subscribers)
                {
                    subscriber.Handle(current);
                }
            }
        }

        /// <summary>
        /// Start notifying <paramref name="subscriber"/> of messages sent to this queue.
        /// </summary>
        /// <param name="subscriber">An <see cref="ISubscriber"/> object.</param>
        public void Subscribe(ISubscriber subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");
            this.subscribers.Add(subscriber);
        }

        /// <summary>
        /// Stop notifying <paramref name="subscriber"/> of messages sent to this queue.
        /// </summary>
        /// <param name="subscriber">An <see cref="ISubscriber"/> object.</param>
        public void Unsubscribe(ISubscriber subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");
            this.subscribers.Remove(subscriber);
        }
    }
}
