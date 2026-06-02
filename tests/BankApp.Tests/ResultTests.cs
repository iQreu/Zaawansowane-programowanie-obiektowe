using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class ResultTests
{
    [Fact]
    public void Success_CarriesValue()
    {
        var r = Result<int>.Success(42);
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var r = Result<int>.Failure("błąd");
        Assert.False(r.IsSuccess);
        Assert.Equal("błąd", r.Error);
    }
}
