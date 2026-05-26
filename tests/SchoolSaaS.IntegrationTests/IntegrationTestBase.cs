using Microsoft.AspNetCore.Mvc.Testing;

namespace SchoolSaaS.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(DatabaseFixture fixture) => Fixture = fixture;

    protected DatabaseFixture Fixture { get; }

    protected WebApplicationFactory<Program> Factory => Fixture.Factory;
}
