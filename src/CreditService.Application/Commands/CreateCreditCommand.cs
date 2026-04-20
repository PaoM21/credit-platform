using MediatR;
using System;

namespace CreditService.Application.Commands
{
    public class CreateCreditCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
}
