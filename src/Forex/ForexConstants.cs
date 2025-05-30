// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Azure.Migrate.Explore.Forex
{
    class ForexConstants
    {
        private readonly Dictionary<string, double> ForexRates;

        private static object instanceLock = new object();

        private static ForexConstants internalInstance = null;

        private ForexConstants()
        {
            ForexRates = new Dictionary<string, double>();

            // Exchange rates from -> https://azure.microsoft.com/en-in/pricing/details/site-recovery/
            // Updated -> 2023/08/02
            // Base price is USD
            ForexRates.Add("USD", 1.0);
            ForexRates.Add("AUD", 1.4864);
            ForexRates.Add("BRL", 4.7306);
            ForexRates.Add("CAD", 1.3197);
            ForexRates.Add("DKK", 6.7797);
            ForexRates.Add("EUR", 0.9098);
            ForexRates.Add("INR", 81.9394);
            ForexRates.Add("JPY", 141.255);
            ForexRates.Add("KRW", 1277.75);
            ForexRates.Add("NZD", 1.6141);
            ForexRates.Add("NOK", 10.1251);
            ForexRates.Add("RUB", 90.8);
            ForexRates.Add("SEK", 10.4888);
            ForexRates.Add("CHF", 0.869);
            ForexRates.Add("TWD", 31.239);
            ForexRates.Add("GBP", 0.7783);
        }

        public static ForexConstants Instance
        {
            get
            {
                if (internalInstance == null)
                {
                    lock (instanceLock)
                    {
                        if (internalInstance == null)
                        {
                            internalInstance = new ForexConstants();
                        }
                    }
                }
                return internalInstance;
            }
        }

        public double GetExchangeRate(string currencySymbol)
        {
            if (!ForexRates.ContainsKey(currencySymbol))
                return 1.0;

            return ForexRates[currencySymbol];
        }
    }
}