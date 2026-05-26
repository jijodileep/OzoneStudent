using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Interceptors;
using SchoolSaaS.Shared.Events;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.UnitTests.Persistence;

public class OutboxSaveChangesInterceptorTests
{
    [Fact]
    public async Task SaveChanges_WithIntegrationEvent_WritesOutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext { TenantId = tenantId };
        var accessor = new TenantContextAccessor(tenantContext);

        var outboxInterceptor = new OutboxSaveChangesInterceptor();
        var tenantInterceptor = new TenantSaveChangesInterceptor(accessor);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(tenantInterceptor, outboxInterceptor)
            .Options;

        await using var context = new TestApplicationDbContext(options, accessor);

        var aggregate = new TestAggregate();
        aggregate.AddDomainEvent(new TestIntegrationEvent(tenantId, "hello"));
        context.TestAggregates.Add(aggregate);

        await context.SaveChangesAsync();

        var outbox = await context.OutboxMessages.IgnoreQueryFilters().SingleAsync();
        outbox.EventType.Should().Be(nameof(TestIntegrationEvent));
        outbox.TenantId.Should().Be(tenantId);
        outbox.ProcessedAt.Should().BeNull();
        outbox.PayloadJson.Should().Contain("hello");
    }

    private sealed class TestApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContextAccessor accessor) : ApplicationDbContext(options, accessor)
    {
        public DbSet<TestAggregate> TestAggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestAggregate>(entity => entity.ToTable("test_aggregates"));
        }
    }

    private sealed class TestAggregate : AggregateRoot
    {
        public string Name { get; set; } = "test";
    }
}
