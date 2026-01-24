using LabExtended.Core;
using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Actions.API
{
    /// <summary>
    /// Represents a cacheable parameter.
    /// </summary>
    public class CompiledParameter
    {
        /// <summary>
        /// Gets or sets a value indicating whether the parameter is compiled.
        /// </summary>
        public bool IsCompiled { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the source of the parameter.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Retrieves the stored value cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the stored value will be cast and returned.</typeparam>
        /// <returns>The stored value cast to type <typeparamref name="T"/>.</returns>
        /// <exception cref="Exception">Thrown if the parameter has not been compiled.</exception>
        public T GetValue<T>()
        {
            if (!IsCompiled)
                throw new("Parameter is not compiled.");

            return (T)Value;
        }

        /// <summary>
        /// Ensures that the parameter is compiled by assigning its value from the source or a provided default value.
        /// </summary>
        /// <remarks>If the parameter is already compiled, the method returns immediately without
        /// modifying its value. If the source is available, it is used to compile the parameter; otherwise, the default
        /// value is used if provided.</remarks>
        /// <param name="defaultValue">The value to assign if the source is null or empty. If not specified and the source is unavailable, an
        /// exception is thrown.</param>
        /// <returns>true if the parameter was successfully compiled; otherwise, an exception is thrown.</returns>
        /// <exception cref="Exception">Thrown if the source is null or empty and no default value is provided.</exception>
        public bool EnsureCompiled(string? defaultValue = null)
        {
            if (IsCompiled)
                return true;

            if (!string.IsNullOrEmpty(Source))
            {
                Value = Source;

                IsCompiled = true;
                return true;
            }

            if (defaultValue != null)
            {
                Value = defaultValue;

                IsCompiled = true;
                return true;
            }

            throw new($"Parameter could not be compiled from source '{Source}'");
        }

        /// <summary>
        /// Ensures that the parameter value is compiled from the source string using the specified parsing delegate,
        /// optionally assigning a default value if parsing fails.
        /// </summary>
        /// <remarks>If the value has already been compiled, this method returns <see langword="true"/>
        /// without performing any further action. If parsing fails and a non-null default value is provided, the
        /// default value is assigned and the method returns <see langword="true"/>.</remarks>
        /// <typeparam name="T">The type of the value to be parsed and assigned.</typeparam>
        /// <param name="tryParseDelegate">A delegate that attempts to parse the source string into a value of type <typeparamref name="T"/>. Must not
        /// be null.</param>
        /// <param name="defaultValue">The value to assign if parsing the source fails. If not provided, the default value for type <typeparamref
        /// name="T"/> is used.</param>
        /// <returns>true if the value was successfully compiled from the source or assigned from the default value; otherwise,
        /// an exception is thrown.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tryParseDelegate"/> is null.</exception>
        /// <exception cref="Exception">Thrown if parsing the source fails and no default value is provided.</exception>
        public bool EnsureCompiled<T>(TryParseDelegate<T> tryParseDelegate, T defaultValue = default!)
        {
            if (tryParseDelegate is null)
                throw new ArgumentNullException(nameof(tryParseDelegate));

            if (IsCompiled)
                return true;

            if (!string.IsNullOrEmpty(Source) && tryParseDelegate(Source, out T parsedValue))
            {
                Value = parsedValue!;

                IsCompiled = true;
                return true;
            }

            if (defaultValue != null)
            {
                Value = defaultValue;

                IsCompiled = true;
                return true;
            }

            throw new($"Parameter could not be compiled from source '{Source}'");
        }
    }
}