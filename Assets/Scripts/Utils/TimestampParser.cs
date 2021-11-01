using System;
using System.Text;
using CCE.Data;
using Unity.Mathematics;

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
                if (!Double.TryParse(timestamp, out double result))
                    throw new FormatException("Could not parse timestamp. Invalid Format");

                return result;
            }

            string[] parts = timestamp.Split(':');
            string secondsString = parts[1];
            string minutesString = parts[0];
            if (!Double.TryParse(secondsString, out double seconds))
                throw new FormatException("Could not parse timestamp. Invalid Format");
            if (!Int32.TryParse(minutesString, out int minutes))
                throw new FormatException("Could not parse timestamp. Invalid Format");

            return 60 * minutes + seconds;
        }

        private static readonly char[] _idSeparators = { '(', ')' };

        public static int ParseNoteTimestamp(string timestamp)
        {
            if (!Int32.TryParse(timestamp.Split(_idSeparators)[1], out int noteId))
            {
                throw new FormatException("Could not parse note timestamp. Invalid Format");
            }

            return noteId;
        }
        
        public static string Serialize(double time)
        {
            var sb = new StringBuilder(9);
            if (time < 0)
            {
                sb.Append('-');
                time = -time;
            }
            
            int minutes = (int)Math.Floor(time / 60);
            sb.Append(minutes.ToString("D2"));
            sb.Append(':');

            int seconds = (int)(time - minutes * 60);
            sb.Append(seconds.ToString("D2"));
            sb.Append('.');
            
            int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
            sb.Append(milliseconds.ToString("D3"));
            
            return sb.ToString();
        }

        public static string Serialize(Note note)
        {
            return $"{Serialize(note.Time)} ({note.ID})";
        }
    }
}