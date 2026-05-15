using DAL;
using static Controllers.AccessControl;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web;

namespace Controllers
{

    [UserAccess(Access.View)]
    public class TeachersController : Controller
    {
        public ActionResult Index()
        {
            if (Session["TeacherSearchString"] == null)
                Session["TeacherSearchString"] = "";

            return View();
        }

        public ActionResult GetTeachers()
        {
            string searchString = (Session["TeacherSearchString"] ?? "").ToString().ToLower();

            List<Teacher> teachers = DB.Instance.Teachers.ToList();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                teachers = teachers
                    .Where(t =>
                        (t.FirstName ?? "").ToLower().Contains(searchString) ||
                        (t.LastName ?? "").ToLower().Contains(searchString) ||
                        (t.Code ?? "").ToLower().Contains(searchString) ||
                        (t.Phone ?? "").ToLower().Contains(searchString))
                    .ToList();
            }

            teachers = teachers
                .OrderBy(t => t.LastName ?? "")
                .ThenBy(t => t.FirstName ?? "")
                .ToList();

            return PartialView(teachers);
        }

        public ActionResult SetSearch(string searchString)
        {
            Session["TeacherSearchString"] = searchString ?? "";
            return new EmptyResult();
        }

        public ActionResult Details(int id)
        {
            Teacher teacher = DB.Instance.Teachers.Get(id);

            if (teacher == null)
                return RedirectToAction("Index");

            Session["CurrentTeacherId"] = id;

            return View(teacher);
        }

        public ActionResult GetTeacherDetails()
        {
            if (Session["CurrentTeacherId"] == null)
                return new EmptyResult();

            int id = (int)Session["CurrentTeacherId"];
            Teacher teacher = DB.Instance.Teachers.Get(id);

            if (teacher == null)
                return new EmptyResult();

            return PartialView(teacher);
        }

        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            Teacher teacher = new Teacher
            {
                Code = Teacher.GenerateUniqueCode(),
                StartDate = DateTime.Now,
                Avatar = "/App_Assets/teachers/no_avatar.png"
            };

            ViewBag.Creating = true;

            return View(teacher);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Create(Teacher teacher, HttpPostedFileBase AvatarFile)
        {
            teacher.Code = Teacher.GenerateUniqueCode();

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                teacher.Avatar = SaveTeacherAvatar(AvatarFile);
            }

            if (string.IsNullOrWhiteSpace(teacher.Avatar))
                teacher.Avatar = "/App_Assets/Teachers/no_avatar.png";

            if (teacher.IsValid())
            {
                DB.Instance.Teachers.Add(teacher);
                return RedirectToAction("Details", new { id = teacher.Id });
            }

            ViewBag.Creating = true;

            return View(teacher);
        }

        [UserAccess(Access.Write)]
        public ActionResult Edit(int id)
        {
            Teacher teacher = DB.Instance.Teachers.Get(id);

            if (teacher == null)
                return RedirectToAction("Index");

            Session["CurrentTeacherId"] = teacher.Id;
            Session["CurrentTeacherCode"] = teacher.Code;

            ViewBag.Creating = false;

            PrepareAllocationSelectLists(teacher);

            return View(teacher);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Teacher teacher, List<int> selectedCoursesId, HttpPostedFileBase AvatarFile)
        {
            if (Session["CurrentTeacherId"] == null || Session["CurrentTeacherCode"] == null)
                return RedirectToAction("Index");

            teacher.Id = (int)Session["CurrentTeacherId"];
            teacher.Code = (string)Session["CurrentTeacherCode"];

            Teacher oldTeacher = DB.Instance.Teachers.Get(teacher.Id);

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                teacher.Avatar = SaveTeacherAvatar(AvatarFile);
            }
            else if (oldTeacher != null)
            {
                teacher.Avatar = oldTeacher.Avatar;
            }

            if (string.IsNullOrWhiteSpace(teacher.Avatar))
                teacher.Avatar = "/App_Assets/Teachers/no_avatar.png";

            if (!teacher.IsValid())
            {
                TempData["Alert"] = "Le prof n'est pas valide.";
                ViewBag.Creating = false;
                PrepareAllocationSelectLists(teacher);
                return View(teacher);
            }

            DB.Instance.Teachers.Update(teacher);
            teacher.UpdateAllocations(selectedCoursesId);

            return RedirectToAction("Details", new { id = teacher.Id });
        }

        private void PrepareAllocationSelectLists(Teacher teacher)
        {
            ViewBag.Allocations = teacher.NextSessionCoursesToSelectList;

            List<int> coursesAlreadyAllocatedToOtherTeachers = DB.Instance.Allocations.ToList()
                .Where(a =>
                    a.Year == NextSession.Year &&
                    a.TeacherId != teacher.Id &&
                    a.Course != null &&
                    NextSession.ValidSessions.Contains(a.Course.Session))
                .Select(a => a.CourseId)
                .ToList();

            List<Course> availableCourses = DB.Instance.Courses.ToList()
                .Where(c =>
                    NextSession.ValidSessions.Contains(c.Session) &&
                    !coursesAlreadyAllocatedToOtherTeachers.Contains(c.Id))
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();

            ViewBag.Courses = SelectListUtilities<Course>.Convert(availableCourses, "Caption");
        }

        [UserAccess(Access.Write)]
        public ActionResult Delete(int id)
        {
            Teacher teacher = DB.Instance.Teachers.Get(id);

            if (teacher != null)
            {
                teacher.DeleteAllAllocations();
                DB.Instance.Teachers.Delete(id);
            }

            return RedirectToAction("Index");
        }
        private string SaveTeacherAvatar(HttpPostedFileBase avatarFile)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            string extension = Path.GetExtension(avatarFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return "/App_Assets/Teachers/no_avatar.png";

            string fileName = Guid.NewGuid().ToString() + extension;

            string folderPath = Server.MapPath("~/App_Assets/Teachers/");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);

            avatarFile.SaveAs(fullPath);

            return "/App_Assets/Teachers/" + fileName;
        }
        public ActionResult About()
        {
            ViewBag.PageTitle = "À propos";
            ViewBag.Title = "À propos";
            return View("~/Views/Shared/About.cshtml");
        }
    }
}