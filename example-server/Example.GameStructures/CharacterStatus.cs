using System;

using ProtoBuf;

namespace Example.GameStructures
{
    [Flags, ProtoContract]
    public enum CharacterStatus
    {
        None = 0,
        Dead = 1
    }
}
