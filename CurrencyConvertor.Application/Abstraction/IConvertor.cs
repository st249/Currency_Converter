using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyConvertor.Application.Dtos;

namespace CurrencyConvertor.Application
{
    public interface IConvertor
    {
        void ClearConfiguration();

        Task UpdateConfiguration(IEnumerable<CurrencyConvertorDto> conversionRates);

        List<CurrencyConvertorDto> GetConfigurations();

        Task<double> Convert(CurrencyConvertInput input);
    }
}