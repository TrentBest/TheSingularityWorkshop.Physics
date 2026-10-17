using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Numerics;

[TestFixture]
public sealed class Squirrel3RandomTests
{
    [TestCase(0u, 0u, 436901570u)]
    [TestCase(1u, 0u, 725778245u)]
    [TestCase(0u, 1u, 3732717647u)]
    [TestCase(123456789u, 987654321u, 1708007830u)]
    [TestCase(uint.MaxValue, uint.MaxValue, 2397536069u)]
    public void Hash_MatchesSquirrel3GoldenVectors(uint position, uint seed, uint expected)
    {
        Assert.That(Squirrel3.Hash(position, seed), Is.EqualTo(expected));
    }

    [Test]
    public void Stream_RepeatsForSameSeedAndCallOrder()
    {
        var first = new Squirrel3Random(42u);
        var second = new Squirrel3Random(42u);

        for (var i = 0; i < 1024; i++)
            Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
    }

    [Test]
    public void NextDouble_StaysInsideUnitInterval()
    {
        var random = new Squirrel3Random(2026u);

        for (var i = 0; i < 10_000; i++)
        {
            var value = random.NextDouble();
            Assert.That(value, Is.GreaterThanOrEqualTo(0d));
            Assert.That(value, Is.LessThan(1d));
        }
    }

    [Test]
    public void NextInt_StaysWithinHalfOpenBounds()
    {
        var random = new Squirrel3Random(7u);

        for (var i = 0; i < 10_000; i++)
        {
            var value = random.NextInt(-17, 23);
            Assert.That(value, Is.GreaterThanOrEqualTo(-17));
            Assert.That(value, Is.LessThan(23));
        }
    }

    [Test]
    public void NextInt_SupportsTheFullIntHalfOpenRange()
    {
        var random = new Squirrel3Random(9u);

        Assert.That(random.NextInt(int.MinValue, int.MaxValue),
            Is.InRange(int.MinValue, int.MaxValue - 1));
    }

    [Test]
    public void NextDouble_RejectsInvalidBounds()
    {
        var random = new Squirrel3Random(1u);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(double.NaN, 2d));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(-double.MaxValue, double.MaxValue));
    }

    [Test]
    public void NextInt_RejectsEmptyOrReversedBounds()
    {
        var random = new Squirrel3Random(1u);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(4, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(5, 4));
    }

    [Test]
    public void Chance_HandlesEndpointsWithoutConsumingTheStream()
    {
        var random = new Squirrel3Random(12u);
        var reference = new Squirrel3Random(12u);

        Assert.That(random.Chance(0d), Is.False);
        Assert.That(random.Chance(1d), Is.True);
        Assert.That(random.NextUInt(), Is.EqualTo(reference.NextUInt()));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.Chance(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.Chance(1.01d));
    }

    [Test]
    public void Pick_RejectsEmptyListsAndReturnsAnExistingItem()
    {
        var random = new Squirrel3Random(5u);
        var choices = new[] { "iron", "copper", "stone" };

        Assert.That(choices, Does.Contain(random.Pick(choices)));
        Assert.Throws<ArgumentException>(() => random.Pick(Array.Empty<string>()));
    }

    [Test]
    public void Shuffle_IsRepeatableAndPreservesEveryItem()
    {
        var first = Enumerable.Range(0, 128).ToArray();
        var second = Enumerable.Range(0, 128).ToArray();

        new Squirrel3Random(99u).Shuffle(first.AsSpan());
        new Squirrel3Random(99u).Shuffle(second.AsSpan());

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first.Order(), Is.EqualTo(Enumerable.Range(0, 128)));
    }

    [Test]
    public void Fork_IsRepeatableForAStableStreamId()
    {
        var first = new Squirrel3Random(123u).Fork(4u);
        var second = new Squirrel3Random(123u).Fork(4u);
        var different = new Squirrel3Random(123u).Fork(5u);

        Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
        Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
        Assert.That(first.Seed, Is.Not.EqualTo(different.Seed));
    }
}
