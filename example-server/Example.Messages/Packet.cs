using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;

namespace Example.Messages
{
    /// <summary>
    /// A Packet is used to transport multiple messages over the wire with time sync information.
    /// </summary>
    [ProtoContract(IgnoreListHandling = true)]
    public class Packet : IEnumerable<Msg>, IPacket
    {
        /// <summary>
        /// The maximum number of Msg structs per Packet (Msg serializes to ~32 bytes). This is aligned to ethernet transmission size to ensure
        /// simple message packets aren't broken up if possible.
        /// </summary>
        public const int MESSAGES_PER_PACKET = 1500 / 32;

        /// <summary>
        /// Creates a new Packet.
        /// </summary>
        public Packet()
            : this(DateTime.Now.ToBinary())
        {
        }

        /// <summary>
        /// Creates a new Packet.
        /// </summary>
        /// <param name="timestamp">The timestamp that the messages were generated.</param>
        public Packet(long timestamp)
        {
            this.Messages = new List<Msg>();
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Adds a message to the packet.
        /// </summary>
        /// <param name="message">A <see cref="Msg"/> value.</param>
        /// <returns>True if the message was added to the Packet; otherwise, false. A false return means that the
        /// packet is full (capacity is at <see cref="MESSAGES_PER_PACKET"/>).</returns>
        public bool Add(Msg message)
        {
            if (this.Messages.Count <= MESSAGES_PER_PACKET)
            {
                this.Messages.Add(message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of messages in the Packet.
        /// </summary>
        public int Count
        {
            get { return this.Messages.Count; }
        }

        /// <summary>
        /// Gets the timestamp that this Packet was assembled.
        /// </summary>
        [ProtoMember(1)]
        public long Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of messages in the packet.
        /// </summary>
        [ProtoMember(2, Options = MemberSerializationOptions.OverwriteList)]
        public List<Msg> Messages
        {
            get;
            set;
        }

        #region IPacket implementation

        public byte PacketHeader
        {
            get
            {
                return Framing.HDR_PACKET;
            }
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        /// Returns an iterator that iterates through the messages in the Packet.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{Msg}"/> object.</returns>
        public IEnumerator<Msg> GetEnumerator()
        {
            return this.Messages.GetEnumerator();
        }

        /// <summary>
        /// Returns an iterator that iterates through the messages in the Packet.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Messages.GetEnumerator();
        }

        #endregion
    }
}
