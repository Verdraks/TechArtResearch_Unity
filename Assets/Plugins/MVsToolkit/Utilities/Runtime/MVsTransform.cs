using UnityEngine;

namespace MVsToolkit.Utilities
{
    public static class MVsTransform
    {
        /// <summary>
        /// Sets the X component of the Transform's position while keeping Y and Z unchanged.
        /// This is equivalent to assigning a new Vector3 with the updated X value.
        /// Returns the Transform to allow method chaining.
        /// </summary>
        public static Transform PosX(this Transform t, float value)
        {
            // Replace only the X component (creates a new Vector3)
            t.position = new Vector3(value, t.position.y, t.position.z);
            return t; // Enables chaining like t.PosX(3).PosY(5)
        }

        /// <summary>
        /// Sets the Y component of the Transform's position while keeping X and Z unchanged.
        /// This is equivalent to assigning a new Vector3 with the updated Y value.
        /// Returns the Transform to allow method chaining.
        /// </summary>
        public static Transform PosY(this Transform t, float value)
        {
            // Replace only the Y component (creates a new Vector3)
            t.position = new Vector3(t.position.x, value, t.position.z);
            return t;
        }

        /// <summary>
        /// Sets the Z component of the Transform's position while keeping X and Y unchanged.
        /// This is equivalent to assigning a new Vector3 with the updated Z value.
        /// Returns the Transform to allow method chaining.
        /// </summary>
        public static Transform PosZ(this Transform t, float value)
        {
            // Replace only the Z component (creates a new Vector3)
            t.position = new Vector3(t.position.x, t.position.y, value);
            return t;
        }
    }
}