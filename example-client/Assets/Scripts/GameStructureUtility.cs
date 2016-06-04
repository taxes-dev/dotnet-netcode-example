using UnityEngine;

using Example.GameStructures;

namespace Example.Client
{
    /// <summary>
    /// Extension methods for working with game structures.
    /// </summary>
    public static class GameStructureUtility
    {
        /// <summary>
        /// Converts the server-based Vector3D object into a Unity Vector3.
        /// </summary>
        /// <param name="vector3d">A <see cref="Vector3D"/> value.</param>
        /// <returns>A <see cref="Vector3"/> value.</returns>
        public static Vector3 ToVector3(this Vector3D vector3d)
        {
            return new Vector3(vector3d.X, vector3d.Y, vector3d.Z);
        }

        /// <summary>
        /// Converts a Unity Vector3 into the server-based Vector3D object.
        /// </summary>
        /// <param name="vector3">A <see cref="Vector3"/> value.</param>
        /// <returns>A <see cref="Vector3D"/> value.</returns>
        public static Vector3D ToVector3D(this Vector3 vector3)
        {
            return new Vector3D(vector3.x, vector3.y, vector3.z);
        }
    }
}
