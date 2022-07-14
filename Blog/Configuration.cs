namespace Blog
{
    public static class Configuration
    {
        // TOKEN - JWT é o formato - Json Web Token
        public static string JwtKey { get; set; } = "46qfSH7lcE62dNWliLK9yg==";
        public static string ApiKeyName { get; set; } = "api_key";
        public static string ApiKey { get; set; } = "curso_api_r6coPeJfz0abvsPmZMvjWw==";

        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
