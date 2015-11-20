namespace NGraphics
{
    public static class Platforms
    {
        static readonly IPlatform nulll = new NullPlatform();
        private static IPlatform current = null;

        public static IPlatform Null => nulll;

        public static IPlatform Current
        {
            get
            {
                if (current == null)
                {
#if MAC
                    current = new ApplePlatform ();
#elif __IOS__
                    current = new ApplePlatform ();
#elif __ANDROID__
                    current = new AndroidPlatform ();
#elif NETFX_CORE
                    current = new WinRTPlatform ();
#else
                    current = new SystemDrawingPlatform();
#endif
                }

                return current;
            }
        }
    }
}
