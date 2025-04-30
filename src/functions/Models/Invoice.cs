namespace Billy.Function.Models;

public class Invoice
{
    public decimal AmountDue { get; set; }
    public string BillingAddress { get; set; }
    public string BillingAddressRecipient { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerAddressRecipient { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerTaxId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceId { get; set; }
    public decimal InvoiceTotal { get; set; }
    public decimal PreviousUnpaidBalance { get; set; }
    public string PurchaseOrder { get; set; }
    public string VendorName { get; set; }
    public List<InvoiceItem> Items { get; set; } = new ();
}
