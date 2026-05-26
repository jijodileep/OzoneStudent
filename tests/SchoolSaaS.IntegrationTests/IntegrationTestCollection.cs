namespace SchoolSaaS.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "IntegrationTests";
}
