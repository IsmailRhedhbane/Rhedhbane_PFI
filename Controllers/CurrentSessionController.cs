using static Controllers.AccessControl;
using Models;
using System;
using System.Web.Mvc;

namespace Controllers
{
    [UserAccess(Access.Write)]
    public class CurrentSessionController : Controller
    {
        public ActionResult Edit()
        {
            ViewBag.Year = NextSession.Year;
            ViewBag.Session = NextSession.ValidSessions.Contains(1) ? "Automne" : "Hiver";

            return View();
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(int year, string session)
        {
            NextSession.CurrentDate = new DateTime(year, session == "Automne" ? 8 : 1, 15);

            return RedirectToAction("Index", "Students");
        }
    }
}