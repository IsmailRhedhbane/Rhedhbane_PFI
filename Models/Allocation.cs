using DAL;
using Newtonsoft.Json;
using System.Linq;

namespace Models
{
    public class Allocation : Record
    {
        public Allocation()
        {
            Year = NextSession.Year;
        }

        public int TeacherId { get; set; }

        public int CourseId { get; set; }

        public int Year { get; set; }

        [JsonIgnore]
        public Course Course => DB.Instance.Courses.Get(CourseId);

        [JsonIgnore]
        public Teacher Teacher => DB.Instance.Teachers.Get(TeacherId);

        [JsonIgnore]
        public bool IsNextSession
        {
            get
            {
                if (Course == null)
                    return false;

                return Year == NextSession.Year && NextSession.ValidSessions.Contains(Course.Session);
            }
        }
    }
}