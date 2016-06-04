using System;
using System.Threading;

using Example.GameStructures;

namespace Example.Server
{
    /// <summary>
    /// Helper utilities for world object.
    /// </summary>
    internal static class WorldUtility
    {
        #region Private fields
        private static int lastObjID = 0;
        #endregion

        /// <summary>
        /// Populates the given <see cref="BaseWorldObject"/> with the next available global ID.
        /// </summary>
        /// <param name="obj">A <see cref="BaseWorldObject"/>. On return, its <see cref="BaseWorldObject.ObjectID"/> will
        /// be set to a valid identifier if it wasn't already.</param>
        /// <remarks>All instantiated objects in the world have a sequential identifier. This helper method
        /// just manages the assignment of these identifiers in a thread-safe way.</remarks>
        public static void NextObjectID(BaseWorldObject obj)
        {
            // The idea behind this constant is to give a window where we need to restart at 0. Theoretically
            // by the point you reach almost 2 billion world objects instantiated, the ones at the bottom are
            // long gone, but you may need to adjust to your needs.
            const int ROLLOVER = Int32.MaxValue - 100000;

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (obj.ObjectID == BaseWorldObject.OBJECT_ID_INVALID)
            {
                obj.ObjectID = Interlocked.Increment(ref lastObjID);
                if (lastObjID > ROLLOVER)
                {
                    Interlocked.Exchange(ref lastObjID, 0);
                }
            }
        }
    }
}
