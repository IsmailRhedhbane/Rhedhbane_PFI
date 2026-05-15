using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public sealed class NextSession
    {
        public const int January = 1;
        public const int August = 8;

        private static readonly NextSession instance = new NextSession();

        public static NextSession Instance => instance;

        public static DateTime CurrentDate = DateTime.Now;

        public static List<int> ValidSessions
        {
            get
            {
                List<int> result = new List<int>();

                if (CurrentDate.Month > January && CurrentDate.Month <= August)
                {
                    result.Add(1);
                    result.Add(3);
                    result.Add(5);
                }
                else
                {
                    result.Add(2);
                    result.Add(4);
                    result.Add(6);
                }

                return result;
            }
        }

        public static int Year
        {
            get
            {
                int value = CurrentDate.Year;

                if (CurrentDate.Month > August && CurrentDate.Month <= 12)
                    value++;

                return value;
            }
        }

        public static string ShortCaption => (ValidSessions.Contains(1) ? "Automne " : "Hiver ") + Year;

        public static string Caption => "Session " + ShortCaption;
    }
}