using System;

using ProtoBuf;

namespace Example.GameStructures
{
    /// <summary>
    /// Types of objects that can be represented by <see cref="BaseWorldObject"/>.
    /// </summary>
    [ProtoContract]
    public enum WorldObjectType
    {
        /// <summary>
        /// A generic object.
        /// </summary>
        Generic,
        /// <summary>
        /// A non-interactive NPC.
        /// </summary>
        NpcStatic,
        /// <summary>
        /// An interactive NPC.
        /// </summary>
        NpcInteractive,
        /// <summary>
        /// A mobile (AI-controlled) NPC.
        /// </summary>
        Mobile,
        /// <summary>
        /// A player.
        /// </summary>
        Player
    }
}
