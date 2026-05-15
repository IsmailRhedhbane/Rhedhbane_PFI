using DAL;
using Newtonsoft.Json;
using System.Linq;

namespace Models
{
    public class Registration : Record
    {
        public Registration()
        {
            Year = NextSession.Year;
        }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public int Year { get; set; }

        [JsonIgnore]
        public Course Course => DB.Instance.Courses.Get(CourseId);

        [JsonIgnore]
        public Student Student => DB.Instance.Students.Get(StudentId);

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