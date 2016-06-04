using System;

using Example.Messages;

namespace Example.Server
{
    /// <summary>
    /// Interface for objects that wish to be notified by the <see cref="MessageBroker"/>.
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// Process the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A <see cref="Msg"/> value.</param>
        /// <remarks>Only messages which were cleared by the <see cref="Interested(MsgType, ushort)"/> method should be passed here.</remarks>
        void Handle(Msg message);
    }
}
