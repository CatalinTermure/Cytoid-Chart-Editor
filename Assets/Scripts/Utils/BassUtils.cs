using ManagedBass;

namespace CCE.Utils
{
    public static class BassUtils
    {
        public static void PrintLastError()
        {
            if (Bass.LastError == Errors.OK)
            {
                return;
            }

            throw new BassException(Bass.LastError);
        }
    }
}