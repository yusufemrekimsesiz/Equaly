namespace Equaly.Services
{
    public static class AppSession
    {
        private const string CurrentGroupIdKey = "CurrentGroupId";

        public static int CurrentGroupId
        {
            get => Preferences.Default.Get(CurrentGroupIdKey, 0);
            set => Preferences.Default.Set(CurrentGroupIdKey, value);
        }

        public static bool HasSelectedGroup => CurrentGroupId > 0;
    }
}