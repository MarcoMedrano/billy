using AutoMapper;
using Billy.Function.Models;
using Newtonsoft.Json;

namespace Billy.Function.Parsing;

public class InvoiceParser
{
    private static readonly IMapper mapper;

    static InvoiceParser()
    {
        var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<JsonSourceInvoice, Invoice>()
                    .ForMember(dest => dest.AmountDue, opt => opt.MapFrom(src => src.AmountDue.ValueNumber))
                    .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.BillingAddress.ValueString))
                    .ForMember(dest => dest.BillingAddressRecipient, opt => opt.MapFrom(src => src.BillingAddressRecipient.ValueString))
                    .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.CustomerAddress.ValueString))
                    .ForMember(dest => dest.CustomerAddressRecipient, opt => opt.MapFrom(src => src.CustomerAddressRecipient.ValueString))
                    .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId.ValueString))
                    .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName.ValueString))
                    .ForMember(dest => dest.CustomerTaxId, opt => opt.MapFrom(src => src.CustomerTaxId.ValueString))
                    .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => DateTime.Parse(src.DueDate.ValueDate)))
                    .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => DateTime.Parse(src.InvoiceDate.ValueDate)))
                    .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId.ValueString))
                    .ForMember(dest => dest.InvoiceTotal, opt => opt.MapFrom(src => src.InvoiceTotal.ValueNumber))
                    .ForMember(dest => dest.PreviousUnpaidBalance, opt => opt.MapFrom(src => src.PreviousUnpaidBalance.ValueNumber))
                    .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src.PurchaseOrder.ValueString))
                    .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.VendorName.ValueString))
                    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.ValueArray.Select(i => i.ValueObject)));
                
                cfg.CreateMap<JsonSourceItem, InvoiceItem>()
                    .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date != null && !string.IsNullOrEmpty(src.Date.ValueDate) ? 
                        DateTime.Parse(src.Date.ValueDate) : (DateTime?)null))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.ValueString))
                    .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode.ValueString))
                    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity.ValueNumber))
                    .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.ValueNumber))
                    .ForMember(dest => dest.TaxRate, opt => opt.MapFrom(src => src.TaxRate.ValueNumber))
                    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice.ValueNumber))
                    .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit.ValueString))
                    .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.ValueNumber));
            });
            
        mapper = config.CreateMapper();
    }

    public static Invoice Parse(string jsonContent)
    {
        var jsonSource = JsonConvert.DeserializeObject<JsonSourceInvoice>(jsonContent);

        return mapper.Map<Invoice>(jsonSource);
    }
}
