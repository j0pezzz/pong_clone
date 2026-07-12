namespace Project.Internal.Utility
{
    public static class TeamUtility
    {
        public static Team GetOppositeTeam(this Team team) => team == Team.Team1 ? Team.Team2 : Team.Team1;
    }
}