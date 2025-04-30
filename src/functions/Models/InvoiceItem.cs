namespace Billy.Function.Models;

public class InvoiceItem
{
    public DateTime? Date { get; set; }
    public string Description { get; set; }
    public string ProductCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Unit { get; set; }
    public decimal? UnitPrice { get; set; }
}
