using Safari.Net.Data.Entities.Models;
using Safari.Net.Data.Repositories;

namespace SafariDigital.Data.Context;

public class SafariDigitalRepository<T>(SafariDigitalContext context) : Repository<T, SafariDigitalContext>(context)
    where T : EntityBase;