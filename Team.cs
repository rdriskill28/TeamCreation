using System.Collections.Generic;


namespace TeamCreation
{
    public class Team
    {
        public Dictionary<string, float> Player { get; set; }
        public float TeamRating { get; set; }
        public int Id { get; set; }

        public Team()
        {
            Player = new Dictionary<string, float>();
            TeamRating = 0.0F;
        }
    }
}
