using LabExtended.API;

using UnityEngine;

namespace SecretLabAPI.Voting.API
{
    /// <summary>
    /// Represents the state of a voting session, including available options and votes cast by players.
    /// </summary>
    public class VoteInfo
    {
        /// <summary>
        /// Gets or sets the player who started the vote.
        /// </summary>
        public ExPlayer Starter { get; set; }

        /// <summary>
        /// Gets the collection of available vote options.
        /// </summary>
        public List<VoteOption> Options { get; } = new();

        /// <summary>
        /// Gets the collection of votes cast by players.
        /// </summary>
        /// <remarks>Each entry in the dictionary maps an individual player to their selected vote option.
        /// The collection is read-only; to modify votes, use the appropriate methods provided by the class.</remarks>
        public Dictionary<ExPlayer, VoteOption> Votes { get; } = new();

        /// <summary>
        /// Gets or sets the duration of the vote.
        /// </summary>
        public int Duration { get; set; } = 0;

        /// <summary>
        /// Gets or sets the title of the vote.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Calculates the percentage of votes for the specified option.
        /// </summary>
        /// <param name="option">The vote option for which to calculate the percentage.</param>
        /// <returns>An integer representing the percentage of votes for the specified option. Returns 0 if there are no votes.</returns>
        public int GetPercentage(VoteOption option)
        {
            if (Votes.Count == 0)
                return 0;

            var count = Votes.Count(kvp => kvp.Value == option);
            return Mathf.CeilToInt(((float)count / (float)Votes.Count) * 100);
        }

        /// <summary>
        /// Determines the winning option based on the current votes, if there is a clear winner.
        /// </summary>
        /// <remarks>If two or more options are tied for the highest number of votes, the method returns
        /// null to indicate that there is no single winner.</remarks>
        /// <returns>The option that has received the most votes, or null if there is a tie or no votes have been cast.</returns>
        public VoteOption? GetWinner()
        {
            if (Votes.Count == 0)
                return null;

            var first = Options
                .OrderByDescending(opt => Votes.Count(kvp => kvp.Value == opt))
                .First();
            var firstVotes = Votes.Count(kvp => kvp.Value == first);

            for (var x = 0; x < Options.Count; x++)
            {
                var option = Options[x];
                var votes = Votes.Count(kvp => kvp.Value == option);

                if (votes < firstVotes)
                    continue;

                if (votes == firstVotes && option != first)
                    return null;
            }

            return first;
        }
    }
}