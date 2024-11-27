using Digital.Net.Entities.Models;
using Digital.Net.Entities.Repositories;

namespace SafariDigital.Data.Context;

public class SafariDigitalRepository<T>(SafariDigitalContext context) : Repository<T, SafariDigitalContext>(context)
    where T : EntityBase;