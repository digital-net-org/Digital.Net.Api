using System;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Lib.Random;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestFormDataFactory
{
    public static Form BuildTestForm(
        this CmsContext context,
        string? name = null,
        string? description = null,
        bool published = false,
        string submitLabel = "Submit"
    )
    {
        var form = new Form
        {
            Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            Description = description,
            Published = published,
            SubmitLabel = submitLabel
        };
        context.Forms.Add(form);
        context.SaveChanges();
        return form;
    }

    public static FormField BuildTestFormField(
        this CmsContext context,
        Guid formId,
        string? name = null,
        string type = "text",
        string? label = null,
        bool required = false,
        int sortOrder = 0,
        string? validationJson = null,
        string? optionsJson = null
    )
    {
        var field = new FormField
        {
            FormId = formId,
            Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            Type = type,
            Label = label ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            Required = required,
            SortOrder = sortOrder,
            ValidationJson = validationJson,
            OptionsJson = optionsJson
        };
        context.FormFields.Add(field);
        context.SaveChanges();
        return field;
    }

    public static FormSubmission BuildTestFormSubmission(
        this CmsContext context,
        Guid formId,
        string? valuesJson = null,
        string submitterIp = "127.0.0.1",
        string userAgent = "TestAgent/1.0"
    )
    {
        var submission = new FormSubmission
        {
            FormId = formId,
            ValuesJson = valuesJson ?? "{}",
            SubmitterIp = submitterIp,
            UserAgent = userAgent
        };
        context.FormSubmissions.Add(submission);
        context.SaveChanges();
        return submission;
    }
}
