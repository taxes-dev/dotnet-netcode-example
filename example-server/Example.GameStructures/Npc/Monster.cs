using System;

using ProtoBuf;

namespace Example.GameStructures.Npc
{
    /// <summary>
    /// An instance of a monster (basic hostile NPC) character.
    /// </summary>
    [ProtoContract]
    public class Monster : BaseCharacter
    {
        /// <summary>
        /// Creates a new <seealso cref="Monster"/>.
        /// </summary>
        public Monster() {
            this.FactionID = Factions.GENERIC_HOSTILE;
            this.ObjectType = WorldObjectType.Mobile;
        }

        /// <summary>
        /// Gets or sets the template ID for the monster. Used for correlating the monster with its statistics (server) and models (client).
        /// </summary>
        [ProtoMember(1)]
        public int MonsterTemplateID { get; set; }
    }
}
