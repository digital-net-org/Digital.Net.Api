using System.Collections;
using System.ComponentModel.DataAnnotations;
using SafariDigital.Core.Enum;
using SafariDigital.Core.Validation;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Core.Validation;

public class ResultTest : UnitTest
{
    [Fact]
    public void AddError_ShouldHandleException()
    {
        // Arrange
        var model = new Result();
        var exception = new Exception("Test exception");

        // Act
        model.AddError(exception);
        var error = model.Errors[0];

        // Assert
        Assert.Single((IEnumerable)model.Errors);
        Assert.True(model.HasError);
        Assert.Equal("Test exception", error.Message);
    }

    [Fact]
    public void AddError_ShouldHandleError()
    {
        // Arrange
        var model = new Result();
        const TestEnum errorType = TestEnum.Test;

        // Act
        model.AddError(errorType);
        var error = model.Errors[0];

        // Assert
        Assert.Single((IEnumerable)model.Errors);
        Assert.True(model.HasError);
        Assert.False(model.HasWarning);
        Assert.Equal(errorType.GetDisplayName(), error.Message);
        Assert.Equal(errorType, error.Code);
    }

    [Fact]
    public void AddWarning_ShouldHandleWarning()
    {
        // Arrange
        var model = new Result();
        const TestEnum warningType = TestEnum.Test;

        // Act
        model.AddWarning(warningType);
        var warning = model.Warnings[0];

        // Assert
        Assert.Single(model.Warnings);
        Assert.False(model.HasError);
        Assert.True(model.HasWarning);
        Assert.Equal(warningType.GetDisplayName(), warning.Message);
        Assert.Equal(warningType, warning.Code);
    }

    [Fact]
    public void AddInfo_ShouldHandleInfo()
    {
        // Arrange
        var model = new Result();
        const TestEnum infoType = TestEnum.Test;

        // Act
        model.AddInfo(infoType);
        var info = model.Infos[0];

        // Assert
        Assert.Single(model.Infos);
        Assert.False(model.HasError);
        Assert.False(model.HasWarning);
        Assert.Equal(infoType.GetDisplayName(), info.Message);
        Assert.Equal(infoType, info.Code);
    }

    [Fact]
    public void Merge_ShouldMergeMessages()
    {
        // Arrange
        var model1 = new Result<string>("Test");
        var model2 = new Result<string>("Toast");

        // Act
        model1.AddError(new Exception("Error 1"));
        model2.AddError(new Exception("Error 2"));
        model2.AddWarning(TestEnum.Test);
        var test = model1.Merge(model2);

        // Assert
        Assert.Equal(2, model1.Errors.Count);
        Assert.Single(model1.Warnings);
        Assert.Equal("Test", model1.Value);
        Assert.Collection(
            model1.Errors,
            error => Assert.Equal("Error 1", error.Message),
            error => Assert.Equal("Error 2", error.Message)
        );
    }

    [Fact]
    public void ValidateExpression_ShouldHandleMessages()
    {
        // Arrange
        var model = new Result();

        // Act
        var result = model.ValidateExpression(Expression());

        // Assert
        Assert.Equal(5, result);
        Assert.True(model.HasError);
        Assert.Single((IEnumerable)model.Errors);
        Assert.Equal(TestEnum.Test, model.Errors[0].Code);

        return;

        // Arrange (Again)
        Result<int> Expression()
        {
            var subModel = new Result<int>(5);
            subModel.AddError(TestEnum.Test);
            return subModel;
        }
    }

    [Fact]
    public async Task ValidateExpressionAsync_ShouldHandleMessages()
    {
        // Arrange
        var model = new Result();

        // Act
        var result = await model.ValidateExpressionAsync(Expression());

        // Assert
        Assert.Equal(5, result);
        Assert.True(model.HasError);
        Assert.Single((IEnumerable)model.Errors);
        Assert.Equal(TestEnum.Test, model.Errors[0].Code);

        return;

        // Arrange (Again)
        async Task<Result<int>> Expression()
        {
            var subModel = new Result<int>(5);
            subModel.AddError(TestEnum.Test);
            await Task.Delay(10); // Simulate async operation
            return subModel;
        }
    }

    private enum TestEnum
    {
        [Display(Name = "Test of very simple case")]
        Test,
        Test2
    }
}