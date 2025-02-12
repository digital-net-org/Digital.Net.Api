using Digital.Lib.Net.Core.Messages;
using Digital.Pages.Data.Models.Documents;

namespace Digital.Pages.Services.Documents;

public static class DocumentUtils
{
    public static Result RemoveDocumentFile(string directory, Document? document)
    {
        var result = new Result();
        try
        {
            if (document is null) throw new Exception("Document object is not defined.");
            var path = Path.Combine(directory, document.FileName);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}