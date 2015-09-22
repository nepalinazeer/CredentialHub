using System;


namespace CredentialHub
{
    [Serializable]
    public class Credential
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public  string Password { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
    }
}
