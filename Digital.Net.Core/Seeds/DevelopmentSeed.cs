using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Entities.Seeds;
using Digital.Net.Core.Services.Authentication.Utils;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Seeds;

public class DevelopmentSeed(
    ILogger<DevelopmentSeed> logger,
    DigitalContext context
) : Seeder<User>(logger, context), ISeed
{
    private readonly DigitalContext _context = context;
    
    public const string DefaultPassword = "Devpassword123!";
    public static string DefaultHashPassword = PasswordUtils.HashPassword(DefaultPassword);

    public static string GenerateApiKey(User user) =>
        $"dev_{user.Login.ToLower()}_s12d5fg4h56m56z4ergf561gfj764m4fgsd56fgsj956qierfgd5498746sf8gap9jrp8ez7tazecz079e87u98uo7tyu978az111era98dwckg833574kiumpt"
            [..128];

    public override async Task ApplySeed()
    {
        var result = await SeedAsync(Users);
        if (result.HasError)
            throw new Exception(result.Errors.First().Message);

        foreach (var apiKey in result.Value!.Select(user => new ApiKey(user.Id, $"dev-{user.Login.ToLower()}", GenerateApiKey(user))))
            await _context.ApiKeys.AddAsync(apiKey);
        await _context.SaveChangesAsync();
    }

    private static List<User> Users =>
    [
        new()
        {
            Username = "BenoitSafari",
            Login = "BenoitSafari",
            Password = DefaultHashPassword,
            Email = "benoitsafari@fake.com",
            IsActive = true,
            IsAdmin = true
        },
        new()
        {
            Username = "jdupont", Login = "jdupont", Password = DefaultHashPassword,
            Email = "j.dupont@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "mbernard", Login = "mbernard", Password = DefaultHashPassword,
            Email = "m.bernard@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "tleclerc", Login = "tleclerc", Password = DefaultHashPassword,
            Email = "thomas.leclerc@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "apetit", Login = "apetit", Password = DefaultHashPassword,
            Email = "a.petit@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "rmoreau", Login = "rmoreau", Password = DefaultHashPassword,
            Email = "remi.moreau@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "flambert", Login = "flambert", Password = DefaultHashPassword,
            Email = "f.lambert@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "grousseau", Login = "grousseau", Password = DefaultHashPassword,
            Email = "g.rousseau@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "ndurand", Login = "ndurand", Password = DefaultHashPassword,
            Email = "n.durand@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "blenoir", Login = "blenoir", Password = DefaultHashPassword,
            Email = "b.lenoir@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "pmartin", Login = "pmartin", Password = DefaultHashPassword,
            Email = "p.martin@gmail.com", IsActive = true, IsAdmin = false
        },

        new()
        {
            Username = "sdubois", Login = "sdubois", Password = DefaultHashPassword,
            Email = "s.dubois@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "hrobert", Login = "hrobert", Password = DefaultHashPassword,
            Email = "h.robert@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "lrichard", Login = "lrichard", Password = DefaultHashPassword,
            Email = "l.richard@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "cgarcia", Login = "cgarcia", Password = DefaultHashPassword,
            Email = "c.garcia@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "vfaure", Login = "vfaure", Password = DefaultHashPassword,
            Email = "v.faure@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "qandré", Login = "qandre", Password = DefaultHashPassword,
            Email = "q.andre@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "efernand", Login = "efernand", Password = DefaultHashPassword,
            Email = "e.fernand@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "yblanc", Login = "yblanc", Password = DefaultHashPassword,
            Email = "y.blanc@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "ojacquet", Login = "ojacquet", Password = DefaultHashPassword,
            Email = "o.jacquet@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "kcolin", Login = "kcolin", Password = DefaultHashPassword,
            Email = "k.colin@gmail.com", IsActive = true, IsAdmin = false
        },

        new()
        {
            Username = "brenaud", Login = "brenaud", Password = DefaultHashPassword,
            Email = "b.renaud@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "nollivier", Login = "nollivier", Password = DefaultHashPassword,
            Email = "n.ollivier@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "dmeunier", Login = "dmeunier", Password = DefaultHashPassword,
            Email = "d.meunier@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "pschmitt", Login = "pschmitt", Password = DefaultHashPassword,
            Email = "p.schmitt@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "xnoel", Login = "xnoel", Password = DefaultHashPassword,
            Email = "x.noel@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "jchevalier", Login = "jchevalier", Password = DefaultHashPassword,
            Email = "j.chevalier@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "mmarchand", Login = "mmarchand", Password = DefaultHashPassword,
            Email = "m.marchand@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "tbarbier", Login = "tbarbier", Password = DefaultHashPassword,
            Email = "t.barbier@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "vperrin", Login = "vperrin", Password = DefaultHashPassword,
            Email = "v.perrin@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "fmonnier", Login = "fmonnier", Password = DefaultHashPassword,
            Email = "f.monnier@gmail.com", IsActive = true, IsAdmin = false
        },

        new()
        {
            Username = "aboyer", Login = "aboyer", Password = DefaultHashPassword,
            Email = "a.boyer@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "gremy", Login = "gremy", Password = DefaultHashPassword,
            Email = "g.remy@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "jlefevre", Login = "jlefevre", Password = DefaultHashPassword,
            Email = "j.lefevre@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "cdumas", Login = "cdumas", Password = DefaultHashPassword,
            Email = "c.dumas@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "rcollet", Login = "rcollet", Password = DefaultHashPassword,
            Email = "r.collet@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "sdupuy", Login = "sdupuy", Password = DefaultHashPassword,
            Email = "s.dupuy@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "blemoine", Login = "blemoine", Password = DefaultHashPassword,
            Email = "b.lemoine@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "tdelahaye", Login = "tdelahaye", Password = DefaultHashPassword,
            Email = "t.delahaye@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "yberthe", Login = "yberthe", Password = DefaultHashPassword,
            Email = "y.berthe@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "jhuet", Login = "jhuet", Password = DefaultHashPassword,
            Email = "j.huet@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "pcolas", Login = "pcolas", Password = DefaultHashPassword,
            Email = "p.colas@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "dperrot", Login = "dperrot", Password = DefaultHashPassword,
            Email = "d.perrot@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "nrenaudin", Login = "nrenaudin", Password = DefaultHashPassword,
            Email = "n.renaudin@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "lpascal", Login = "lpascal", Password = DefaultHashPassword,
            Email = "l.pascal@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "fvallet", Login = "fvallet", Password = DefaultHashPassword,
            Email = "f.vallet@yahoo.fr", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "mrenaud", Login = "mrenaud", Password = DefaultHashPassword,
            Email = "m.renaud@gmail.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "achauvin", Login = "achauvin", Password = DefaultHashPassword,
            Email = "a.chauvin@outlook.com", IsActive = true, IsAdmin = false
        },
        new()
        {
            Username = "jcharrier", Login = "jcharrier", Password = DefaultHashPassword,
            Email = "j.charrier@gmail.com", IsActive = true, IsAdmin = false
        }
    ];
}