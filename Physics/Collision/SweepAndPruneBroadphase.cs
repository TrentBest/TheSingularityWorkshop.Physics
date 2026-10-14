namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>Stable identity and conservative bounds for one broadphase proxy.</summary>
public readonly record struct BroadphaseProxy(long Id, AxisAlignedBounds Bounds);

/// <summary>An unordered candidate pair. Candidates require a more precise contact test.</summary>
public readonly record struct BroadphasePair(long FirstId, long SecondId);

/// <summary>
/// One-dimensional sweep-and-prune screening over conservative 3D AABBs.
/// It avoids comparing pairs whose X intervals cannot overlap, then checks Y and Z.
/// This is a candidate generator, not a collision detector or a complete spatial index.
/// </summary>
public static class SweepAndPruneBroadphase
{
    /// <summary>
    /// Returns every intersecting proxy pair in deterministic ID order.
    /// Duplicate proxy IDs are rejected because IDs define pair identity.
    /// Bounds must conservatively enclose each proxy over the interval being screened.
    /// </summary>
    public static IReadOnlyList<BroadphasePair> FindCandidates(IEnumerable<BroadphaseProxy> proxies)
    {
        ArgumentNullException.ThrowIfNull(proxies);
        var ordered = proxies.OrderBy(proxy => proxy.Bounds.Min.X).ThenBy(proxy => proxy.Id).ToArray();
        var ids = new HashSet<long>();
        foreach (var proxy in ordered)
            if (!ids.Add(proxy.Id))
                throw new ArgumentException($"Duplicate broadphase proxy ID: {proxy.Id}.", nameof(proxies));

        var pairs = new List<BroadphasePair>();
        for (var i = 0; i < ordered.Length; i++)
        {
            var current = ordered[i];
            for (var j = i + 1; j < ordered.Length; j++)
            {
                var next = ordered[j];
                if (next.Bounds.Min.X > current.Bounds.Max.X)
                    break;

                if (!current.Bounds.Intersects(next.Bounds))
                    continue;

                var first = Math.Min(current.Id, next.Id);
                var second = Math.Max(current.Id, next.Id);
                pairs.Add(new BroadphasePair(first, second));
            }
        }

        pairs.Sort(static (a, b) =>
        {
            var first = a.FirstId.CompareTo(b.FirstId);
            return first != 0 ? first : a.SecondId.CompareTo(b.SecondId);
        });
        return pairs;
    }
}
