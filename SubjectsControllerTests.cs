using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Controllers;
using OnlineSchool.Models;

namespace OnlineSchool.Tests
{
    public class SubjectsControllerTests
    {
        private SchoolContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new SchoolContext(options);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoSubjects()
        {
            var context = GetInMemoryContext();
            var controller = new SubjectsController(context);
            var result = await controller.GetAll();
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenValidSubject()
        {
            var context = GetInMemoryContext();
            var controller = new SubjectsController(context);
            var subject = new Subject { Name = "Математика", Description = "Базова математика" };
            var result = await controller.Create(subject);
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(1, await context.Subjects.CountAsync());
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenDuplicateName()
        {
            var context = GetInMemoryContext();
            context.Subjects.Add(new Subject { Name = "Математика" });
            await context.SaveChangesAsync();
            var controller = new SubjectsController(context);
            var result = await controller.Create(new Subject { Name = "Mathematics" });
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenEmptyName()
        {
            var context = GetInMemoryContext();
            var controller = new SubjectsController(context);
            var result = await controller.Create(new Subject { Name = "  " });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_ReturnsSubject_WhenExists()
        {
            var context = GetInMemoryContext();
            context.Subjects.Add(new Subject { Id = 1, Name = "Фізика" });
            await context.SaveChangesAsync();
            var controller = new SubjectsController(context);
            var result = await controller.GetById(1);
            Assert.Equal("Physics", result.Value.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
        {
            var context = GetInMemoryContext();
            var controller = new SubjectsController(context);
            var result = await controller.GetById(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenNoLessons()
        {
            var context = GetInMemoryContext();
            context.Subjects.Add(new Subject { Id = 1, Name = "Chemistry" });
            await context.SaveChangesAsync();
            var controller = new SubjectsController(context);
            var result = await controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await context.Subjects.CountAsync());
        }

        [Fact]
        public async Task Delete_ReturnsConflict_WhenHasLessons()
        {
            var context = GetInMemoryContext();
            var subject = new Subject { Id = 1, Name = "Біологія" };
            var teacher = new Teacher { Id = 1, FullName = "Строценко" };
            var student = new Student { Id = 1, FullName = "Малява" };
            context.Subjects.Add(subject);
            context.Teachers.Add(teacher);
            context.Students.Add(student);
            context.Lessons.Add(new Lesson
            {
                Id = 1, SubjectId = 1, TeacherId = 1, StudentId = 1,
                Date = DateTime.Now, Time = TimeSpan.FromHours(10)
            });
            await context.SaveChangesAsync();
            var controller = new SubjectsController(context);

            var result = await controller.Delete(1);

            Assert.IsType<ConflictObjectResult>(result);
        }
    }
}