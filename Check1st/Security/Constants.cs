namespace Check1st.Security;

public static class Constants
{
    public enum Role
    {
        None = 0,
        Admin,
        Teacher
    }

    public static class Policy
    {
        public const string IsAdmin = "IsAdmin";
        public const string IsAdminOrTeacher = "IsAdminOrTeacher";
        public const string CanReadConsultation = "CanReadConsultation";
    }
}
