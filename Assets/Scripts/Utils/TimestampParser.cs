using System;
using System.Text;

namespace CCE.Utils
{
    public static class TimestampParser
    {
        public static double Parse(string timestamp)
        {
            if (timestamp.Contains("("))
            {
                throw new FormatException("Use ParseNoteTimestamp instead.");
            }

            if (!timestamp.Contains(":"))
            {
                if (!Double.TryParse(timestamp, out var result))
                {
                    throw new FormatException("Could not parse timestamp. Invalid Format");
                }

                return result;
            }

            var parts = timestamp.Split(':');
            var secondsString = parts[1];
            var minutesString = parts[0];
            if (!Double.TryParse(secondsString, out var seconds))
            {
                throw new FormatException("Could not parse timestamp. Invalid Format");
            }

            if (!Int32.TryParse(minutesString, out var minutes))
            {
                throw new FormatException("Could not parse timestamp. Invalid Format");
            }

            return 60 * minutes + seconds;
        }

        public static string Serialize(double time)
        {
            var sb = new StringBuilder(9);
            if (time < 0)
            {
                sb.Append('-');
                time = -time;
            }

            var minutes = (int)Math.Floor(time / 60);
            sb.Append(minutes.ToString("D2"));
            sb.Append(':');

            var seconds = (int)(time - minutes * 60);
            sb.Append(seconds.ToString("D2"));
            sb.Append('.');

            var milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
            sb.Append(milliseconds.ToString("D3"));

            return sb.ToString();
        }
    }
}