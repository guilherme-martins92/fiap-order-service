using fiap_order_service.Models;
using FluentValidation;

namespace fiap_order_service.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
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
        }
    }
}
