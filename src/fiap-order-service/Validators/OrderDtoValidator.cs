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
            RuleFor(x => x.Item)
                .NotEmpty().WithMessage("At least one item is required.");
            RuleFor(x => x.Item).ChildRules(itens =>
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
