using AutoFixture;
using StrategyTester.TechnicalAnalysis;
using StrategyTester.TechnicalAnalysis.Indicators;
using System.Collections.Generic;
using Xunit;

namespace TechnicalAnalysis.Tests.TechnicalAnalysis.Indicators
{
    public class MaIndicatorTests
    {
        private readonly Fixture _fixture;

        public MaIndicatorTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void BarCountBiggerThanCandleSize_ShouldReturnZero()
        {
            var sut = new MaIndicator(3, c => c.Close);
            var candles = new List<Candle>
            {
                _fixture.Create<Candle>(),
                _fixture.Create<Candle>(),
                _fixture.Create<Candle>()
            };

            var ma = sut.Calculate(candles, 1);

            Assert.Equal(0, ma);
        }

        [Fact]
        public void ValidIndex_ShouldReturnAverage()
        {
            var sut = new MaIndicator(3, c => c.Close);
            var candle = _fixture.Create<Candle>();

            var candles = new List<Candle>
            {
                candle,candle,candle
            };

            var ma = sut.Calculate(candles, 2);

            Assert.Equal(candle.Close, ma);
        }
    }
}
