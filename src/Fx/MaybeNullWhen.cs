#if !NULLABLE_ATTRIBUTES_DISABLE
#nullable enable
#pragma warning disable

namespace System.Diagnostics.CodeAnalysis
{
    using global::System;

#if DEBUG
    /// <summary>
    ///     Specifies that when a method returns <see cref="ReturnValue"/>, 
    ///     the parameter may be <see langword="null"/> even if the corresponding type disallows it.
    /// </summary>
#endif
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
#if !NULLABLE_ATTRIBUTES_INCLUDE_IN_CODE_COVERAGE
    [ExcludeFromCodeCoverage, DebuggerNonUserCode]
#endif
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
#if DEBUG
        /// <summary>
        ///     Gets the return value condition.
        ///     If the method returns this value, the associated parameter may be <see langword="null"/>.
        /// </summary>
#endif
        public bool ReturnValue { get; }

#if DEBUG
        /// <summary>
        ///      Initializes the attribute with the specified return value condition.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition.
        ///     If the method returns this value, the associated parameter may be <see langword="null"/>.
        /// </param>
#endif
        public MaybeNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}

#pragma warning restore
#nullable restore
#endif // NULLABLE_ATTRIBUTES_DISABLE