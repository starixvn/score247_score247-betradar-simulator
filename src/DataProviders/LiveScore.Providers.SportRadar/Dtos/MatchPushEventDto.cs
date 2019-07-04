namespace LiveScore.Providers.SportRadar.Dtos
{    
    using Newtonsoft.Json;

    public class MatchPushEventDto
    {
        public PushEventPayload payload { get; set; }

        public PushEventMetaData metadata { get; set; }
    }

    public class PushEventPayload
    {
        public Sport_Event_Status sport_event_status { get; set; }

        [JsonProperty(PropertyName = "event")]
        public TimelineDto timeline { get; set; }
    }

    public class PushEventMetaData
    {
        public string tournament_id { get; set; }

        public string season_id { get; set; }

        public string sport_event_id { get; set; }

        public string sport_id { get; set; }
    }
}
