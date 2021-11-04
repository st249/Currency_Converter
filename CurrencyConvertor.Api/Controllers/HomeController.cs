using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using CurrencyConvertor.Application;
using Microsoft.AspNetCore.Mvc;
using CurrencyConvertor.Application.Dtos;

namespace CurrencyConvertor.Api.Controllers
{
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly IConvertor _convertor;
        public HomeController(IConvertor convertor)
        {
            _convertor = convertor;
        }

        [HttpPost]
        [Route("/ClearConfigurations")]
        public IActionResult ClearConfigurations()
        {
            _convertor.ClearConfiguration();
            return Ok();
        }

        [HttpPost]
        [Route("/UpdateConfigurations")]
        public async Task<IActionResult> UpdateConfigurations([FromBody] IEnumerable<CurrencyConvertorDto> newConf)
        {
            await _convertor.UpdateConfiguration(newConf);
            return Ok();
        }

        [HttpGet]
        [Route("/Convert/{sourceCurr}/{destCurr}/{amount}")]
        public async Task<IActionResult> Convert(string sourceCurr, string destCurr, double amount)
        {
            var convertInput = new CurrencyConvertInput()
            {
                Amount = amount,
                DestCurrency = destCurr,
                SourceCurrency = sourceCurr
            };

            var output = await _convertor.Convert(convertInput);

            return Ok(new { ConvertionValue = output });
        }

        [HttpGet]
        [Route("/GetConfigurations")]
        public IActionResult GetConfigurations()
        {
            return Ok(_convertor.GetConfigurations());
        }
    }
}