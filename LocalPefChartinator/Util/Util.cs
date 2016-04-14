using NodaTime;

namespace LocalPefChartinator.Util
{
    public static class Util
    {
        public static ZonedDateTime PlusDays(this ZonedDateTime time, long days)
        {
            return time.Plus(Duration.FromStandardDays(days));
        }

        public static bool IsAfter(this LocalDate main, LocalDate other)
        {
            return main.CompareTo(other) > 0;
        }

        public static bool IsBefore(this LocalDate main, LocalDate other)
        {
            return main.CompareTo(other) < 0;
        }
    }
}
