using System.Collections.Generic;
using System.Linq;

namespace DddEfCoreExample
{
    public class Student : Entity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public virtual Course FavoriteCourse { get; private set; }

        private readonly List<Enrollment> _enrollments = new List<Enrollment>();
        public virtual IReadOnlyList<Enrollment> Enrollments => _enrollments.ToList();
        
        protected Student()
        {
        }

        public Student(string name, string email, Course favoriteCourse, Grade favoriteCourseGrade)
            : this()
        {
            Name = name;
            Email = email;
            FavoriteCourse = favoriteCourse;

            EnrollIn(favoriteCourse, favoriteCourseGrade);
        }

        public string EnrollIn(Course course, Grade grade)
        {
            if (Enrollments.Any(x => x.Course == course))
                return $"Already enrolled in course '{course.Name}'";
            
            var enrollment = new Enrollment(course, this, grade);
            _enrollments.Add(enrollment);

            return "OK";
        }

        public void Disenroll(Course course)
        {
            Enrollment enrollment = _enrollments.FirstOrDefault(x => x.Course == course);

            if (enrollment == null)
                return;

            _enrollments.Remove(enrollment);
        }
    }
}