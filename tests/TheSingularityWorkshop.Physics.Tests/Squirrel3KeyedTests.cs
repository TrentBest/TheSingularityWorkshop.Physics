using System.Collections.Concurrent;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Numerics;

[TestFixture]
public sealed class Squirrel3KeyedTests
{
    [Test]
    public void Hash_IsStableForTheSameFourPartKey()
    {
        var first = Squirrel3Keyed.Hash(17u, 42u, 1001u, 3u);
        var second = Squirrel3Keyed.Hash(17u, 42u, 1001u, 3u);
        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Hash_ChangesWhenPurposeChangesForKnownKey()
    {
        var first = Squirrel3Keyed.Hash(17u, 42u, 1001u, 3u);
        var otherPurpose = Squirrel3Keyed.Hash(17u, 42u, 1001u, 4u);
        Assert.That(otherPurpose, Is.Not.EqualTo(first));
    }

    [Test]
    public void Hash_IsIndependentOfEvaluationOrderAndParallelScheduling()
    {
        const uint seed = 123u;
        var keys = Enumerable.Range(0, 20_000)
            .Select(i => (Entity: (uint)(i % 997), Tick: (uint)(i / 997), Purpose: (uint)(i % 7)))
            .ToArray();
        var expected = keys.Select(k => Squirrel3Keyed.Hash(seed, k.Entity, k.Tick, k.Purpose)).ToArray();
        var shuffled = keys.Reverse().ToArray();
        var actual = new ConcurrentDictionary<int, uint>();

        Parallel.For(0, shuffled.Length, i =>
        {
            var key = shuffled[i];
            actual[i] = Squirrel3Keyed.Hash(seed, key.Entity, key.Tick, key.Purpose);
        });

        for (var i = 0; i < shuffled.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[keys.Length - 1 - i]));
    }

    [Test]
    public void UnitDouble_IsAlwaysInsideHalfOpenUnitInterval()
    {
        for (uint i = 0; i < 10_000; i++)
        {
            var value = Squirrel3Keyed.UnitDouble(91u, i, i * 17u, 5u);
            Assert.That(value, Is.GreaterThanOrEqualTo(0d));
            Assert.That(value, Is.LessThan(1d));
        }
    }
}
