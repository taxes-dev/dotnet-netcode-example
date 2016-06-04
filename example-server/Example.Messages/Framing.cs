using System;
using System.Diagnostics;
using System.IO;

using ProtoBuf;

namespace Example.Messages
{
    /// <summary>
    /// Utility class used to create the raw wire format for sending and receiving data.
    /// </summary>
    /// <remarks>
    /// This part is important because protobufs don't give built-in information about what types they
    /// represnt nor how many bytes they actually contain, so we have to encode that data as a custom
    /// header when sending and receiving packets to/from the client or server.
    /// </remarks>
    public static class Framing
    {
        #region Private fields
        /// <summary>
        /// Arbitrary bytes which let us know we possibly have a valid packet to read.
        /// </summary>
        private static readonly byte[] HDR_PREAMBLE = { 0xfe, 0xdc };
        #endregion

        #region Header constants
        // Each IPacket type must be uniquely identified so we know how to deserialize it
        public const byte HDR_PACKET = 1;
        public const byte HDR_MONSTER = 2;
        #endregion

        /// <summary>
        /// Reads an <see cref="IPacket"/> instance from the given buffer, if possible.
        /// </summary>
        /// <param name="buffer">A buffer.</param>
        /// <param name="size">The number of bytes to read from <paramref name="buffer"/>.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> to begin reading.</param>
        /// <param name="packet">On return, contains an instance of an <paramref name="IPacket"/> object if
        /// deserialization was successful.</param>
        /// <returns>The number of bytes actually consumed in constructing <paramref name="packet"/>.</returns>
        public static int ReadPacket(byte[] buffer, int size, int offset, out IPacket packet)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length - 1)
            {
                throw new ArgumentException("Offset out of range. (Less than zero or past end of buffer.)", "offset");
            }
            if (size < 6 || offset + size > buffer.Length) // 6 = premable + id + size + at least 1 body byte
            {
                throw new ArgumentException("Size out of range. (Less than 6 or past end of buffer.)", "size");
            }

            packet = null;

            // 1. Peek at first bytes to see if we have a valid packet. (bytes 0-1 should match HDR_PREAMBLE)
            int p = offset;
            if (buffer[p] == HDR_PREAMBLE[0] && buffer[p + 1] == HDR_PREAMBLE[1])
            {
                p += 2;

                // 2. Read packet body type (expected deserialize type, either HDR_PACKET or HDR_MONSTER)
                byte packetType = buffer[p];
                p++;

                // 3. Read remaining length of the packet
                byte[] lenbytes = { buffer[p], buffer[p + 1] };
                if (BitConverter.IsLittleEndian)
                {
                    // 3a. We always use network-order in the header, this reverses it on a little-endian platform
                    Array.Reverse(lenbytes);
                }
                short pktlength = BitConverter.ToInt16(lenbytes, 0);
                p += 2;

                // 4. Read rest of the packet via MemoryStream
                Debug.Assert(p + pktlength - 1 < size, "[Framing.ReadPacket] Not enough bytes for the remainder of the packet.");
                using (var stream = new MemoryStream(buffer, p, pktlength))
                {
                    if (packetType == HDR_PACKET)
                    {
                        // 4a. Attempt to deserialize a message packet and return it
                        packet = Serializer.Deserialize<Packet>(stream);
                    }
                    else if (packetType == HDR_MONSTER)
                    {
                        // 4b. Attempt to deserialize a monster packet and return it
                        packet = Serializer.Deserialize<MonsterPacket>(stream);
                    }
                    else
                    {
                        Debug.Assert(false, "[Framing.ReadPacket] Got invalid packet type: " + packetType);
                    }
                }
                p += pktlength;
            }

            // 5. Return how many bytes we actually used to construct the packet
            return p - offset;
        }

        /// <summary>
        /// Writes an <see cref="IPacket"/> instance to the given buffer.
        /// </summary>
        /// <param name="buffer">A buffer.</param>
        /// <param name="size">The maximum number of bytes to attempt to write to <paramref name="buffer"/>.</param>
        /// <param name="offset">The offset within <paramref name="buffer"/> to start writing.</param>
        /// <param name="packet">The <see cref="IPacket"/> object to serialize.</param>
        /// <returns>The actual number of bytes written to <paramref name="buffer"/>, if succesful.</returns>
        public static int WritePacket(byte[] buffer, int size, int offset, IPacket packet)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length - 1)
            {
                throw new ArgumentException("Offset out of range. (Less than zero or past end of buffer.)", "offset");
            }
            if (offset + size > buffer.Length)
            {
                throw new ArgumentException("Size out of range. (Past end of buffer.)", "size");
            }

            int written = 0;
            // 1. Wrap a memory stream around the destination buffer
            using (var stream = new MemoryStream(buffer, offset, size, true))
            {
                // 2. Write the header preamble
                stream.Write(HDR_PREAMBLE, 0, HDR_PREAMBLE.Length);

                // 3. Write the type of packet (either HDR_PACKET or HDR_MONSTER)
                stream.WriteByte(packet.PacketHeader);
                stream.Position += 2; // skip length for now

                // 4. Serialize the packet object into the stream
                int start = (int)stream.Position;
                switch (packet.PacketHeader)
                {
                    case HDR_PACKET:
                        Serializer.Serialize<Packet>(stream, (Packet)packet);
                        break;
                    case HDR_MONSTER:
                        Serializer.Serialize<MonsterPacket>(stream, (MonsterPacket)packet);
                        break;
                }
                written = (int)stream.Position;

                // 5. Backup to the header and write out the packet length into the appropriate place
                short pktlength = (short)(written - start);
                stream.Position = start - 2;
                byte[] lenbytes = BitConverter.GetBytes(pktlength);
                if (BitConverter.IsLittleEndian)
                {
                    // 5a. We always use network-order in the header, this reverses it on a little-endian platform
                    Array.Reverse(lenbytes);
                }
                stream.Write(lenbytes, 0, 2);
            }

            // 6. Return how many bytes we actually wrote into the buffer
            return written;
        }
    }
}
