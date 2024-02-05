namespace Check1st.Security;

public static class Constants
{
    public enum Role
    {
        Admin,
        Teacher
    }

    public static class Policy
    {
        public const string IsAdmin = "IsAdmin";
        public const string IsAdminOrTeacher = "IsAdminOrTeacher";
    }
}
