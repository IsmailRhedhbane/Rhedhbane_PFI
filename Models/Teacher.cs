using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Models
{
    public class Teacher : Record
    {
        private string _email;

        [Required(ErrorMessage = "Le prénom est requis.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        public string LastName { get; set; }

        public string Code { get; set; }

        [Required(ErrorMessage = "La date d'embauche est requise.")]
        public DateTime StartDate { get; set; }

        [EmailAddress(ErrorMessage = "Courriel invalide.")]
        public string Email
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_email))
                    return _email;

                return GenerateDefaultEmail();
            }
            set
            {
                _email = value;
            }
        }

        [Required(ErrorMessage = "Le téléphone est requis.")]
        public string Phone { get; set; }

        public string Avatar { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get { return LastName + " " + FirstName; }
        }

        [JsonIgnore]
        public string Caption
        {
            get { return Code + " " + LastName + " " + FirstName; }
        }

        [JsonIgnore]
        public List<Allocation> Allocations
        {
            get
            {
                return DB.Instance.Allocations.ToList()
                    .Where(a => a.TeacherId == Id)
                    .ToList();
            }
        }

        [JsonIgnore]
        public List<Allocation> NextSessionAllocations
        {
            get
            {
                return Allocations
                    .Where(a => a.IsNextSession)
                    .ToList();
            }
        }

        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                List<Course> courses = new List<Course>();

                foreach (Allocation allocation in Allocations
                    .Where(a => a.Course != null)
                    .OrderBy(a => a.Course.Session)
                    .ThenBy(a => a.Course.Code))
                {
                    courses.Add(allocation.Course);
                }

                return courses;
            }
        }

        [JsonIgnore]
        public List<Course> NextSessionCourses
        {
            get
            {
                List<Course> courses = new List<Course>();

                foreach (Allocation allocation in NextSessionAllocations
                    .Where(a => a.Course != null)
                    .OrderBy(a => a.Course.Session)
                    .ThenBy(a => a.Course.Code))
                {
                    courses.Add(allocation.Course);
                }

                return courses;
            }
        }

        [JsonIgnore]
        public SelectList CoursesSelectList
        {
            get { return SelectListUtilities<Course>.Convert(Courses, "Caption"); }
        }

        [JsonIgnore]
        public SelectList NextSessionCoursesToSelectList
        {
            get { return SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption"); }
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Phone);
        }

        public void DeleteAllAllocations()
        {
            foreach (Allocation allocation in Allocations.ToList())
            {
                DB.Instance.Allocations.Delete(allocation.Id);
            }
        }

        public void DeleteNextSessionAllocations()
        {
            foreach (Allocation allocation in NextSessionAllocations.ToList())
            {
                DB.Instance.Allocations.Delete(allocation.Id);
            }
        }

        public void UpdateAllocations(List<int> selectedCoursesId)
        {
            DeleteNextSessionAllocations();

            if (selectedCoursesId != null)
            {
                foreach (int courseId in selectedCoursesId)
                {
                    DB.Instance.Allocations.Add(new Allocation
                    {
                        TeacherId = Id,
                        CourseId = courseId,
                        Year = NextSession.Year
                    });
                }
            }
        }

        public static string GenerateUniqueCode()
        {
            Random random = new Random();
            string code;

            do
            {
                code = "CLG-420-" + random.Next(10000, 99999).ToString();
            }
            while (DB.Instance.Teachers.ToList().Any(t => t.Code == code));

            return code;
        }

        private string GenerateDefaultEmail()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                return "";

            string firstName = RemoveAccents(FirstName).Replace(" ", "").Replace("-", "");
            string lastName = RemoveAccents(LastName).Replace(" ", "").Replace("-", "");

            return firstName + "." + lastName + "@clg.qc.ca";
        }

        private static string RemoveAccents(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}