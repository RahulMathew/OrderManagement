using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Queries.Order;
using OrderManagement.Presentation.Mappings;
using OrderManagement.Presentation.Requests;

namespace OrderManagement.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var createOrderCommand = OrderRequestMapping.ToCommand(request, idempotencyKey);

            var result = await _sender.Send(createOrderCommand, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error!.Code == "Idempotency.Conflict"
                    ? Conflict(new
                    {
                        result.Error.Code,
                        result.Error.Message
                    })
                    : BadRequest(new
                    {
                        result.Error.Code,
                        result.Error.Message
                    });
            }

            return CreatedAtAction(nameof(GetOrderById),
                new { orderId = result.Value },
                new { OrderId = result.Value });
        }

        // GET: api/orders/{orderId}
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetOrderById(Guid orderId, CancellationToken cancellationToken)
        {
            var query = new GetOrderByIdQuery(orderId);

            var result = await _sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(new
                {
                    Code = result.Error!.Code,
                    Message = result.Error.Message
                });
            }

            return Ok(result.Value);
        }
    }
}
