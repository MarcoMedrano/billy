namespace Billy.Function.Models;

public class Card
{
    public string CardIssuer { get; set; }
    public long CardNumber { get; set; }
    public string GoodThru { get; set; }
    public string Name { get; set; }
    public ushort SecurityCode { get; set; }
}