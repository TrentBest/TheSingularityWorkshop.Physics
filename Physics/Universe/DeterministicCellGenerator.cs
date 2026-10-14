namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Stable, stateless seed derivation for procedural initial conditions.
/// This is not a physical solver and does not make evolving state deterministic
/// unless model inputs, event order, and time advancement are also controlled.
/// </summary>
public static class DeterministicCellGenerator
{
    /// <summary>Derives a repeatable 64-bit value from a universe seed and cell address.</summary>
    public static ulong DeriveSeed(ulong universeSeed, int x, int y, int z)
    {
        // Coordinate mixing is independent of cell visitation order and runtime Random implementation.
        var value = universeSeed;
        value = Mix(value ^ unchecked((ulong)(long)x * 0x9E3779B185EBCA87UL));
        value = Mix(value ^ unchecked((ulong)(long)y * 0xC2B2AE3D27D4EB4FUL));
        value = Mix(value ^ unchecked((ulong)(long)z * 0x165667B19E3779F9UL));
        return Mix(value);
    }

    /// <summary>Returns a deterministic value in [0, 1), suitable for seeded fixture generation.</summary>
    public static double DeriveUnitInterval(ulong universeSeed, int x, int y, int z)
    {
        // Use the high 53 bits to create a representable binary64 fraction.
        return (DeriveSeed(universeSeed, x, y, z) >> 11) * (1.0 / 9007199254740992.0);
    }

    private static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
}
