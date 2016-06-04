using System;

using Example.GameStructures;

namespace Example.Messages
{
    /// <summary>
    /// Builds <see cref="Msg"/> values.
    /// </summary>
    public class MsgBuilder
    {
        #region Private fields
        private MsgType msgType;
        private ushort cmd;
        private ushort subcmd;
        private int clientId;
        private int sourceId;
        private int targetId;
        private int data;
        private Vector3D vector;
        #endregion

        /// <summary>
        /// Creates a new <see cref="MsgBuilder"/>.
        /// </summary>
        /// <param name="msgType">A <see cref="MsgType"/> value.</param>
        private MsgBuilder(MsgType msgType)
        {
            this.msgType = msgType;
            this.sourceId = Msg.NO_TARGET;
            this.targetId = Msg.NO_TARGET;
        }

        /// <summary>
        /// Creates a client message builder.
        /// </summary>
        /// <returns>A new <see cref="MsgBuilder"/>.</returns>
        public static MsgBuilder Client()
        {
            return new MsgBuilder(MsgType.Client);
        }

        /// <summary>
        /// Creates a server message builder.
        /// </summary>
        /// <returns>A new <see cref="MsgBuilder"/>.</returns>
        public static MsgBuilder Server()
        {
            return new MsgBuilder(MsgType.Server);
        }

        /// <summary>
        /// Creates a client/server message builder.
        /// </summary>
        /// <returns>A new <see cref="MsgBuilder"/>.</returns>
        public static MsgBuilder Sync()
        {
            return new MsgBuilder(MsgType.ClientAndServer);
        }

        /// <summary>
        /// Sets the command for the message.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        /// <seealso cref="Msgs"/>
        public MsgBuilder Command(ushort cmd)
        {
            this.cmd = cmd;
            return this;
        }

        /// <summary>
        /// Sets the sub-command for the message.
        /// </summary>
        /// <param name="subcmd">The sub-command.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        /// <seealso cref="Msgs"/>
        public MsgBuilder Subcommand(ushort subcmd)
        {
            this.subcmd = subcmd;
            return this;
        }

        /// <summary>
        /// Sets the client ID for the message.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        public MsgBuilder ClientID(int clientId)
        {
            this.clientId = clientId;
            return this;
        }

        /// <summary>
        /// Sets the source game object ID for the message.
        /// </summary>
        /// <param name="sourceId">The source ID.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        public MsgBuilder SourceID(int sourceId)
        {
            this.sourceId = sourceId;
            return this;
        }

        /// <summary>
        /// Sets the target game object ID for the message.
        /// </summary>
        /// <param name="targetId">The target ID.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        public MsgBuilder TargetID(int targetId)
        {
            this.targetId = targetId;
            return this;
        }

        /// <summary>
        /// Sets the data field of the message.
        /// </summary>
        /// <param name="data">The data to set.</param>
        /// <returns>The current <see cref="MsgBuilder"/></returns>
        public MsgBuilder Data(int data)
        {
            this.data = data;
            return this;
        }

        /// <summary>
        /// Puts a vector into the message.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>The current <see cref="MsgBuilder"/>.</returns>
        public MsgBuilder Vector(float x, float y, float z)
        {
            this.vector = new Vector3D(x, y, z);
            return this;
        }

        /// <summary>
        /// Puts a vector into the message.
        /// </summary>
        /// <param name="vector">A <see cref="Vector3D"/> value.</param>
        /// <returns></returns>
        public MsgBuilder Vector(Vector3D vector)
        {
            this.vector = vector;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Msg"/> value based on the data in the builder.
        /// </summary>
        /// <returns>A <see cref="Msg"/> value.</returns>
        public Msg Build()
        {
            return new Msg()
            {
                type = this.msgType,
                cmd = this.cmd,
                subcmd = this.subcmd,
                client_id = this.clientId,
                source_id = this.sourceId,
                target_id = this.targetId,
                data = this.data,
                vector = this.vector
            };
        }
    }
}
