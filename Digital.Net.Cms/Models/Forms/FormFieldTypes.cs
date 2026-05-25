namespace Digital.Net.Cms.Models.Forms;

public static class FormFieldTypes
{
    public const string Text = "text";
    public const string Textarea = "textarea";
    public const string Email = "email";
    public const string Number = "number";
    public const string Checkbox = "checkbox";
    public const string Select = "select";
    public const string Radio = "radio";
    public const string Message = "message";

    public static readonly IReadOnlyList<string> All =
        [Text, Textarea, Email, Number, Checkbox, Select, Radio, Message];
}
