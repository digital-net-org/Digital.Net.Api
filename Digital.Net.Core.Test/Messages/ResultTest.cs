using System.Collections;
using System.ComponentModel.DataAnnotations;
using Digital.Net.Core.Messages;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.Messages;

public class ResultTest : UnitTest
{
    [Test]
    public async Task AddError_ReturnsResult_WhenException()
    {
        var model = new Result();
        model.AddError(new Exception("Test exception"));
        var error = model.Errors[0];
        await Assert.That((IEnumerable)model.Errors).HasSingleItem();
        await Assert.That(model.HasError).IsTrue();
        await Assert.That(error.Message).IsEqualTo("Test exception");
    }

    [Test]
    public async Task Merge_ReturnsResultWithNewErrors()
    {
        var model1 = new Result<string>("Test");
        var model2 = new Result<string>("Toast");

        model1.AddError(new Exception("Error 1"));
        model2.AddError(new Exception("Error 2"));
        var test = model1.Merge(model2);

        await Assert.That(model1.Errors.Count).IsEqualTo(2);
        await Assert.That(model1.Value).IsEqualTo("Test");
        await Assert.That(model1.Errors[0].Message).IsEqualTo("Error 1");
        await Assert.That(model1.Errors[1].Message).IsEqualTo("Error 2");
    }

    [Test]
    public async Task Try_ReturnsResultWithError_WhenExceptionThrown()
    {
        var model = new Result();
        model.Try(() =>
        {
            throw new Exception("Test exception");
            return new Result<string>();
        });
        var error = model.Errors[0];
        await Assert.That(model.Errors).HasSingleItem();
        await Assert.That(model.HasError).IsTrue();
        await Assert.That(error.Message).IsEqualTo("Test exception");
    }

    [Test]
    public async Task Try_ReturnsResultWithoutError_WhenNoExceptionThrown()
    {
        var model = new Result();
        model.Try(() => new Result<string>());
        await Assert.That(model.Errors).IsEmpty();
        await Assert.That(model.HasError).IsFalse();
    }

    [Test]
    public async Task TryGeneric_ReturnsResultWithError_WhenExceptionThrown()
    {
        var model = new Result<string>();
        model.Try(() =>
        {
            throw new Exception("Test exception");
            return new Result<string>();
        });
        var error = model.Errors[0];
        await Assert.That(model.Errors).HasSingleItem();
        await Assert.That(model.HasError).IsTrue();
        await Assert.That(error.Message).IsEqualTo("Test exception");
    }

    [Test]
    public async Task TryGeneric_ReturnsResultWithoutError_WhenNoExceptionThrown()
    {
        var model = new Result<string>();
        model.Try(() => new Result<string>());
        await Assert.That(model.Errors).IsEmpty();
        await Assert.That(model.HasError).IsFalse();
    }

    [Test]
    public async Task HasError_ReturnsTrue_WhenErrorsExist()
    {
        var model = new Result();
        model.AddError(new Exception("Test exception"));
        await Assert.That(model.HasError).IsTrue();
    }

    [Test]
    public async Task HasError_WithExceptionParameter_ReturnsTrue_WhenErrorIsFound()
    {
        var model = new Result();
        model.AddError(new AggregateException("Test exception"));
        var result = model.HasErrorOfType<AggregateException>();
        await Assert.That(result).IsTrue();
        await Assert.That(model.HasErrorOfType<NotImplementedException>()).IsFalse();
    }

    [Test]
    public async Task TryAsync_ReturnsResultWithError_WhenExceptionThrown()
    {
        var model = new Result();
        await model.TryAsync(async () =>
        {
            await Task.Yield();
            throw new Exception("Async error");
            return new Result<string>();
        });
        await Assert.That(model.HasError).IsTrue();
        await Assert.That(model.Errors[0].Message).IsEqualTo("Async error");
    }

    [Test]
    public async Task TryAsync_ReturnsResultWithoutError_WhenNoExceptionThrown()
    {
        var model = new Result();
        await model.TryAsync(async () =>
        {
            await Task.Yield();
            return new Result<string>("ok");
        });
        await Assert.That(model.HasError).IsFalse();
    }

    [Test]
    public async Task ClearErrors_RemovesAllErrors()
    {
        var model = new Result();
        model.AddError(new Exception("Error 1"));
        model.AddError(new Exception("Error 2"));
        await Assert.That(model.HasError).IsTrue();

        model.ClearErrors();
        await Assert.That(model.HasError).IsFalse();
        await Assert.That(model.Errors).IsEmpty();
    }

    [Test]
    public async Task AddInfo_AddsInfoMessage()
    {
        var model = new Result();
        model.AddInfo("Test info");
        await Assert.That(model.Infos).HasSingleItem();
        await Assert.That(model.Infos[0].Message).IsEqualTo("Test info");
    }

    private enum TestEnum
    {
        [Display(Name = "Test of very simple case")]
        Test,
        Test2
    }
}