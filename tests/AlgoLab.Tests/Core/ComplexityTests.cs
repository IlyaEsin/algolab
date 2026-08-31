using AlgoLab.Core.Contracts;

namespace AlgoLab.Tests.Core;

public sealed class ComplexityTests
{
    [Theory]
    [InlineData(Complexity.O1, "O(1)")]
    [InlineData(Complexity.ON, "O(n)")]
    [InlineData(Complexity.ONLogN, "O(n log n)")]
    [InlineData(Complexity.ON2, "O(n^2)")]
    public void Display_is_stable(Complexity complexity, string expected)
    {
        Assert.Equal(expected, complexity.Display());
    }

    [Fact]
    public void Grow_is_monotonic_for_polynomial_classes()
    {
        Assert.True(Complexity.ON.Grow(200) > Complexity.ON.Grow(100));
        Assert.True(Complexity.ON2.Grow(200) > Complexity.ON2.Grow(100) * 3);
    }

    [Fact]
    public void Grow_of_constant_class_ignores_n()
    {
        Assert.Equal(Complexity.O1.Grow(10), Complexity.O1.Grow(10_000));
    }

    [Fact]
    public void Grow_stays_finite_for_factorial_at_measurable_sizes()
    {
        Assert.True(double.IsFinite(Complexity.ONFactorial.Grow(20)));
    }
}
