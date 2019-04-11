namespace LiveScore.Features.Matches.Models
{
    using LiveScore.Shared.Models;

    public class EventTypes : Enumeration
    {
        //break_start
        //cancelled_video_assistant_referee
        //corner_kick
        //free_kick
        //goal_kick
        //injury
        //injury_return
        //injury_time_shown
        //match_ended
        //match_started
        //offside
        //penalty_awarded
        //penalty_missed
        //penalty_shootout
        //period_score
        //period_start
        //possible_video_assistant_referee
        //red_card
        public static readonly EventTypes RedCard = new EventTypes("red_card", nameof(RedCard));

        //score_change
        public static readonly EventTypes ScoreChange = new EventTypes("score_change", nameof(ScoreChange));

        //shot_off_target
        //shot_on_target
        //shot_saved
        //substitution
        public static readonly EventTypes Substitution = new EventTypes("substitution", nameof(Substitution));

        //throw_in
        //video_assistant_referee
        //video_assistant_referee_over
        //yellow_card
        public static readonly EventTypes YellowCard = new EventTypes("yellow_card", nameof(YellowCard));

        //yellow_red_card
        public static readonly EventTypes YellowRedCard = new EventTypes("yellow_red_card", nameof(YellowRedCard));

        public EventTypes()
        {
        }

        public EventTypes(string value, string displayName)
            : base(value, displayName)
        {
        }

        public EventTypes(string value)
            : base(value, value)
        {
        }
    }
}