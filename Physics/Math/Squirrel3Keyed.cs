namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Stateless, keyed Squirrel3 sampling for simulation work that may execute in parallel or be reordered.
/// </summary>
/// <remarks>
/// Each result depends only on the four 32-bit key fields, never on call order or mutable stream state.
/// The key layout is fixed as seed, entity ID, logical tick, and purpose/stream ID. All identifiers must
/// be stable across replay; do not use object hashes, allocation order, wall-clock time, or thread IDs.
/// The output is only 32 bits, so collisions between different keys are inevitable at sufficient scale.
/// This is deterministic pseudorandom variation, not cryptographic randomness.
/// </remarks>
public static class Squirrel3Keyed
{
    private const uint EntityDomain = 0xA511E9B3u;
    private const uint TickDomain = 0x63D83595u;
    private const uint PurposeDomain = 0xB8D5A4F1u;

    /// <summary>Hashes a stable simulation key into a 32-bit sample, independent of evaluation order.</summary>
    /// <param name="seed">Stable experiment/world seed.</param>
    /// <param name="entityId">Stable entity identifier, not an object hash.</param>
    /// <param name="logicalTick">Logical simulation tick, not wall-clock time.</param>
    /// <param name="purposeId">Stable purpose identifier, separating random decisions for the same entity/tick.</param>
    public static uint Hash(uint seed, uint entityId, uint logicalTick, uint purposeId)
    {
        var value = Squirrel3.Hash(entityId, seed ^ EntityDomain);
        value = Squirrel3.Hash(logicalTick, value ^ TickDomain);
        return Squirrel3.Hash(purposeId, value ^ PurposeDomain);
    }

    /// <summary>Returns a keyed sample in [0, 1) with 32 bits of source precision.</summary>
    public static double UnitDouble(uint seed, uint entityId, uint logicalTick, uint purposeId) =>
        Hash(seed, entityId, logicalTick, purposeId) * (1.0 / 4294967296.0);
}
