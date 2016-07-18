namespace Tfs.WebHooks
{
    public static class StringExtensions
    {
        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }
    }
}