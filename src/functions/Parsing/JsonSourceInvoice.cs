using Newtonsoft.Json;

namespace Billy.Function.Parsing;

public class JsonSourceInvoice
{
    public JsonSourceValue<decimal> AmountDue { get; set; }
    public JsonSourceValue<string> BillingAddress { get; set; }
    public JsonSourceValue<string> BillingAddressRecipient { get; set; }
    public JsonSourceValue<string> CustomerAddress { get; set; }
    public JsonSourceValue<string> CustomerAddressRecipient { get; set; }
    public JsonSourceValue<string> CustomerId { get; set; }
    public JsonSourceValue<string> CustomerName { get; set; }
    public JsonSourceValue<string> CustomerTaxId { get; set; }
    public JsonSourceValue<string> DueDate { get; set; }
    public JsonSourceValue<string> InvoiceDate { get; set; }
    public JsonSourceValue<string> InvoiceId { get; set; }
    public JsonSourceValue<decimal> InvoiceTotal { get; set; }
    public JsonSourceValue<decimal> PreviousUnpaidBalance { get; set; }
    public JsonSourceValue<string> PurchaseOrder { get; set; }
    public JsonSourceValue<string> VendorName { get; set; }
    public JsonSourceArray Items { get; set; }
}

public class JsonSourceValue<T>
{
    [JsonProperty("valueString")]
    public string ValueString { get; set; }
    
    [JsonProperty("valueNumber")]
    public decimal ValueNumber { get; set; }
    
    [JsonProperty("valueDate")]
    public string ValueDate { get; set; }
    
    public T Value => typeof(T) == typeof(string) ? 
        (T)(object)ValueString : 
        typeof(T) == typeof(decimal) ? 
        (T)(object)ValueNumber : 
        default(T);
}

public class JsonSourceArray
{
    [JsonProperty("valueArray")]
    public List<JsonSourceArrayItem> ValueArray { get; set; }
}

public class JsonSourceArrayItem
{
    [JsonProperty("valueObject")]
    public JsonSourceItem ValueObject { get; set; }
}

public class JsonSourceItem
{
    public JsonSourceValue<string> Date { get; set; }
    public JsonSourceValue<string> Description { get; set; }
    public JsonSourceValue<string> ProductCode { get; set; }
    public JsonSourceValue<decimal> Quantity { get; set; }
    public JsonSourceValue<decimal> TaxAmount { get; set; }
    public JsonSourceValue<decimal> TaxRate { get; set; }
    public JsonSourceValue<decimal> TotalPrice { get; set; }
    public JsonSourceValue<string> Unit { get; set; }
    public JsonSourceValue<decimal> UnitPrice { get; set; }
}