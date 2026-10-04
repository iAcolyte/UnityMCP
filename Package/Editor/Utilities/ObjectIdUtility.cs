using UnityEditor;

namespace UnityMCP.Editor.Utilities
{
    /// <summary>
    /// Version-independent access to Unity object identifiers.
    /// </summary>
    /// <remarks>
    /// Unity 6.4 (6000.4) introduced EntityId.ToULong/FromULong and later turned the int-based
    /// GetInstanceID/InstanceIDToObject APIs into compile errors. IDs are exposed as long so the
    /// raw EntityId bits survive a JSON round-trip; on older versions the values are the same
    /// int instance IDs as before.
    /// </remarks>
    public static class ObjectIdUtility
    {
        /// <summary>
        /// Gets the identifier of a Unity object.
        /// </summary>
        /// <param name="obj">The object to identify.</param>
        /// <returns>The object's identifier, or 0 if the object is null.</returns>
        public static long GetId(UnityEngine.Object obj)
        {
            if (obj is null)
            {
                return 0;
            }
#if UNITY_6000_4_OR_NEWER
            return unchecked((long)UnityEngine.EntityId.ToULong(obj.GetEntityId()));
#else
            return obj.GetInstanceID();
#endif
        }

        /// <summary>
        /// Resolves a Unity object from an identifier returned by <see cref="GetId"/>.
        /// </summary>
        /// <param name="id">The object identifier.</param>
        /// <returns>The resolved object, or null if no object has this identifier.</returns>
        public static UnityEngine.Object FromId(long id)
        {
#if UNITY_6000_4_OR_NEWER
            return EditorUtility.EntityIdToObject(UnityEngine.EntityId.FromULong(unchecked((ulong)id)));
#else
            if (id < int.MinValue || id > int.MaxValue)
            {
                return null;
            }
#pragma warning disable CS0618 // InstanceIDToObject is deprecated in Unity 6 but still functional
            return EditorUtility.InstanceIDToObject((int)id);
#pragma warning restore CS0618
#endif
        }

        /// <summary>
        /// Converts a boxed identifier read via reflection (int instance ID or EntityId) to an object identifier.
        /// </summary>
        /// <param name="value">The boxed identifier value.</param>
        /// <returns>The object identifier, or 0 if the value is not a recognized identifier.</returns>
        public static long FromBoxedValue(object value)
        {
            switch (value)
            {
                case int instanceId:
                    return instanceId;
#if UNITY_6000_4_OR_NEWER
                case UnityEngine.EntityId entityId:
                    return unchecked((long)UnityEngine.EntityId.ToULong(entityId));
#endif
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Checks whether an object reference property stores a non-empty reference,
        /// even if the referenced object itself is missing.
        /// </summary>
        /// <param name="property">An ObjectReference serialized property.</param>
        /// <returns>True if the stored reference is not empty.</returns>
        public static bool HasReference(SerializedProperty property)
        {
#if UNITY_6000_4_OR_NEWER
            return property.objectReferenceEntityIdValue != UnityEngine.EntityId.None;
#else
            return property.objectReferenceInstanceIDValue != 0;
#endif
        }
    }
}
