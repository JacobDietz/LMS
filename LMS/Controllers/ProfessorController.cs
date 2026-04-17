using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            // get all enrolled students in that class
            var query = from e in db.Enrolleds
                        where e.ClassId == cl.ClassId
                        join s in db.Students on e.UId equals s.UId
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade ?? "--"
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class,
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            if (cl == null) return Json(null);

            // get all assignments in that class, filtered by category if provided
            var query = from a in db.Assignments
                        where a.ClassId == cl.ClassId &&
                              (category == null || a.CategoryName == category)
                        select new
                        {
                            aname = a.Name,
                            cname = a.CategoryName,
                            due = a.Due,
                            submissions = a.Submissions.Count
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            if (cl == null) return Json(null);

            // get all assignment categories for that class
            var query = from ac in db.AssignmentCategories
                        where ac.ClassId == cl.ClassId
                        select new { name = ac.Name, weight = ac.Weight };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            if (cl == null) return Json(new { success = false });

            // check if the category already exists in this class
            var existing = db.AssignmentCategories.FirstOrDefault(ac =>
                ac.ClassId == cl.ClassId &&
                ac.Name == category);

            if (existing != null) return Json(new { success = false });

            // create and save the new category
            db.AssignmentCategories.Add(new AssignmentCategory
            {
                Name = category,
                ClassId = cl.ClassId,
                Weight = (uint)catweight
            });
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            if (cl == null) return Json(new { success = false });

            // check if an assignment with this name already exists in the class
            var existing = db.Assignments.FirstOrDefault(a =>
                a.ClassId == cl.ClassId &&
                a.Name == asgname);

            if (existing != null) return Json(new { success = false });

            // create and save the new assignment
            db.Assignments.Add(new Assignment
            {
                ClassId = cl.ClassId,
                Name = asgname,
                Points = (uint)asgpoints,
                Due = asgdue,
                Content = asgcontents,
                CategoryName = category
            });
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        ///
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            // get the class
            var cl = db.Classes.FirstOrDefault(c =>
                c.Subject == subject &&
                c.Number == (uint)num &&
                c.Season == season &&
                c.Year == (uint)year);

            // get the assignment
            var assignment = db.Assignments.FirstOrDefault(a =>
                a.ClassId == cl.ClassId &&
                a.CategoryName == category &&
                a.Name == asgname);

            // get all submissions with student info
            var query = from sub in db.Submissions
                        where sub.AId == assignment.AssignmentId
                        join s in db.Students on sub.UId equals s.UId
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            time = sub.Time,
                            score = sub.Score
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var cl = db.Classes.FirstOrDefault(c => c.Subject == subject && c.Number == (uint)num && c.Season == season && c.Year == (uint)year);
            if (cl == null) return Json(new { success = false });

            var assignment = db.Assignments.FirstOrDefault(a => a.ClassId == cl.ClassId && a.CategoryName == category && a.Name == asgname);
            if (assignment == null) return Json(new { success = false });

            var submission = db.Submissions.FirstOrDefault(s => s.AId == assignment.AssignmentId && s.UId == uid);
            if (submission == null) return Json(new { success = false });

            submission.Score = (uint)score;
            db.SaveChanges();

            UpdateStudentGrade(uid, cl.ClassId);
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        where c.Professor == uid
                        join co in db.Courses on new { c.Subject, c.Number } equals new { co.Subject, co.Number }
                        select new
                        {
                            subject = c.Subject,
                            number = c.Number,
                            name = co.Name,
                            season = c.Season,
                            year = c.Year
                        };
            return Json(query.ToArray());
        }


        private void UpdateStudentGrade(string uid, uint classId)
        {
            var categories = db.AssignmentCategories.Where(ac => ac.ClassId == classId).ToList();
            double earnedScore = 0;
            double totalWeight = 0;

            foreach (var cat in categories)
            {
                var assignments = db.Assignments.Where(a => a.ClassId == classId && a.CategoryName == cat.Name).ToList();
                if (!assignments.Any()) continue;

                double totalPoints = assignments.Sum(a => (double)a.Points);
                if (totalPoints == 0) continue;

                double earned = 0;
                foreach (var asg in assignments)
                {
                    var sub = db.Submissions.FirstOrDefault(s => s.AId == asg.AssignmentId && s.UId == uid);
                    earned += sub?.Score ?? 0;
                }

                earnedScore += (earned / totalPoints) * cat.Weight;
                totalWeight += cat.Weight;
            }

            if (totalWeight == 0) return;

            double pct = earnedScore / totalWeight * 100;
            string letterGrade = pct >= 93 ? "A"  : pct >= 90 ? "A-" :
                                 pct >= 87 ? "B+" : pct >= 83 ? "B"  :
                                 pct >= 80 ? "B-" : pct >= 77 ? "C+" :
                                 pct >= 73 ? "C"  : pct >= 70 ? "C-" :
                                 pct >= 67 ? "D+" : pct >= 63 ? "D"  :
                                 pct >= 60 ? "D-" : "E";

            var enrolled = db.Enrolleds.FirstOrDefault(e => e.UId == uid && e.ClassId == classId);
            if (enrolled != null)
            {
                enrolled.Grade = letterGrade;
                db.SaveChanges();
            }
        }


        /*******End code to modify********/
    }
}

