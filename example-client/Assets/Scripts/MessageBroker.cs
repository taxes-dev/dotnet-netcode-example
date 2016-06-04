using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Example.Messages;

namespace Example.Client
{
    public class MessageBroker : Singleton<MessageBroker>
    {
        #region Private fields
        private const int INITIAL_QUEUE_SIZE = 1000;
        private const int INITIAL_SUBS_SIZE = 100;
        private const int LIMIT_TIME_SLICE = 33; // limits the processing time spent on notifying subscribers (33 ms = 30 fps)
        private Queue<Msg> messageQueue;
        private Dictionary<ushort, List<ISubscriber>> subscribers;
        #endregion

        /// <summary>
        /// Access via Singleton only.
        /// </summary>
        protected MessageBroker()
        {
            this.messageQueue = new Queue<Msg>(INITIAL_QUEUE_SIZE);
            this.subscribers = new Dictionary<ushort, List<ISubscriber>>(INITIAL_SUBS_SIZE);
        }

        /// <summary>
        /// Enqueues a message for processing.
        /// </summary>
        /// <param name="message">A <see cref="Msg"/> value.</param>
        public void Enqueue(Msg message)
        {
            if (this.messageQueue != null)
            {
                this.messageQueue.Enqueue(message);
            }
        }

        /// <summary>
        /// Adds a subscriber to the message broker.
        /// </summary>
        /// <param name="subscriber">An <see cref="ISubscriber"/> object.</param>
        /// <param name="commands">The command(s) that <paramref name="subscriber"/> wants to receive.</param>
        /// <remarks>Be sure to call <see cref="Unsubscribe(ISubscriber)"/> before destroying <paramref name="subscriber"/>.</remarks>
        public void Subscribe(ISubscriber subscriber, params ushort[] commands)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
            if (commands == null)
            {
                throw new ArgumentNullException("commands");
            }
            if (this.subscribers != null)
            {
                for (int i = 0; i < commands.Length; i++)
                {
                    if (this.subscribers.ContainsKey(commands[i]))
                    {
                        this.subscribers[commands[i]].Add(subscriber);
                    }
                    else
                    {
                        var subs = new List<ISubscriber>();
                        subs.Add(subscriber);
                        this.subscribers.Add(commands[i], subs);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a subscriber from the message broker.
        /// </summary>
        /// <param name="subscriber">An <see cref="ISubscriber"/> object previously added via the <see cref="Subscribe(ISubscriber,ushort[])"/> method.</param>
        public void Unsubscribe(ISubscriber subscriber)
        {
            if (this.subscribers != null)
            {
                foreach (List<ISubscriber> subs in this.subscribers.Values)
                {
                    subs.Remove(subscriber);
                }
            }
        }

        #region Unity3D events

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            // TODO: ensure notified objects don't update the processed queue during notifications (could result in infinite queue inflation)
            if (this.messageQueue != null && this.subscribers != null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (this.messageQueue.Count > 0)
                {
                    Msg current = this.messageQueue.Dequeue();

                    // If you're not familiar with LINQ, this is grabbing all of the subscriber lists that match either
                    // the current message's command or the CMD_ALL filter, and then combining them into a single list
                    // of unique subscribers to notify.
                    var notify = (from s in this.subscribers
                                 where s.Key == Msgs.CMD_ALL || s.Key == current.cmd
                                 select s.Value).SelectMany(n => n).Distinct();
                    foreach (ISubscriber subscriber in notify)
                    {
                        subscriber.Handle(current);
                    }

                    if (stopwatch.ElapsedMilliseconds > LIMIT_TIME_SLICE)
                    {
                        // over the limit, leave rest of queue for next frame
                        // TODO: prioritize and drop certain messages
                        UnityEngine.Debug.LogWarning("[MessageBroker] Over single-frame time limit.");
                        break;
                    }
                }
                stopwatch.Stop();
                if (this.messageQueue.Count > 0)
                {
                    UnityEngine.Debug.LogWarning("[MessageBroker] Unable to process all messages in a single frame.");
                }
            }
        }

        /// <summary>
        /// Called when the object is about to be destroyed.
        /// </summary>
        void Destroy()
        {
            if (this.messageQueue != null)
            {
                this.messageQueue.Clear();
                this.messageQueue = null;
            }
            if (this.subscribers != null)
            {
                this.subscribers.Clear();
                this.subscribers = null;
            }
        }

        #endregion
    }
}
