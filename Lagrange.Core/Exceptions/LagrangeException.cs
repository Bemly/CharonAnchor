namespace Lagrange.Core.Exceptions;

/// <summary>
/// The Exception class for Lagrange.Core, All exceptions should be derived from this class.
/// </summary>
public class LagrangeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LagrangeException"/> class.
    /// </summary>
    public LagrangeException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LagrangeException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public LagrangeException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LagrangeException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public LagrangeException(string? message, Exception innerException) : base(message, innerException) { }
}