namespace CurrencyConvertor.Application.Dtos
{
    public class CurrencyConvertorDto
    {
        public string SourceCurrency { get; set; }
        public string DestCurrency { get; set; }
        public double Rate { get; set; }
    }
}