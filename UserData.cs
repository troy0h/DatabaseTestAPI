namespace DatabaseTestAPI
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConf { get; set; }
        public string UserID { get; set; }
        public string PassHash { get; set; }
        public string Salt { get; set; }
    }
}
