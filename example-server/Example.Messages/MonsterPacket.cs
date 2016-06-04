using System;

using ProtoBuf;
using Example.GameStructures.Npc;

namespace Example.Messages
{
    /// <summary>
    /// The MonsterPacket transports a set of data about a monster NPC.
    /// </summary>
    [ProtoContract]
    public class MonsterPacket : IPacket
    {
        /// <summary>
        /// Creates a new MonsterPacket.
        /// </summary>
        public MonsterPacket()
            : this(DateTime.Now.ToBinary())
        {
        }

        /// <summary>
        /// Creates a new MonsterPacket.
        /// </summary>
        /// <param name="timestamp">The timestamp that the messages were generated.</param>
        public MonsterPacket(long timestamp)
        {
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Gets or sets the timestamp that this MonsterPacket was assembled.
        /// </summary>
        [ProtoMember(1)]
        public long Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Monster"/> for the MonsterPacket.
        /// </summary>
        [ProtoMember(2)]
        public Monster MonsterInstance
        {
            get;
            set;
        }

        public byte PacketHeader
        {
            get
            {
                return Framing.HDR_MONSTER;
            }
        }
    }
}
