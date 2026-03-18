using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MVsToolkit.Favorites.Editor
{
    /// <summary>
    /// A contract resolver that includes private fields in JSON serialization.
    /// This keeps model classes clean from Newtonsoft.Json attributes.
    /// </summary>
    public class PrivateFieldsContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Include private fields
            if (member is not FieldInfo { IsPrivate: true }) return property;
            property.Writable = true;
            property.Readable = true;

            return property;
        }

        protected override List<MemberInfo> GetSerializableMembers(System.Type objectType)
        {
            List<MemberInfo> members = base.GetSerializableMembers(objectType);
            
            // Add private fields
            FieldInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in privateFields)
            {
                if (!members.Contains(field))
                {
                    members.Add(field);
                }
            }

            return members;
        }
    }
}
