using AlgoLab.Core.Contracts;

namespace AlgoLab.Tests.Core;

public sealed class StructuralComparerTests
{
    [Fact]
    public void Equal_arrays_are_equal()
    {
        Assert.True(StructuralComparer<int[]>.Instance.Equals([1, 2, 3], [1, 2, 3]));
    }

    [Fact]
    public void Different_order_is_not_equal()
    {
        Assert.False(StructuralComparer<int[]>.Instance.Equals([1, 2, 3], [3, 2, 1]));
    }

    [Fact]
    public void Nested_collections_compare_by_content()
    {
        int[][] expected = [[1, 2], [3]];
        int[][] actual = [[1, 2], [3]];
        Assert.True(StructuralComparer<int[][]>.Instance.Equals(expected, actual));
    }

    [Fact]
    public void Nulls_are_equal_to_each_other()
    {
        Assert.True(StructuralComparer<int[]?>.Instance.Equals(null, null));
        Assert.False(StructuralComparer<int[]?>.Instance.Equals([1], null));
    }
}
