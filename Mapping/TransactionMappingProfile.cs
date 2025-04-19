using AutoMapper;
using Banko.Models;
using Banko.Models.DTOs;

namespace Banko.Mapping
{
  public class TransactionMappingProfile : Profile
  {
    public TransactionMappingProfile()
    {
      CreateMap<Transactions, TransactionReadDto>()
        // Only need to specify mappings for non-matching properties or those needing transformation
        .ForMember(dest => dest.TransactionStatusName, opt => opt.MapFrom(src => src.Status.ToString()))
        .ForMember(dest => dest.TransactionTypeName, opt => opt.MapFrom(src => src.Type.ToString()))
        .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
        .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency.ToString()))
        // Null handling for optional fields
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
        .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow))
      ;

      CreateMap<TransactionCreateDto, Transactions>()
        .ForMember(dest => dest.TransactionNumber, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
        .ForMember(dest => dest.DestinationAccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
        // Don't map properties that will be set in the service
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.ReferenceNumber, opt => opt.Ignore())
        .ForMember(dest => dest.SourceAccountNumber, opt => opt.Ignore())
        .ForMember(dest => dest.SourceAccountId, opt => opt.Ignore())
        .ForMember(dest => dest.SourceName, opt => opt.Ignore())
        .ForMember(dest => dest.DestinationAccountId, opt => opt.Ignore())
        .ForMember(dest => dest.RecipientName, opt => opt.Ignore())
        .ForMember(dest => dest.Status, opt => opt.Ignore())
        .ForMember(dest => dest.Currency, opt => opt.Ignore())
        .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.TransactionDate, opt => opt.Ignore())
        .ForMember(dest => dest.Metadata, opt => opt.Ignore())
      ;
    }
  }
}
