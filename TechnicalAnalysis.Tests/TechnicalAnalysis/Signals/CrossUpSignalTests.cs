using Moq;
using StrategyTester.TechnicalAnalysis;
using StrategyTester.TechnicalAnalysis.Signals;
using System.Collections.Generic;
using Xunit;

namespace TechnicalAnalysis.Tests
{
    public class CrossUpSignalTests
    {
        [Fact]
        public void MarginCrossDown_ShouldReturnFalse()
        {
            var ma7 = new Mock<IIndicator>();
            var ma25 = new Mock<IIndicator>();

            var sut = new CrossUpSignal(ma7.Object, ma25.Object);

            ma7.Setup(i => i.Calculate(It.IsAny<IReadOnlyList<Candle>>(), It.IsAny<int>())).Returns(1);
            ma25.Setup(i => i.Calculate(It.IsAny<IReadOnlyList<Candle>>(), It.IsAny<int>())).Returns(2);

            var candles = new List<Candle>();
            var index = 0;
            var result = sut.Calculate(candles, index);

            Assert.False(result);
        }

        [Fact]
        public void MarginCrossUp_ShouldReturnTrue()
        {
            var ma7 = new Mock<IIndicator>();
            var ma25 = new Mock<IIndicator>();

            var sut = new CrossUpSignal(ma7.Object, ma25.Object);

            ma7.Setup(i => i.Calculate(It.IsAny<IReadOnlyList<Candle>>(), It.IsAny<int>())).Returns(2);
            ma25.Setup(i => i.Calculate(It.IsAny<IReadOnlyList<Candle>>(), It.IsAny<int>())).Returns(1);

            var candles = new List<Candle>();
            var index = 0;
            var result = sut.Calculate(candles, index);

            Assert.True(result);
        }
    }
}
