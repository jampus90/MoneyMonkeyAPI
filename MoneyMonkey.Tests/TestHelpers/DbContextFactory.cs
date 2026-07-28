using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MoneyMonkey.Data;

namespace MoneyMonkey.Tests.TestHelpers;

public static class DbContextFactory
{
    public static MoneyMonkeyDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MoneyMonkeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new MoneyMonkeyDbContext(options);
    }
}
