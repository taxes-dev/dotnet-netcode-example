using System;

using ProtoBuf;

namespace Example.Messages
{
    /// <summary>
    /// Describes the handling type for this message.
    /// </summary>
    [Flags, ProtoContract]
    public enum MsgType : byte
    {
        /// <summary>
        /// You forgot to set the message type.
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// This message should be handled only by clients.
        /// </summary>
        Client = 1,
        /// <summary>
        /// This message should be handled only by the server.
        /// </summary>
        Server = 2,
        /// <summary>
        /// This message is intended for both clients and the server.
        /// </summary>
        ClientAndServer = Client | Server
    }
}
