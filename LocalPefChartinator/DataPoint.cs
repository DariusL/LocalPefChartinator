using NodaTime;

namespace LocalPefChartinator
{
    public struct DataPoint
    {
        public readonly int Pef;
        public readonly ZonedDateTime Time;

        public DataPoint(int pef, ZonedDateTime time)
        {
            Pef = pef;
            Time = time;
        }
    }
}