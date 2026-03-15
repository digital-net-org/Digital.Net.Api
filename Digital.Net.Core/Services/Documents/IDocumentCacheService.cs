using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Core.Services.Documents;

public interface IDocumentCacheService
{
    Result<FileResult?> GetCachedDocumentFile(Document? document);
}
