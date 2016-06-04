using System;

using ProtoBuf;

namespace Example.GameStructures
{
    /// <summary>
    /// Base class for character-type world objects.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(Npc.Monster))]
    public abstract class BaseCharacter : BaseWorldObject
    {
        /// <summary>
        /// Gets or sets the character's state.
        /// </summary>
        [ProtoMember(1)]
        public CharacterStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the character's level.
        /// </summary>
        [ProtoMember(2)]
        public byte Level { get; set; }

        /// <summary>
        /// Gets or sets the character's hit points.
        /// </summary>
        [ProtoMember(3)]
        public uint HP { get; set; }

        /// <summary>
        /// Gets or sets the direction the character is facing (rotation).
        /// </summary>
        [ProtoMember(4)]
        public Vector3D Facing { get; set; }

        /// <summary>
        /// Gets or sets the ID of the faction this character belongs to.
        /// </summary>
        /// <seealso cref="Factions"/>
        [ProtoMember(5)]
        public int FactionID { get; set; }
    }
}
