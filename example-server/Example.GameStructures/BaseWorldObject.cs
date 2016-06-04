using System;

using ProtoBuf;

namespace Example.GameStructures
{
    /// <summary>
    /// Base class for all instances of server-side world objects.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(BaseCharacter))]
    public abstract class BaseWorldObject
    {
        /// <summary>
        /// A world object with an invalid ID is not yet known to the server.
        /// </summary>
        public const int OBJECT_ID_INVALID = 0;

        /// <summary>
        /// Creates a new <see cref="BaseWorldObject"/>.
        /// </summary>
        public BaseWorldObject()
        {
            this.ObjectID = OBJECT_ID_INVALID;
            this.ObjectType = WorldObjectType.Generic;
            this.IsStatic = false;
        }

        /// <summary>
        /// Gets or sets the object's globally unique instance ID.
        /// </summary>
        [ProtoMember(1)]
        public int ObjectID { get; set; }

        /// <summary>
        /// Gets or sets the object's type.
        /// </summary>
        [ProtoMember(2)]
        public WorldObjectType ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the object's location in world coordinates.
        /// </summary>
        [ProtoMember(3)]
        public Vector3D WorldLoc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this object is static (non-interactive, non-updated).
        /// </summary>
        [ProtoMember(4)]
        public bool IsStatic { get; set; }
    }
}
