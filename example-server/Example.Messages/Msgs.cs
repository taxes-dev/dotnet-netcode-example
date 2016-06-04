using System;

namespace Example.Messages
{
    /// <summary>
    /// Container for pre-defined message constants.
    /// </summary>
    public static class Msgs
    {
        #region Meta-constants

        public const ushort CMD_ALL = UInt16.MaxValue;
        public const ushort CMD_NONE = 0;

        #endregion

        #region Client input commands

        public const ushort CMD_INPUT_MOVE = 1000;
        public const ushort SCMD_INPUT_MOVE_SW = 1;
        public const ushort SCMD_INPUT_MOVE_S = 2;
        public const ushort SCMD_INPUT_MOVE_SE = 3;
        public const ushort SCMD_INPUT_MOVE_W = 4;
        public const ushort SCMD_INPUT_MOVE_STOP = 5;
        public const ushort SCMD_INPUT_MOVE_E = 6;
        public const ushort SCMD_INPUT_MOVE_NW = 7;
        public const ushort SCMD_INPUT_MOVE_N = 8;
        public const ushort SCMD_INPUT_MOVE_NE = 9;

        public const ushort CMD_INPUT_ATK = 1100;
        public const ushort SCMD_INPUT_ATK_PRIMARY = 1;
        public const ushort SCMD_INPUT_ATK_SECONDARY = 2;

        #endregion

        #region Console commands

        public const ushort CMD_CONSOLE = 1200;
        public const ushort SCMD_CONSOLE_SYSTEM = 1;
        public const ushort SCMD_CONSOLE_SAY = 2;
        public const ushort SCMD_CONSOLE_BATTLE = 3;

        #endregion

        #region Connection management commands

        public const ushort CMD_CONNECT = 1300;
        public const ushort SCMD_CONNECT_START = 1;
        public const ushort SCMD_CONNECT_KEEPALIVE = 2;
        public const ushort SCMD_CONNECT_END = 3;

        #endregion

        #region Position update commands

        public const ushort CMD_POS = 1400;
        public const ushort SCMD_POS_UPDATE = 1;
        public const ushort SCMD_POS_CORRECTION = 2;

        #endregion

        #region Data synchronization commands

        public const ushort CMD_SYNC = 1500;
        public const ushort SCMD_SYNC_MONSTER = 1;

        #endregion
    }
}
