using DAL;
using static Controllers.AccessControl;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    [UserAccess(Access.View)]
    public class StudentsController : Controller
    {
        public ActionResult Index()
        {
            if (Session["SearchString"] == null)
                Session["SearchString"] = "";

            if (Session["SelectedStudentYear"] == null)
                Session["SelectedStudentYear"] = 0;

            Session["StudentsYearsList"] = DB.Instance.Students.ToList()
                .Select(s => s.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View();
        }

        public ActionResult GetStudents()
        {
            string searchString = (Session["SearchString"] ?? "").ToString().ToLower();
            int selectedYear = (int)(Session["SelectedStudentYear"] ?? 0);

            List<Student> students = DB.Instance.Students.ToList();

            Session["StudentsYearsList"] = students
                .Select(s => s.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                students = students
                    .Where(s =>
                        s.FirstName.ToLower().Contains(searchString) ||
                        s.LastName.ToLower().Contains(searchString) ||
                        s.Code.ToLower().Contains(searchString) ||
                        s.Email.ToLower().Contains(searchString) ||
                        s.Phone.ToLower().Contains(searchString))
                    .ToList();
            }

            if (selectedYear > 0)
            {
                students = students
                    .Where(s => s.Year == selectedYear)
                    .ToList();
            }

            students = students
                .OrderByDescending(s => s.Year)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            return PartialView(students);
        }

        public ActionResult SetSearch(string searchString, int selectedYear = 0)
        {
            Session["SearchString"] = searchString ?? "";
            Session["SelectedStudentYear"] = selectedYear;

            return new EmptyResult();
        }

        public ActionResult Details(int id)
        {
            Student student = DB.Instance.Students.Get(id);

            if (student == null)
                return RedirectToAction("Index");

            Session["CurrentStudentId"] = id;

            return View(student);
        }

        public ActionResult GetStudentDetails()
        {
            if (Session["CurrentStudentId"] == null)
                return new EmptyResult();

            int id = (int)Session["CurrentStudentId"];
            Student student = DB.Instance.Students.Get(id);

            if (student == null)
                return new EmptyResult();

            return PartialView(student);
        }

        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            Student student = new Student
            {
                BirthDate = DateTime.Now.AddYears(-18),
                Code = Student.GenerateUniqueCode()
            };

            ViewBag.Creating = true;

            return View(student);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Create(Student student)
        {
            student.Code = Student.GenerateUniqueCode();

            if (student.IsValid())
            {
                DB.Instance.Students.Add(student);
                return RedirectToAction("Details", new { id = student.Id });
            }

            ViewBag.Creating = true;

            return View(student);
        }

        [UserAccess(Access.Write)]
        public ActionResult Edit(int id)
        {
            Student student = DB.Instance.Students.Get(id);

            if (student == null)
                return RedirectToAction("Index");

            Session["CurrentStudentId"] = student.Id;
            Session["CurrentStudentCode"] = student.Code;

            ViewBag.Creating = false;

            ViewBag.Registrations = student.NextSessionCoursesToSelectList;

            List<Course> nextSessionCourses = DB.Instance.Courses.ToList()
                .Where(c => NextSession.ValidSessions.Contains(c.Session))
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();

            ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

            ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

            return View(student);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Student student, List<int> selectedCoursesId)
        {
            if (Session["CurrentStudentId"] == null || Session["CurrentStudentCode"] == null)
                return RedirectToAction("Index");

            student.Id = (int)Session["CurrentStudentId"];
            student.Code = (string)Session["CurrentStudentCode"];

            if (student.IsValid())
            {
                DB.Instance.Students.Update(student);
                student.UpdateRegistrations(selectedCoursesId);

                return RedirectToAction("Details", new { id = student.Id });
            }

            ViewBag.Creating = false;

            ViewBag.Registrations = student.NextSessionCoursesToSelectList;
            List<Course> nextSessionCourses = DB.Instance.Courses.ToList()
                .Where(c => NextSession.ValidSessions.Contains(c.Session))
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();

            ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");


            ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

            ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

            return View(student);
        }

        [UserAccess(Access.Write)]
        public ActionResult Delete(int id)
        {
            Student student = DB.Instance.Students.Get(id);

            if (student != null)
            {
                student.DeleteAllRegistrations();
                DB.Instance.Students.Delete(id);
            }

            return RedirectToAction("Index");
        }
        public ActionResult GetStudentInfo()
        {
            if (Session["CurrentStudentId"] == null)
                return new EmptyResult();

            int id = (int)Session["CurrentStudentId"];
            Student student = DB.Instance.Students.Get(id);

            if (student == null)
                return new EmptyResult();

            return PartialView(student);
        }

        public ActionResult GetStudentRegistrations()
        {
            if (Session["CurrentStudentId"] == null)
                return new EmptyResult();

            int id = (int)Session["CurrentStudentId"];
            Student student = DB.Instance.Students.Get(id);

            if (student == null)
                return new EmptyResult();

            return PartialView(student);
        }
        public ActionResult About()
        {
            ViewBag.PageTitle = "À propos";
            ViewBag.Title = "À propos";
            return View("~/Views/Shared/About.cshtml");
        }

    }
}