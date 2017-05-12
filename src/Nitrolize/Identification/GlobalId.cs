using System;
using System.Linq;
using System.Reflection;

namespace Nitrolize.Identification
{
    public static class GlobalId
    {
        public static string ToGlobalId(string source, string id)
        {
            return Base64.Encode($"{source}#{id}");
        }

        public static T ToLocalId<T>(string globalId)
        {
            return Decompose<T>(globalId).Id;
        }

        public static object ToLocalId(Type idType, string globalId)
        {
            var method = typeof(GlobalId).GetMethods().Where(m => m.Name == "ToLocalId" && m.IsGenericMethod).First();
            method = method.MakeGenericMethod(idType);
            return method.Invoke(null, new object[] { globalId });
        }

        public static string ToEntityName(string globalId)
        {
            return Decompose(globalId).Item1;
        }

        public static Tuple<string, string> Decompose(string globalId)
        {
            var decoded = Base64.Decode(globalId);
            var splitted = decoded.Split('#');

            return new Tuple<string, string>(splitted[0], splitted[1]);
        }

        public static GlobalIdComponents<T> Decompose<T>(string globalId)
        {
            var stringComponents = Decompose(globalId);

            var stringId = Convert.ToString(stringComponents.Item2);
            var id = GetIdFromString<T>(stringId);

            return new GlobalIdComponents<T>(stringComponents.Item1, id);
        }

        private static T GetIdFromString<T>(string id)
        {
            if (typeof(T) == typeof(Int32))
            {
                return (T)(object)Convert.ToInt32(id);
            }

            if (typeof(T) == typeof(Guid))
            {
                return (T)(object)Guid.Parse(id);
            }

            throw new NotImplementedException($"The type {typeof(T)} as an id type is currently not supported.");
        }

        public class GlobalIdComponents<T>
        {
            public GlobalIdComponents(string entityName, T id)
            {
                this.EntityName = entityName;
                this.Id = id;
            }

            public string EntityName { get; }

            public T Id { get; }
        }
    }
}
