namespace Lagrange.Proto;

internal static class ProtoConstants
{
    public const int StackallocByteThreshold = 256;
    public const int StackallocCharThreshold = StackallocByteThreshold / 2;
    
    // In the worst case, a single UTF-16 character could be expanded to 3 UTF-8 bytes.
    // Only surrogate pairs expand to 4 UTF-8 bytes but that is a transformation of 2 UTF-16 characters going to 4 UTF-8 bytes (factor of 2).
    // All other UTF-16 characters can be represented by either 1 or 2 UTF-8 bytes.
    public const int MaxExpansionFactorWhileTranscoding = 3;
}