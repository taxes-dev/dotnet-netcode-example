using System;

namespace Example.Messages
{
    /// <summary>
    /// Describes a packet that will be sent between the client and server.
    /// </summary>
    public interface IPacket
    {
        /// <summary>
        /// Gets the unique packet identifier for the framing header (see <see cref="Framing"/>).
        /// </summary>
        byte PacketHeader { get; }
    }
}
