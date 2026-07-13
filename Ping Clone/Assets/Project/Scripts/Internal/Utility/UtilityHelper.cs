namespace Project.Internal.Utility
{
    public class UtilityHelper
    {
        /// <summary>
        /// Checks which player won based on their points.
        /// </summary>
        /// <param name="p1Points">Player1 points.</param>
        /// <param name="p2Points">Player2 points.</param>
        /// <param name="rPoints">Required points.</param>
        /// <returns>The winning 'Team'.</returns>
        public static Team DetermineWinner(int p1Points, int p2Points, int rPoints)
        {
            if (p1Points >= rPoints)
            {
                return Team.Team1;
            }

            if (p2Points >= rPoints)
            {
                return Team.Team2;
            }

            return Team.None;
        }
    }
}