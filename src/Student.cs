namespace DddEfCoreExample
{
    public class Student
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public long FavoriteCourseId { get; private set; }

        public Student(string name, string email, long favoriteCourseId)
        {
            Name = name;
            Email = email;
            FavoriteCourseId = favoriteCourseId;
        }
    }
}