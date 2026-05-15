using DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Models
{
    public class Course : Record
    {
        [Required(ErrorMessage = "Le sigle est requis.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Le titre est requis.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La session est requise.")]
        public int Session { get; set; }

        [JsonIgnore]
        public string Caption => "[" + Session + "] " + Code + " " + Title;

        [JsonIgnore]
        public List<Registration> Registrations
        {
            get
            {
                return DB.Instance.Registrations.ToList()
                    .Where(r => r.CourseId == Id)
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
        public List<Student> Students
        {
            get
            {
                List<Student> students = new List<Student>();

                foreach (Registration registration in Registrations.Where(r => r.Student != null).OrderBy(r => r.Student.LastName))
                {
                    students.Add(registration.Student);
                }

                return students;
            }
        }

        [JsonIgnore]
        public List<Student> NextSessionStudents
        {
            get
            {
                List<Student> students = new List<Student>();

                foreach (Registration registration in NextSessionRegistrations.Where(r => r.Student != null).OrderBy(r => r.Student.LastName))
                {
                    students.Add(registration.Student);
                }

                return students;
            }
        }

        [JsonIgnore]
        public Allocation NextSessionAllocation
        {
            get
            {
                return DB.Instance.Allocations.ToList()
                    .FirstOrDefault(a => a.CourseId == Id && a.IsNextSession);
            }
        }

        [JsonIgnore]
        public Teacher NextSessionTeacher
        {
            get
            {
                Allocation allocation = NextSessionAllocation;

                if (allocation == null)
                    return null;

                return allocation.Teacher;
            }
        }

        [JsonIgnore]
        public SelectList StudentsSelectList => SelectListUtilities<Student>.Convert(Students, "Caption");

        [JsonIgnore]
        public SelectList NextSessionStudentsToSelectList => SelectListUtilities<Student>.Convert(NextSessionStudents, "Caption");

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Code)
                && !string.IsNullOrWhiteSpace(Title)
                && Session >= 1
                && Session <= 6;
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

        public void UpdateRegistrations(List<int> selectedStudentsId)
        {
            DeleteNextSessionRegistrations();

            if (selectedStudentsId != null)
            {
                foreach (int studentId in selectedStudentsId)
                {
                    DB.Instance.Registrations.Add(new Registration
                    {
                        StudentId = studentId,
                        CourseId = Id,
                        Year = NextSession.Year
                    });
                }
            }
        }

        public void UpdateAllocation(int teacherId)
        {
            Allocation oldAllocation = NextSessionAllocation;

            if (oldAllocation != null)
                DB.Instance.Allocations.Delete(oldAllocation.Id);

            if (teacherId > 0)
            {
                DB.Instance.Allocations.Add(new Allocation
                {
                    TeacherId = teacherId,
                    CourseId = Id,
                    Year = NextSession.Year
                });
            }
        }
    }
}