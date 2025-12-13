namespace cadwiki.AC.DocProcessor
{
    public enum OpenResult
    {
        Opened,
        AlreadyOpen,
        Locked,
        Failed,
        Skipped,
        NotFound
    }

    public class OpenResultChecker
    {
        public static bool WasOpened(OpenResult res)
        {
            if (res == OpenResult.Opened || res == OpenResult.AlreadyOpen)
            {
                return true;
            }
            return false;
        }
    }
}
