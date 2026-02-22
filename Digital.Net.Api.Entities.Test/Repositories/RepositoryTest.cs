using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Api.Entities.Test.Repositories;

public class RepositoryTest : UnitTest, IDisposable
{
    private static Document GetTestDocument() => new()
    {
        FileName = Randomizer.GenerateRandomString(Randomizer.AnyCharacter),
        MimeType = Randomizer.GenerateRandomString(Randomizer.AnyCharacter),
        FileSize = 1
    };

    private readonly SqliteConnection _connection;
    private readonly Repository<Document> _documentRepository;
    private readonly Repository<Avatar> _avatarRepository;

    public RepositoryTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _documentRepository = new Repository<Document>(context);
        _avatarRepository = new Repository<Avatar>(context);
    }

    [Test]
    public async Task UpdateNestedEntities_ShouldUpdateNestedEntity()
    {
        const string payload = "test";
        var document = await _documentRepository.CreateAndSaveAsync(GetTestDocument());
        var avatar = await _avatarRepository.CreateAndSaveAsync(new Avatar { DocumentId = document.Id });
        avatar.Document!.FileName = payload;
        await _documentRepository.UpdateAndSaveAsync(document);

        var result = _documentRepository.Get().First();
        await Assert.That(result.FileName).IsEqualTo(payload);
    }

    [Test]
    public async Task AddTimestamps_ShouldSetNow_OnCreatedAtAndUpdatedAt()
    {
        var document = await _documentRepository.CreateAndSaveAsync(GetTestDocument());
        await Assert.That(document.CreatedAt > DateTime.UtcNow.AddSeconds(-60)).IsTrue();
        await Assert.That(document.UpdatedAt is null).IsTrue();

        document = await _documentRepository.UpdateAndSaveAsync(document);
        await Assert.That(document.UpdatedAt > DateTime.UtcNow.AddSeconds(-60)).IsTrue();
    }

    public void Dispose() => _connection.Dispose();
}