using DAL;
using static Controllers.AccessControl;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Controllers
{
    [UserAccess(Access.View)]
    public class CoursesController : Controller
    {
        public ActionResult Index()
        {
            if (Session["CourseSearchString"] == null)
                Session["CourseSearchString"] = "";

            return View();
        }

        public ActionResult GetCourses()
        {
            string searchString = (Session["CourseSearchString"] ?? "").ToString().ToLower();

            List<Course> courses = DB.Instance.Courses.ToList();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                courses = courses
                    .Where(c =>
                        c.Code.ToLower().Contains(searchString) ||
                        c.Title.ToLower().Contains(searchString) ||
                        c.Session.ToString().Contains(searchString))
                    .ToList();
            }

            courses = courses
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();

            return PartialView(courses);
        }

        public ActionResult SetSearch(string searchString)
        {
            Session["CourseSearchString"] = searchString ?? "";
            return new EmptyResult();
        }

        public ActionResult Details(int id)
        {
            Course course = DB.Instance.Courses.Get(id);

            if (course == null)
                return RedirectToAction("Index");

            Session["CurrentCourseId"] = id;

            return View(course);
        }

        public ActionResult GetCourseDetails()
        {
            if (Session["CurrentCourseId"] == null)
                return new EmptyResult();

            int id = (int)Session["CurrentCourseId"];
            Course course = DB.Instance.Courses.Get(id);

            if (course == null)
                return new EmptyResult();

            return PartialView(course);
        }

        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            Course course = new Course
            {
                Session = 1
            };

            ViewBag.Creating = true;

            return View(course);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Create(Course course)
        {
            if (course.IsValid())
            {
                DB.Instance.Courses.Add(course);
                return RedirectToAction("Details", new { id = course.Id });
            }

            ViewBag.Creating = true;

            return View(course);
        }

        [UserAccess(Access.Write)]
        public ActionResult Edit(int id)
        {
            Course course = DB.Instance.Courses.Get(id);

            if (course == null)
                return RedirectToAction("Index");

            Session["CurrentCourseId"] = course.Id;

            ViewBag.Creating = false;

            ViewBag.Registrations = course.NextSessionStudentsToSelectList;

            List<Student> students = DB.Instance.Students.ToList()
                .OrderByDescending(s => s.Year)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            ViewBag.Students = SelectListUtilities<Student>.Convert(students, "Caption");

            List<Teacher> teachers = DB.Instance.Teachers.ToList()
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToList();

            int selectedTeacherId = course.NextSessionTeacher != null ? course.NextSessionTeacher.Id : 0;

            ViewBag.Teachers = new SelectList(teachers, "Id", "Caption", selectedTeacherId);

            return View(course);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Course course, List<int> selectedStudentsId, int teacherId = 0)
        {
            if (Session["CurrentCourseId"] == null)
                return RedirectToAction("Index");

            course.Id = (int)Session["CurrentCourseId"];

            if (course.IsValid())
            {
                DB.Instance.Courses.Update(course);
                course.UpdateRegistrations(selectedStudentsId);
                course.UpdateAllocation(teacherId);

                return RedirectToAction("Details", new { id = course.Id });
            }

            ViewBag.Creating = false;

            ViewBag.Registrations = course.NextSessionStudentsToSelectList;

            List<Student> students = DB.Instance.Students.ToList()
                .OrderByDescending(s => s.Year)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            ViewBag.Students = SelectListUtilities<Student>.Convert(students, "Caption");

            ViewBag.Students = SelectListUtilities<Student>.Convert(students, "Caption");

            List<Teacher> teachers = DB.Instance.Teachers.ToList()
                .OrderByDescending(t => t.StartDate)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToList();

            ViewBag.Teachers = new SelectList(teachers, "Id", "Caption", teacherId);

            return View(course);
        }

        [UserAccess(Access.Write)]
        public ActionResult Delete(int id)
        {
            Course course = DB.Instance.Courses.Get(id);

            if (course != null)
            {
                course.DeleteAllRegistrations();

                List<Allocation> allocations = DB.Instance.Allocations.ToList()
                    .Where(a => a.CourseId == id)
                    .ToList();

                foreach (Allocation allocation in allocations)
                {
                    DB.Instance.Allocations.Delete(allocation.Id);
                }

                DB.Instance.Courses.Delete(id);
            }

            return RedirectToAction("Index");
        }
        public ActionResult About()
        {
            ViewBag.PageTitle = "À propos";
            ViewBag.Title = "À propos";
            return View("~/Views/Shared/About.cshtml");
        }
    }
}