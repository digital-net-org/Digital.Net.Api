using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Files;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Services.Documents;

public interface IDocumentService
{
    string GetDocumentPath(Document document);
    string? ResolveExistingPath(Document document);
    Task<Result<Document>> SaveDocumentAsync(Stream content, FileDefinition definition, User? uploader);

    Task<Result<Document>> SaveImageDocumentAsync(
        Stream content,
        FileDefinition definition,
        User? uploader,
        int? quality = null
    );

    Task<Result> RemoveDocumentAsync(Guid id);
}
