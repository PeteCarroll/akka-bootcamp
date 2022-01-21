using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace ChartApp.Actors
{
    internal class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region MessageTypes

        public class Watch
        {
            public CounterType Counter { get; private set; }

            public Watch(CounterType counter)
            {
                Counter = counter;
            }
        }

        public class UnWatch
        {
            public CounterType Counter { get; private set; }

            public UnWatch(CounterType counter)
            {
                Counter = counter;
            }
        }
        #endregion

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators
            = new Dictionary<CounterType, Func<PerformanceCounter>>()
            {
                {CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true) },
                {CounterType.Memory, () => new PerformanceCounter("Memory", "% Committed Bytes in Use", true) },
                {CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true) }
            };

        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries
            = new Dictionary<CounterType, Func<Series>>()
            {
                {CounterType.Cpu, () => new Series(CounterType.Cpu.ToString()) {ChartType = SeriesChartType.SplineArea, Color=Color.DarkGreen} },
                {CounterType.Memory, () => new Series(CounterType.Memory.ToString()) {ChartType = SeriesChartType.FastLine, Color=Color.MediumBlue} },
                {CounterType.Disk, () => new Series(CounterType.Disk.ToString()) {ChartType = SeriesChartType.SplineArea, Color=Color.DarkRed} },
            };

        private Dictionary<CounterType, IActorRef> _counterActors;
        private IActorRef _chartingActor;

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor) : 
            this(chartingActor, new Dictionary<CounterType, IActorRef>())
        { }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                if (!_counterActors.ContainsKey(watch.Counter))
                {
                    var counterActor = Context.ActorOf(Props.Create(() => new PerformanceCounterActor(watch.Counter.ToString(), CounterGenerators[watch.Counter])));
                    _counterActors[watch.Counter] = counterActor;
                }

                _chartingActor.Tell(new ChartingActor.AddSeries(CounterSeries[watch.Counter]()));
                _counterActors[watch.Counter].Tell(new SubscribeCounter(watch.Counter, _chartingActor));
            });

            Receive<UnWatch>(unwatch =>
            {
                if (!_counterActors.ContainsKey(unwatch.Counter))
                {
                    return; // Do nothing
                }

                _counterActors[unwatch.Counter].Tell(new UnsubscribeCounter(unwatch.Counter, _chartingActor));
                _chartingActor.Tell(new ChartingActor.RemoveSeries(unwatch.Counter.ToString()));
            });
        }
    }
}
