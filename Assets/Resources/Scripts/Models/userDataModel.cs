namespace userDataModel.Models
{
    [System.Serializable]
    public class UserData
    {
        public string firstName;
        public string lastName;
        public string email;
        public string password;
        public string department;
        public string program;
        public string yearSection;
        public string profileImage;

        public UserData(string firstName, string lastName, string email, string password, string department, string program, string yearSection, string profileImage)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.password = password;
            this.department = department;
            this.program = program;
            this.yearSection = yearSection;
            this.profileImage = profileImage;
        }
    }
}
