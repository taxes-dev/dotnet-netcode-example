using System;

using Example.GameStructures;
using ProtoBuf;

namespace Example.Messages
{
    /// <summary>
    /// A basic message, client and/or server. This message structure is used
    /// for sending fast updates of small amounts of data or events, and multiple
    /// such messages can be sent in a single packet between the client and server.
    /// For more complex data, specialized packets should be used instead (such as
    /// <see cref="MonsterPacket"/> for sending structured monster data).
    /// </summary>
    /// <seealso cref="Packet"/>
    [ProtoContract]
    public struct Msg
    {
        /// <summary>
        /// No target specified.
        /// </summary>
        /// <see cref="target_id"/>
        public const int NO_TARGET = -1;

        /// <summary>
        /// Indicates a client packet should be broadcast to all clients rather than a specific one.
        /// </summary>
        /// <see cref="client_id"/>
        public const int ALL_CLIENTS = -1;

        /// <summary>
        /// The type of message, client/server/etc.
        /// </summary>
        [ProtoMember(1)]
        public MsgType type;

        /// <summary>
        /// The message command.
        /// </summary>
        [ProtoMember(2)]
        public ushort cmd;

        /// <summary>
        /// The message sub-command.
        /// </summary>
        [ProtoMember(3)]
        public ushort subcmd;

        /// <summary>
        /// If the message is associated with a specific client, this is the ID.
        /// </summary>
        [ProtoMember(4)]
        public int client_id;

        /// <summary>
        /// The source of the message (world object).
        /// </summary>
        [ProtoMember(5)]
        public int source_id;

        /// <summary>
        /// The target of the message (world object).
        /// </summary>
        [ProtoMember(6)]
        public int target_id;

        /// <summary>
        /// Optional data payload for the command.
        /// </summary>
        [ProtoMember(7)]
        public int data;

        /// <summary>
        /// Optional data payload for the command.
        /// </summary>
        [ProtoMember(8)]
        public Vector3D vector;

        /// <summary>
        /// Gets a value indicating whether or not this is a client message.
        /// </summary>
        /// <returns>True if this is a client message; otherwise, false.</returns>
        public bool IsClient()
        {
            return IsClient(this.type);
        }

        /// <summary>
        /// Gets a value indicating whether or not the given <paramref name="msgType"/> is a client message.
        /// </summary>
        /// <param name="msgType">A <see cref="MsgType"/> value.</param>
        /// <returns>True if <paramref name="msgType"/> is a client message; otherwise, false.</returns>
        public static bool IsClient(MsgType msgType)
        {
            return (msgType & MsgType.Client) == MsgType.Client;
        }

        /// <summary>
        /// Gets a value indicating whether or not this is a server message.
        /// </summary>
        /// <returns>True if this is a server message; otherwise, false.</returns>
        public bool IsServer()
        {
            return IsServer(this.type);
        }

        /// <summary>
        /// Gets a value indicating whether or not the given <paramref name="msgType"/> is a server message.
        /// </summary>
        /// <param name="msgType">A <see cref="MsgType"/> value.</param>
        /// <returns>True if <paramref name="msgType"/> is a server message; otherwise, false.</returns>
        public static bool IsServer(MsgType msgType)
        {
            return (msgType & MsgType.Server) == MsgType.Server;
        }

        /// <summary>
        /// Creates a new client message.
        /// </summary>
        /// <param name="cmd">The message command.</param>
        /// <param name="subcmd">The message sub-command.</param>
        /// <returns>A <see cref="Msg"/> value.</returns>
        public static Msg ClientMsg(ushort cmd, ushort subcmd)
        {
            return new Msg() { type = MsgType.Client, cmd = cmd, subcmd = subcmd, source_id = NO_TARGET, target_id = NO_TARGET };
        }

        /// <summary>
        /// Creates a new server message.
        /// </summary>
        /// <param name="cmd">The message command.</param>
        /// <param name="subcmd">The message sub-command.</param>
        /// <returns>A <see cref="Msg"/> value.</returns>
        public static Msg ServerMsg(ushort cmd, ushort subcmd)
        {
            return new Msg() { type = MsgType.Server, cmd = cmd, subcmd = subcmd, source_id = NO_TARGET, target_id = NO_TARGET };
        }

        /// <summary>
        /// Creates a new client/server message.
        /// </summary>
        /// <param name="cmd">The message command.</param>
        /// <param name="subcmd">The message sub-command.</param>
        /// <returns>A <see cref="Msg"/> value.</returns>
        public static Msg SyncMsg(ushort cmd, ushort subcmd)
        {
            return new Msg() { type = MsgType.ClientAndServer, cmd = cmd, subcmd = subcmd, source_id = NO_TARGET, target_id = NO_TARGET };
        }
    }
}
