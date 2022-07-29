using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using TransactionAggregate;

    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;

        public DeveloperController(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository) {
            this.TransactionAggregateRepository = transactionAggregateRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddTransactionSourceToTransaction([FromQuery]Guid estateId,
                                                                           [FromQuery] Guid merchantId,
                                                                           [FromQuery] Guid transactionId,
                                                                           [FromQuery] Int32 transactionSource,
                                                                           CancellationToken cancellationToken) {
            var aggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            if (aggregate.IsCompleted && aggregate.EstateId == estateId && aggregate.MerchantId == merchantId) {
                aggregate.AddTransactionSource((TransactionSource)transactionSource);

                await this.TransactionAggregateRepository.SaveChanges(aggregate, cancellationToken);

                return this.Ok();
            }

            return this.BadRequest();
        }
    }
}
