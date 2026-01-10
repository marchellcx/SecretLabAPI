namespace SecretLabAPI.Utilities
{
    /// <summary>
    /// Represents a method that performs an operation on a value type passed by reference.
    /// </summary>
    /// <remarks>Use this delegate to define operations that need to modify value type instances in place,
    /// such as updating fields or applying transformations. Passing the parameter by reference avoids unnecessary
    /// copying of value types and allows changes to be reflected in the original variable.</remarks>
    /// <typeparam name="T">The type of the value type parameter on which the operation is performed.</typeparam>
    /// <param name="item">A reference to the value type instance to operate on. The method may modify the value of this parameter.</param>
    public delegate void StructAction<T>(ref T item) where T : struct;
}