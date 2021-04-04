using StrategyTester.TechnicalAnalysis;
using StrategyTester.TechnicalAnalysis.Signals;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace StrategyTester
{
    public class SignalEngine
    {
        private Expression _callExpression;
        readonly ParameterExpression _candles = Expression.Parameter(typeof(IReadOnlyList<Candle>), "candles");
        readonly ParameterExpression _index = Expression.Parameter(typeof(int), "index");

        internal SignalEngine Set(ISignal signal)
        {
            _callExpression = BuildMethodCall(signal);
            return this;
        }

        internal SignalEngine Or(ISignal signal)
        {
            _callExpression = Expression.Or(_callExpression, BuildMethodCall(signal));
            return this;
        }

        internal SignalEngine And(ISignal signal)
        {
            _callExpression = Expression.And(_callExpression, BuildMethodCall(signal));
            return this;
        }

        internal Func<IReadOnlyList<Candle>, int, bool> BuildExpression()
            => Expression.Lambda<Func<IReadOnlyList<Candle>, int, bool>>(
                                _callExpression,
                                new ParameterExpression[] { _candles, _index }).Compile();


        private MethodCallExpression BuildMethodCall(ISignal signal)
            => Expression.Call(Expression.Constant(signal),
                                signal.GetType().GetMethod(nameof(ISignal.Calculate), new Type[] { typeof(IReadOnlyList<Candle>), typeof(int) }),
                                _candles,
                                _index);

    }
}