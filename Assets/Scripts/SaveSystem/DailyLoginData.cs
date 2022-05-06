using System;

namespace AnotherMatch3.SaveSystem
{
    [Serializable]
    public class DailyLoginData
    {
        public DateTime lastLoginTime;
        public int streak;
    }
}