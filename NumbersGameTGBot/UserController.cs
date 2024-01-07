internal class UserController
{
    readonly NumberGameTGBotContext db = new();

    private User? GetUser(long userId)
    {
        var user = db.Users.Find(userId);

        return user;
    }

    private void CreateUser(long userId)
    {
        User newUser = new() { Id = userId, Streak = 1, LastMessageDate = DateTime.Now.ToUniversalTime() };

        db.Add(newUser);
        db.SaveChanges();
    }

    public bool UpdateUserStreak(long userId, int streak, DateTime msgTime)
    {
        var user = GetUser(userId);

        // If user doesn't exist
        if (user is null)
        {
            if (streak != 1)
                return false;

            CreateUser(userId);

            return true;
        }

        if (user.Streak + 1 != streak)
            return false;

        if (user.Streak != 0)
        {
            if (user.LastMessageDate.AddDays(1).Day != DateTime.Now.ToUniversalTime().Day)
                return false;
        }

        var now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        var minTime = now.AddHours(TimePeriod.MinTime.Hour).AddMinutes(TimePeriod.MinTime.Minute).AddSeconds(TimePeriod.MinTime.Second);
        var maxTime = now.AddHours(TimePeriod.MaxTime.Hour).AddMinutes(TimePeriod.MaxTime.Minute).AddSeconds(TimePeriod.MaxTime.Second);
        var msgTimeOffsetted = msgTime + TimePeriod.offset;

        if (!(minTime <= msgTimeOffsetted && msgTimeOffsetted <= maxTime))
            return false;

        user.Streak++;
        user.LastMessageDate = msgTime;
        db?.Update(user);
        db?.SaveChanges();

        return true;
    }
}
