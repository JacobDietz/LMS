using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        [Fact]
        public void TestGetDepartments()
        {
            var db = MakeTinyDB();
            var ctrl = new CommonController(db);
            var result = ctrl.GetDepartments() as JsonResult;
            dynamic x = result.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        [Fact]
        public void TestCreateDepartment()
        {
            var db = MakeTinyDB();
            var ctrl = new AdministratorController(db);
            var result = ctrl.CreateDepartment("MATH", "Mathematics") as JsonResult;
            dynamic x = result.Value;
            Assert.True((bool)x.success);
            Assert.Equal(2, db.Departments.Count());
        }

        [Fact]
        public void TestCreateCourse()
        {
            var db = MakeTinyDB();
            var ctrl = new AdministratorController(db);
            var result = ctrl.CreateCourse("CS", 5530, "Database Systems") as JsonResult;
            dynamic x = result.Value;

            Assert.True((bool)x.success);
            Assert.Equal(1, db.Courses.Count());
        }

        [Fact]
        public void TestEnrollStudent()
        {
            var db = MakeClassDB();
            var ctrl = new StudentController(db);
            var result = ctrl.Enroll("CS", 5530, "Fall", 2025, "u0000001") as JsonResult;
            dynamic x = result.Value;

            Assert.True((bool)x.success);
            Assert.Equal(1, db.Enrolleds.Count());
        }

        [Fact]
        public void TestGetGPA()
        {
            var db = MakeTinyDB();
            db.Enrolleds.Add(new Enrolled { UId = "u0000001", ClassId = 1, Grade = "A" });
            db.Enrolleds.Add(new Enrolled { UId = "u0000001", ClassId = 2, Grade = "B+" });
            db.SaveChanges();

            var ctrl = new StudentController(db);
            var result = ctrl.GetGPA("u0000001") as JsonResult;
            dynamic x = result.Value;
            Assert.Equal(3.65, (double)x.gpa, 2);
        }

        [Fact]
        public void TestGradeSubmission()
        {
            var db = MakeGradeDB();
            var ctrl = new ProfessorController(db);
            var result = ctrl.GradeSubmission("CS", 5530, "Fall", 2025, "Homework", "HW1", "u0000001", 95) as JsonResult;
            dynamic x = result.Value;

            Assert.True((bool)x.success);
            Assert.Equal((uint)95, db.Submissions.First().Score);
            Assert.Equal("A", db.Enrolleds.First().Grade);
        }

        private LMSContext MakeTinyDB()
        {
            var options = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;
            var db = new LMSContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.Departments.Add(new Department
            {
                Subject = "CS",
                Name = "KSoC"
            });

            db.SaveChanges();
            return db;
        }

        private LMSContext MakeClassDB()
        {
            var db = MakeTinyDB();
            db.Courses.Add(new Course
            {
                Subject = "CS",
                Number = 5530,
                Name = "Database Systems"
            });

            db.Classes.Add(new Class
            {
                ClassId = 1,
                Subject = "CS",
                Number = 5530,
                Season = "Fall",
                Year = 2025,
                Location = "WEB 123",
                StartTime = new TimeOnly(9, 0, 0),
                EndTime = new TimeOnly(10, 15, 0),
                Professor = "p0000001"
            });

            db.SaveChanges();
            return db;
        }

        private LMSContext MakeGradeDB()
        {
            var db = MakeClassDB();
            db.AssignmentCategories.Add(new AssignmentCategory
            {
                Name = "Homework",
                ClassId = 1,
                Weight = 100
            });

            db.Assignments.Add(new Assignment
            {
                AssignmentId = 1,
                ClassId = 1,
                Name = "HW1",
                Points = 100,
                Due = DateTime.Now,
                Content = "Test assignment",
                CategoryName = "Homework"
            });

            db.Enrolleds.Add(new Enrolled
            {
                UId = "u0000001",
                ClassId = 1,
                Grade = "--"
            });

            db.Submissions.Add(new Submission
            {
                SubmissionId = 1,
                UId = "u0000001",
                AId = 1,
                Time = DateTime.Now,
                Score = 0,
                Contents = "My submission"
            });

            db.SaveChanges();
            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            return new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();
        }
    }
}