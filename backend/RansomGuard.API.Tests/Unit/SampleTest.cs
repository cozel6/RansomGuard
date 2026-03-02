using FluentAssertions;
using Xunit;

namespace RansomGuard.API.Tests.Unit;

public class SampleTest
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = 42;

        // Act
        var actual = 40 + 2;

        // Assert
        actual.Should().Be(expected);
    }
}
