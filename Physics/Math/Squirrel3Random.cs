namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Squirrel3's 32-bit integer noise hash, exposed as a deterministic random-access primitive.
/// </summary>
/// <remarks>
/// The result is a stable function of <paramref name="position"/> and <paramref name="seed"/>.
/// Unsigned overflow is intentional and part of the algorithm. This is not a cryptographic generator.
/// </remarks>
public static class Squirrel3
{
    private const uint BitNoise1 = 0xB5297A4Du;
    private const uint BitNoise2 = 0x68E31DA4u;
    private const uint BitNoise3 = 0x1B56C4E9u;

    /// <summary>Hashes a 32-bit position with a 32-bit seed into a deterministic 32-bit value.</summary>
    public static uint Hash(uint position, uint seed)
    {
        unchecked
        {
            position *= BitNoise1;
            position += seed;
            position ^= position >> 8;
            position += BitNoise2;
            position ^= position << 8;
            position *= BitNoise3;
            position ^= position >> 8;
            return position;
        }
    }
}

/// <summary>
/// A small deterministic Squirrel3-backed random stream with convenience methods for simulation inputs.
/// </summary>
/// <remarks>
/// The stream is deterministic for a given seed and call order. Use <see cref="Squirrel3.Hash"/> when
/// random access by stable entity/tick index is needed. This class is not thread-safe and is not cryptographic.
/// </remarks>
public sealed class Squirrel3Random
{
    private uint _position;

    /// <summary>Creates a stream from a reproducible 32-bit seed.</summary>
    public Squirrel3Random(uint seed)
    {
        Seed = seed;
    }

    /// <summary>The immutable seed for this stream.</summary>
    public uint Seed { get; }

    /// <summary>Gets the next unsigned 32-bit value.</summary>
    public uint NextUInt()
    {
        var position = _position;
        _position = unchecked(_position + 1u);
        return Squirrel3.Hash(position, Seed);
    }

    /// <summary>Returns a value in [0, 1), with 32 bits of source precision.</summary>
    public double NextDouble() => NextUInt() * (1.0 / 4294967296.0);

    /// <summary>Returns a finite value in [minimumInclusive, maximumExclusive).</summary>
    public double NextDouble(double minimumInclusive, double maximumExclusive)
    {
        if (!double.IsFinite(minimumInclusive) || !double.IsFinite(maximumExclusive) ||
            maximumExclusive <= minimumInclusive ||
            !double.IsFinite(maximumExclusive - minimumInclusive))
        {
            throw new ArgumentOutOfRangeException(nameof(maximumExclusive),
                "Bounds must be finite, increasing, and have a finite difference.");
        }

        return minimumInclusive + (maximumExclusive - minimumInclusive) * NextDouble();
    }

    /// <summary>Returns an integer in [minimumInclusive, maximumExclusive).</summary>
    public int NextInt(int minimumInclusive, int maximumExclusive)
    {
        if (maximumExclusive <= minimumInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumExclusive),
                "The exclusive upper bound must exceed the inclusive lower bound.");
        }

        var range = (uint)((long)maximumExclusive - minimumInclusive);
        return (int)((long)minimumInclusive + NextUIntBounded(range));
    }

    /// <summary>Returns a reproducible Boolean with equal probability in the generated 32-bit domain.</summary>
    public bool NextBool() => (NextUInt() & 1u) != 0;

    /// <summary>Returns true with the requested probability, which must be in [0, 1].</summary>
    public bool Chance(double probability)
    {
        if (!double.IsFinite(probability) || probability < 0d || probability > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be finite and in [0, 1].");
        }

        if (probability <= 0d)
            return false;
        if (probability >= 1d)
            return true;

        return NextDouble() < probability;
    }

    /// <summary>Chooses one item from a non-empty list.</summary>
    public T Pick<T>(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(items));

        return items[NextInt(0, items.Count)];
    }

    /// <summary>Shuffles a span in place using Fisher-Yates.</summary>
    public void Shuffle<T>(Span<T> items)
    {
        for (var i = items.Length - 1; i > 0; i--)
        {
            var j = NextInt(0, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    /// <summary>Creates a repeatable independent stream identified by a stable stream ID.</summary>
    public Squirrel3Random Fork(uint streamId) => new(Squirrel3.Hash(streamId, Seed));

    private uint NextUIntBounded(uint exclusiveUpperBound)
    {
        // Rejection sampling avoids modulo bias for integer ranges.
        var threshold = unchecked(0u - exclusiveUpperBound) % exclusiveUpperBound;
        while (true)
        {
            var value = NextUInt();
            if (value >= threshold)
                return value % exclusiveUpperBound;
        }
    }
}
