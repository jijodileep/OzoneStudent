namespace SchoolSaaS.IntegrationTests.Outbox;

[CollectionDefinition(Name)]
public sealed class OutboxRabbitMqCollection : ICollectionFixture<OutboxRabbitMqFixture>
{
    public const string Name = "OutboxRabbitMq";
}
