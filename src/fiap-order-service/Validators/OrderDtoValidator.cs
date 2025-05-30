using fiap_order_service.Dtos;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace fiap_order_service.Validators
{
    [ExcludeFromCodeCoverage]
    public class OrderDtoValidator : AbstractValidator<OrderDto>
    {
        public OrderDtoValidator()
        {
            RuleFor(x => x.CustomerDocument)
                .NotEmpty().WithMessage("Customer document is required.")
                .Length(11).WithMessage("Customer document must be 11 characters long.");
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters.");
            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("Customer email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Itens)
                .NotEmpty().WithMessage("At least one item is required.");
            RuleForEach(x => x.Itens).ChildRules(itens =>
            {
                itens.RuleFor(x => x.VehicleExternalId)
                    .NotEmpty().WithMessage("Vehicle external ID is required.");
                itens.RuleFor(x => x.Amount)
                    .NotEmpty().WithMessage("Amount is required.")
                    .GreaterThan(0).WithMessage("Amount must be greater than 0.");
            });
        }
    }
}
