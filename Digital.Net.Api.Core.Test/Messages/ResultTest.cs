using System.Collections;
using System.ComponentModel.DataAnnotations;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Messages;

public class ResultTest : UnitTest
{
    [Fact]
    public void AddError_ReturnsResult_WhenException()
    {
        var model = new Result();
        model.AddError(new Exception("Test exception"));
        var error = model.Errors[0];
        Assert.Single((IEnumerable)model.Errors);
        Assert.True(model.HasError);
        Assert.Equal("Test exception", error.Message);
    }

    [Fact]
    public void Merge_ReturnsResultWithNewErrors()
    {
        var model1 = new Result<string>("Test");
        var model2 = new Result<string>("Toast");

        model1.AddError(new Exception("Error 1"));
        model2.AddError(new Exception("Error 2"));
        var test = model1.Merge(model2);

        Assert.Equal(2, model1.Errors.Count);
        Assert.Equal("Test", model1.Value);
        Assert.Collection(
            model1.Errors,
            error => Assert.Equal("Error 1", error.Message),
            error => Assert.Equal("Error 2", error.Message)
        );
    }

    [Fact]
    public void Try_ReturnsResultWithError_WhenExceptionThrown()
    {
        var model = new Result();
        model.Try(() =>
        {
            throw new Exception("Test exception");
            return new Result<string>();
        });
        var error = model.Errors[0];
        Assert.Single(model.Errors);
        Assert.True(model.HasError);
        Assert.Equal("Test exception", error.Message);
    }

    [Fact]
    public void Try_ReturnsResultWithoutError_WhenNoExceptionThrown()
    {
        var model = new Result();
        model.Try(() => new Result<string>());
        Assert.Empty(model.Errors);
        Assert.False(model.HasError);
    }

    [Fact]
    public void TryGeneric_ReturnsResultWithError_WhenExceptionThrown()
    {
        var model = new Result<string>();
        model.Try(() =>
        {
            throw new Exception("Test exception");
            return new Result<string>();
        });
        var error = model.Errors[0];
        Assert.Single(model.Errors);
        Assert.True(model.HasError);
        Assert.Equal("Test exception", error.Message);
    }

    [Fact]
    public void TryGeneric_ReturnsResultWithoutError_WhenNoExceptionThrown()
    {
        var model = new Result<string>();
        model.Try(() => new Result<string>());
        Assert.Empty(model.Errors);
        Assert.False(model.HasError);
    }

    [Fact]
    public void HasError_ReturnsTrue_WhenErrorsExist()
    {
        var model = new Result();
        model.AddError(new Exception("Test exception"));
        Assert.True(model.HasError);
    }

    [Fact]
    public void HasError_WithExceptionParameter_ReturnsTrue_WhenErrorIsFound()
    {
        var model = new Result();
        model.AddError(new AggregateException("Test exception"));
        var result = model.HasErrorOfType<AggregateException>();
        Assert.True(result);
        Assert.False(model.HasErrorOfType<NotImplementedException>());
    }

    private enum TestEnum
    {
        [Display(Name = "Test of very simple case")]
        Test,
        Test2
    }
}