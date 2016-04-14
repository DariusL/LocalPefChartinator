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

        public override string ToString()
        {
            return string.Format("Pef: {0}, Time: {1}", Pef, Time);
        }

        public bool Equals(DataPoint other)
        {
            return Pef == other.Pef && Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DataPoint && Equals((DataPoint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Pef*397) ^ Time.GetHashCode();
            }
        }
    }
}