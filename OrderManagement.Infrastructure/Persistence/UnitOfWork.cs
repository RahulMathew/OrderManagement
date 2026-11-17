using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderManagement.Domain.Common;
using OrderManagement.Application.Abstractions.Events;
using OrderManagement.Application.Abstractions.Persistence;

namespace OrderManagement.Infrastructure.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        private IDbContextTransaction? _transaction;
        private bool _inMemoryTransactionActive;

        public UnitOfWork(AppDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        private bool IsInMemory => _context.Database.IsInMemory();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Step 1: Find tracked domain entities
            var entities = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .ToList();

            // Step 2: Collect domain events
            var domainEvents = entities
                .SelectMany(entity => entity.DomainEvents)
                .ToList();

            // Step 3: Dispatch domain events
            // This stages outbox messages in the same DbContext
            foreach (var domainEvent in domainEvents)
            {
                await _dispatcher.DispatchAsync(
                    domainEvent,
                    cancellationToken);
            }

            // Step 4: Persist changes
            var result = await _context.SaveChangesAsync(cancellationToken);

            // Step 5: Clear events after successful save
            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }

            return result;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null || _inMemoryTransactionActive)
            {
                throw new InvalidOperationException(
                    "A transaction is already active.");
            }

            if (IsInMemory)
            {
                // InMemory does not support transactions.
                // Track the call for development compatibility only.
                _inMemoryTransactionActive = true;
                return;
            }

            _transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (IsInMemory)
            {
                if (!_inMemoryTransactionActive)
                {
                    throw new InvalidOperationException(
                        "No active transaction to commit.");
                }

                // No real commit occurs for InMemory.
                // SaveChangesAsync already persisted the changes.
                _inMemoryTransactionActive = false;
                
                return;
            }

            if (_transaction is null)
            {
                throw new InvalidOperationException(
                    "No active transaction to commit.");
            }

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (IsInMemory)
            {
                // No rollback is possible with InMemory.
                _inMemoryTransactionActive = false;
                
                return;
            }

            if (_transaction is null)
                return;

            try
            {
                await _transaction.RollbackAsync(
                    cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }
    }
}
