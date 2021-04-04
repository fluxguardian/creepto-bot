using StrategyTester.Enums;
using System;

namespace StrategyTester
{
    public record Trade
    {
        public DateTime TradeDate { get; init; }

        public TradeDirection Direction { get; init; }
        
        public decimal Price { get; init; }
        
        public decimal Volume { get; init; }

        public decimal Fee { get; init; }

        public decimal VolumeEur { get; init; }
        public string Signal { get; internal set; }
    }
}
