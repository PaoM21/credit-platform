using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CreditService.Application.Commands;

namespace CreditService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CreditsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // Endpoint mínimo para CreatedAtAction (puedes implementar full query handler luego)
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id) => Ok(new { id });
    }
}