using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Models
{
    public class Student : Record
    {
        [Required(ErrorMessage = "Le prénom est requis.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        public string LastName { get; set; }

        public string Code { get; set; }

        [Required(ErrorMessage = "La date de naissance est requise.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Le courriel est requis.")]
        [EmailAddress(ErrorMessage = "Courriel invalide.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le téléphone est requis.")]
        public string Phone { get; set; }

        [JsonIgnore]
        public string FullName => LastName + " " + FirstName;

        [JsonIgnore]
        public string Caption => Code + " " + LastName + " " + FirstName;

        [JsonIgnore]
        public int Year
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Code) || Code.Length < 4)
                    return DateTime.Now.Year;

                return int.Parse(Code.Substring(0, 4));
            }
        }

        [JsonIgnore]
        public List<Registration> Registrations
        {
            get
            {
                return DB.Instance.Registrations.ToList()
                    .Where(r => r.StudentId == Id)
                    .ToList();
            }
        }

        [JsonIgnore]
        public List<Registration> NextSessionRegistrations
        {
            get
            {
                return Registrations
                    .Where(r => r.IsNextSession)
                    .ToList();
            }
        }

        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                List<Course> courses = new List<Course>();

                foreach (Registration registration in Registrations.Where(r => r.Course != null).OrderBy(r => r.Course.Code))
                {
                    courses.Add(registration.Course);
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

                foreach (Registration registration in NextSessionRegistrations.Where(r => r.Course != null).OrderBy(r => r.Course.Code))
                {
                    courses.Add(registration.Course);
                }

                return courses;
            }
        }

        [JsonIgnore]
        public SelectList CoursesSelectList => SelectListUtilities<Course>.Convert(Courses, "Caption");

        [JsonIgnore]
        public SelectList NextSessionCoursesToSelectList => SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption");

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Phone);
        }

        public void DeleteAllRegistrations()
        {
            foreach (Registration registration in Registrations.ToList())
            {
                DB.Instance.Registrations.Delete(registration.Id);
            }
        }

        public void DeleteNextSessionRegistrations()
        {
            foreach (Registration registration in NextSessionRegistrations.ToList())
            {
                DB.Instance.Registrations.Delete(registration.Id);
            }
        }

        public void UpdateRegistrations(List<int> selectedCoursesId)
        {
            DeleteNextSessionRegistrations();

            if (selectedCoursesId != null)
            {
                foreach (int courseId in selectedCoursesId)
                {
                    DB.Instance.Registrations.Add(new Registration
                    {
                        StudentId = Id,
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
                code = DateTime.Now.Year.ToString() + random.Next(100000, 999999).ToString();
            }
            while (DB.Instance.Students.ToList().Any(s => s.Code == code));

            return code;
        }
    }
}