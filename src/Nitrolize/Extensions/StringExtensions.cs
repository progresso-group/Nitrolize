namespace Nitrolize.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string by lowering it's first character.
        /// I.e.: "SomeString" => "someString"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToFirstLower(this string input)
        {
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// Converts a generic type definition name to a cleaner readable name.
        /// I.e.: "List`1" => List
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ToCleanGenericTypeDefinitionName(this string name)
        {
            return name.Remove(name.IndexOf('`'));
        }
    }
}
