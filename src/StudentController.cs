
using CSharpFunctionalExtensions;

namespace DddEfCoreExample
{
    public sealed class StudentController
    {
        private readonly SchoolContext _context;
        private readonly StudentRepository _repository;


        public StudentController(SchoolContext context)
        {
            _context = context;
            _repository = new StudentRepository(context);
        }

        public string CheckStudentFavoriteCourse(long studentId, long courseId)
        {
            Student student = _context.Students.Find(studentId);
            if (student == null)
                return "Student not found";

            Course course = Course.FromId(courseId);
            if (course == null)
                return "Course not found";

            return student.FavoriteCourse == course ? "Yes" : "No";
        }

        public string EnrollStudent(long studentId, long courseId, Grade grade)
        {
            Student student = _repository.GetById(studentId);
            if (student == null)
                return "Student not found";

            Course course = Course.FromId(courseId);
            if (course == null)
                return "Course not found";

            student.EnrollIn(course, grade);

            _context.SaveChanges();

            return "OK";
        }

        public string DisenrollStudent(long studentId, long courseId)
        {
            Student student = _repository.GetById(studentId);
            if (student == null)
                return "Student not found";

            Course course = Course.FromId(courseId);
            if (course == null)
                return "Course not found";

            student.Disenroll(course);

            _context.SaveChanges();

            return "OK";
        }

        public string RegisterStudent(
            Name name, string email, long favoriteCourseId, Grade favoriteCourseGrade)
        {
            Course favoriteCourse = Course.FromId(favoriteCourseId);
            if (favoriteCourse == null)
                return "Course not found";

            Result<Email> result = Email.Create(email);
            if (result.IsFailure)
            {
                return result.Error;
            }

            var student = new Student(name, result.Value, favoriteCourse, favoriteCourseGrade);

            _repository.Save(student);
            _context.SaveChanges();

            return "OK";
        }

        public string EditPersonalInfo(
            long studentId, Name name, string email, long favoriteCourseId)
        {
            Student student = _repository.GetById(studentId);
            if (student == null)
                return "Student not found";

            Course favoriteCourse = Course.FromId(favoriteCourseId);
            if (favoriteCourse == null)
                return "Course not found";

            Result<Email> result = Email.Create(email);
            if (result.IsFailure)
            {
                return result.Error;
            }

            student.Name = name;
            student.Email = result.Value;
            student.FavoriteCourse = favoriteCourse;

            var studentEntityState = _context.Entry(student).State;
            var favoriteCourseEntityState = _context.Entry(student.FavoriteCourse).State;
            // var enrollmentEntityState = _context.Entry(student.Enrollments[0]).State;
            // var courseEntityState = _context.Entry(student.Enrollments[0].Course).State;

            _context.SaveChanges();

            return "OK";
        }
    }
}