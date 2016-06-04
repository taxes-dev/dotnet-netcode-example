using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Interface for objects that wish to be notified by the <see cref="MessageBroker"/>.
    /// </summary>
    /// <remarks>A subscriber must add itself to the broker by calling the <see cref="MessageBroker.Subscribe(ISubscriber, ushort[])"/>
    /// method in order to receive notifications.</remarks>
    public interface ISubscriber
    {
        /// <summary>
        /// Process the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A <see cref="Msg"/> value.</param>
        void Handle(Msg message);
    }
}
