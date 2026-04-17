using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        where e.UId == uid
                        join c in db.Classes on e.ClassId equals c.ClassId
                        join co in db.Courses on new { c.Subject, c.Number } equals new { co.Subject, co.Number }
                        select new
                        {
                            subject = c.Subject,
                            number = c.Number,
                            name = co.Name,
                            season = c.Season,
                            year = c.Year,
                            grade = e.Grade ?? "--"
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var cl = db.Classes.FirstOrDefault(c => c.Subject == subject && c.Number == (uint)num && c.Season == season && c.Year == (uint)year);
            if (cl == null) return Json(null);

            var query =
                from a in db.Assignments
                where a.ClassId == cl.ClassId
                join s in db.Submissions
                on new { A = a.AssignmentId, B = uid } equals new { A = s.AId, B = s.UId }
                into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    aname = a.Name,
                    cname = a.CategoryName,
                    due = a.Due,
                    score = j == null ? (uint?)null : j.Score
                };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var cl = db.Classes.FirstOrDefault(c => c.Subject == subject && c.Number == (uint)num && c.Season == season && c.Year == (uint)year);
            if (cl == null) return Json(new { success = false });

            var assignment = db.Assignments.FirstOrDefault(a => a.ClassId == cl.ClassId && a.CategoryName == category && a.Name == asgname);
            if (assignment == null) return Json(new { success = false });

            var existing = db.Submissions.FirstOrDefault(s => s.AId == assignment.AssignmentId && s.UId == uid);
            if (existing != null)
            {
                existing.Contents = contents;
                existing.Time = DateTime.Now;
            }
            else
            {
                db.Submissions.Add(new Submission
                {
                    UId = uid,
                    AId = assignment.AssignmentId,
                    Time = DateTime.Now,
                    Score = 0,
                    Contents = contents
                });
            }
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}.
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var cl = db.Classes.FirstOrDefault(c => c.Subject == subject && c.Number == (uint)num && c.Season == season && c.Year == (uint)year);
            if (cl == null) return Json(new { success = false });

            if (db.Enrolleds.Any(e => e.UId == uid && e.ClassId == cl.ClassId))
                return Json(new { success = false });

            db.Enrolleds.Add(new Enrolled { UId = uid, ClassId = cl.ClassId, Grade = "--" });
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var grades = db.Enrolleds
                .Where(e => e.UId == uid && e.Grade != null && e.Grade != "--")
                .Select(e => e.Grade)
                .ToList();

            if (!grades.Any())
                return Json(new { gpa = 0.0 });

            double gpa = grades.Average(g => GradeToPoints(g!));
            return Json(new { gpa });
        }

        private static double GradeToPoints(string grade) => grade switch
        {
            "A"  => 4.0,
            "A-" => 3.7,
            "B+" => 3.3,
            "B"  => 3.0,
            "B-" => 2.7,
            "C+" => 2.3,
            "C"  => 2.0,
            "C-" => 1.7,
            "D+" => 1.3,
            "D"  => 1.0,
            "D-" => 0.7,
            _    => 0.0
        };
                
        /*******End code to modify********/

    }
}

