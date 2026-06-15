using Digital.Net.Lib.Entities.Mutations;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Lib.Entities.Context;

public interface IDigitalNetContext
{
    static abstract string Schema { get; }

    // Contract lock: every Digital.Net context must persist the audit log, so mutation tracking can never be
    // silently skipped on a new context.
    DbSet<EntityMutation> EntityMutations { get; }
}