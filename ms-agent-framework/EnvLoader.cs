namespace ms_agent_framework
{
    public static class EnvLoader
    {
        /// <summary>
        /// Loads environment variables from a .env file if present.
        /// If no path is provided, DotNetEnv will attempt to locate a .env in the project root.
        /// Swallows any exceptions so missing .env files won't crash the app.
        /// </summary>
        public static void Load(string? path = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    DotNetEnv.Env.Load();
                }
                else
                {
                    DotNetEnv.Env.Load(path);
                }
            }
            catch
            {
                // ignore missing or malformed .env
            }
        }
    }
}
