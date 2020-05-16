namespace DddEfCoreExample
{
    public class StudentRepository
    {
        private readonly SchoolContext _context;

        public StudentRepository(SchoolContext context)
        {
            _context = context;
        }

        public Student GetById(long studentId)
        {
            Student student = _context.Students.Find(studentId);

            if (student == null)
                return null;

            _context.Entry(student).Collection(x => x.Enrollments).Load();

            return student;
        }

        public void Save(Student student)
        {
            _context.Students.Attach(student);
            // db context.Attach --> sets the entity state to added only for entities where id is 0.
            // Enties with id other than 0 are marked unchaged.

            // db context.Update --> sets the entity state to added only for entities where id is 0.
            // Enties with id other than 0 are marked modified. (not recommended)

            // db context.Add --> sets all entity states to added.

            //  --> always prefer Attach over update or add.

            var studentEntityState = _context.Entry(student).State;
            var favoriteCourseEntityState = _context.Entry(student.FavoriteCourse).State;
            // var enrollmentEntityState = _context.Entry(student.Enrollments[0]).State;
            // var courseEntityState = _context.Entry(student.Enrollments[0].Course).State;
            // There are 5 entity states: detached (= not tracked), unchanged, deleted, modified & added.
        }
    }
}