using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConvertor.Application.Dtos;
using CurrencyConvertor.Application.Common;

namespace CurrencyConvertor.Application
{
    public sealed class Convertor : IConvertor
    {
        private static readonly Lazy<Convertor> lazy =
               new Lazy<Convertor>(() => new Convertor());

        private List<CurrencyConvertorDto> _configurations;
        private List<List<double>> _rates;
        private List<List<int>> _pathGraph;
        private List<string> _symboles;
        private Graph<string> _graph = new Graph<string>();

        private static readonly Convertor _instance;

        static Convertor()
        {
            _instance = lazy.Value;
            _instance._configurations = new List<CurrencyConvertorDto>();
            ClearGraphs();
        }

        private static void ClearGraphs()
        {
            _instance._rates = new List<List<double>>();
            _instance._symboles = new List<string>();
            _instance._pathGraph = new List<List<int>>();
            _instance._graph = new Graph<string>();
        }

        public void ClearConfiguration()
        {
            _instance._configurations = new List<CurrencyConvertorDto>();
            ClearGraphs();
            Console.WriteLine("Cleared");
        }

        public async Task UpdateConfiguration(IEnumerable<CurrencyConvertorDto> conversionRates)
        {
            ClearConfiguration();
            if (conversionRates == null)
                return;
            foreach (var item in conversionRates)
            {
                var existItem = await GetItemFromConfiguration(item.SourceCurrency, item.DestCurrency);

                if (existItem == null)
                {
                    AppendNewItem(item);
                }
                else
                {
                    if (item.Rate != existItem.Rate)
                    {
                        await UpdateExistingItem(item, existItem);
                    }
                }
            }

            await UpdateGraph();

            Console.WriteLine($"Total Count of Configuration is {_instance._configurations.Count()}");
        }

        private async Task UpdateGraph()
        {
            ClearGraphs();
            var symboles = _instance._configurations.GroupBy(e => e.SourceCurrency).Select(e => e.Key).ToList();
            _instance._symboles = symboles;
            await CreatePathGraph(symboles);
            await CreateRateGraph(symboles);

        }

        private async Task CreateRateGraph(List<string> symboles)
        {
            for (int i = 0; i <= _instance._rates.Count - 1; i++)
            {
                for (int j = i + 1; j < _instance._rates.Count; j++)
                {
                    if (_instance._rates[i][j] == 0)
                    {
                        _instance._rates[i][j] = FindRateByShortestPath(i, j);
                        _instance._rates[j][i] = (double)1 / _instance._rates[i][j];
                        _instance._pathGraph[i][j] = 1;
                    }
                }
            }
        }

        private double FindRateByShortestPath(int sourccCurrencyIndex, int destCurrencyIndex)
        {
            var shortestPath = Algorithms.ShortestPathFunction(_instance._graph, _instance._symboles[sourccCurrencyIndex]);
            double rate = 1;

            var path = shortestPath(_instance._symboles[destCurrencyIndex]).ToList();
            for (int i = 0; i < path.Count - 1; i++)
            {
                rate = rate * _instance._rates[_instance._symboles.IndexOf(path[i])][_instance._symboles.IndexOf(path[i + 1])];
            }
            return rate;

        }

        private async Task CreatePathGraph(List<string> symboles)
        {
            var edges = new List<Tuple<string, string>>();
            foreach (var sourceSymbole in symboles)
            {
                List<int> rowPath = new List<int>();
                List<double> rowRate = new List<double>();
                foreach (var destSymbole in symboles)
                {
                    int hasPath = 0;
                    double rate = 0;
                    if (sourceSymbole == destSymbole)
                    {
                        hasPath = 1;
                        rate = 1;
                    }
                    else
                    {
                        var existItem = await GetItemFromConfiguration(sourceSymbole, destSymbole);
                        if (existItem != null)
                        {
                            hasPath = 1;
                            rate = existItem.Rate;
                            edges.Add(Tuple.Create(sourceSymbole, destSymbole));
                        }
                    }
                    rowPath.Add(hasPath);
                    rowRate.Add(rate);
                }
                _instance._pathGraph.Add(rowPath);
                _instance._rates.Add(rowRate);
            }
            _instance._graph = new Graph<string>(_instance._symboles, edges);

        }

        private async Task UpdateExistingItem(CurrencyConvertorDto item, CurrencyConvertorDto currentItem)
        {
            currentItem.Rate = item.Rate;
            var reverseItem = await GetItemFromConfiguration(item.DestCurrency, item.SourceCurrency);
            reverseItem.Rate = (double)1 / item.Rate;
        }

        private static void AppendNewItem(CurrencyConvertorDto item)
        {
            var reverseItem = new CurrencyConvertorDto()
            {
                SourceCurrency = item.DestCurrency,
                DestCurrency = item.SourceCurrency,
                Rate = (double)1 / item.Rate,
            };

            _instance._configurations.Add(item);
            _instance._configurations.Add(reverseItem);
        }

        public async Task<double> Convert(CurrencyConvertInput input)
        {
            var rate = GetRelatedRate(input.SourceCurrency, input.DestCurrency);
            return rate * input.Amount;
        }

        private double GetRelatedRate(string sourceCurr, string destCurr)
        {
            var sourceIndex = _instance._symboles.IndexOf(sourceCurr);
            var destIndex = _instance._symboles.IndexOf(destCurr);
            if (sourceIndex == -1 || destIndex == -1) throw new Exception("Symbole Not Found");
            var rate = _instance._rates[sourceIndex][destIndex];
            return rate;
        }

        private async Task<CurrencyConvertorDto> GetItemFromConfiguration(string sourceCurr, string destCurr)
        {
            return await Task.Run(() =>
           {
               return _instance._configurations.Where(e => e.SourceCurrency == sourceCurr && e.DestCurrency == destCurr).FirstOrDefault();
           });
        }

        public List<CurrencyConvertorDto> GetConfigurations()
        {
            return _instance._configurations;
        }

        private List<int> GetConnectedNodes(int currentVertice, List<int> visitedNodes)
        {
            var output = new List<int>();
            for (int i = 0; i < _instance._pathGraph.Count; i++)
            {
                if (_instance._pathGraph[currentVertice][i] == 1 && i != currentVertice && !visitedNodes.Contains(i))
                {
                    output.Add(i);
                }
            }
            return output;
        }
    }
}