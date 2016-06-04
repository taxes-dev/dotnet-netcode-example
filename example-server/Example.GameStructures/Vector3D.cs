using System;

using ProtoBuf;

namespace Example.GameStructures
{
    /// <summary>
    /// A 3D world-space coordinate.
    /// </summary>
    [ProtoContract]
    public struct Vector3D
    {
        /// <summary>
        /// Zero vector.
        /// </summary>
        public static Vector3D Zero = new Vector3D(0f, 0f, 0f);

        /// <summary>
        /// The X coordinate.
        /// </summary>
        [ProtoMember(1)]
        public float X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        [ProtoMember(2)]
        public float Y;

        /// <summary>
        /// The Z coordinate.
        /// </summary>
        [ProtoMember(3)]
        public float Z;

        /// <summary>
        /// Creates a new <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Vector3D(float x, float y, float z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return String.Format("({0:0.00},{1:0.00},{2:0.00})", X, Y, Z);
        }

        /// <summary>
        /// Shortcut function to limit the X, Y, Z of a Vector3D to the specified range.
        /// </summary>
        /// <param name="value">A <see cref="Vector3D"/> value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A <see cref="Vector3D"/> value representative of the given <paramref name="value"/> clamped
        /// to the given <paramref name="min"/> and <paramref name="max"/>.</returns>
        public static Vector3D ClampAll(Vector3D value, float min, float max)
        {
            return new Vector3D(
                    value.X.Clamp(min, max),
                    value.Y.Clamp(min, max),
                    value.Z.Clamp(min, max)
                );
        }

        /// <summary>
        /// Add two <see cref="Vector3D"/> values together.
        /// </summary>
        /// <param name="a">A <see cref="Vector3D"/> value.</param>
        /// <param name="b">A <see cref="Vector3D"/> value.</param>
        /// <returns>A <see cref="Vector3D"/> value.</returns>
        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }
}
