using Akka.Actor;

namespace ChartApp.Actors
{
    #region Reporting

    public class GatherMetrics { }

    public class Metric
    {
        public string Series { get; private set; }

        public float CounterValue { get; private set; }

        public Metric(string series, float counterValue)
        {
            Series = series;
            CounterValue = counterValue;
        }
    }

    #endregion

    #region Performance Counter Management

    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    public class SubscribeCounter
    {
        public CounterType Counter { get; private set; }

        public IActorRef Subscriber { get; private set; }

        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }
    }

    public class UnsubscribeCounter
    {
        public CounterType Counter { get; private set; }

        public IActorRef Subscriber { get; private set; }

        public UnsubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }
    }

    #endregion
}
