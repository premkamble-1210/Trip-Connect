using AutoMapper;
using APPLICATION_LAYER.DTOs.Expense;
using DOMAIN_LAYER.Entity.Expense;
using DOMAIN_LAYER.Entity.ExpenseSplit;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for Expense and ExpenseSplit entity mappings
    /// </summary>
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // Expense → ExpenseResponseDto
            CreateMap<Expense, ExpenseResponseDto>()
                .ForMember(dest => dest.Splits, opt => opt.MapFrom(src => 
                    src.ExpenseSplits.Select(es => new ExpenseSplitResponseDto
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        AmountOwed = es.AmountOwed,
                        IsSettled = es.IsSettled
                    }).ToList()));

            // CreateExpenseDto → Expense
            CreateMap<CreateExpenseDto, Expense>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PaidBy, opt => opt.Ignore());

            // ExpenseSplitItemDto → ExpenseSplit
            CreateMap<ExpenseSplitItemDto, ExpenseSplit>()
                .ForMember(dest => dest.IsSettled, opt => opt.MapFrom(src => false));

            // ExpenseSplit → ExpenseSplitResponseDto
            CreateMap<ExpenseSplit, ExpenseSplitResponseDto>();
        }
    }
}
