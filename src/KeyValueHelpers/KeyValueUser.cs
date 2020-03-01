namespace KeyValueHelpers
{
    public class KeyValueUser
    {
        public string Name { get; private set; }
        public string Password { get; private set; }

        public string Access { get; private set; }

        private KeyValueUser() { }

        public static KeyValueUser New(string name, string password, string access)
        {
            var result = new KeyValueUser()
            {
                Name = name,
                Password = password,
                Access = access
            };

            return result;
        }
    }
}
