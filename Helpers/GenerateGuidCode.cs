namespace BackendChat.Helpers
{
    public static class GenerateGuidCode
    {
        public static string GenerateGuidToken() => Guid.NewGuid().ToString();
    }
}
