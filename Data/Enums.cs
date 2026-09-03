namespace AirlockClient.Data
{
    public class Enums
    {
        public enum MatchBoolSettings
        {
            AnonymousVotes,
            VisualTasks,
            ChatDuringTasks,
            AlwaysShowHands,
            ReportBodies,
            AllowDoorSabotage
        }

        public enum MatchFloatSettings
        {
            CrewmateVisionDistance,
            ImpostorVisionDistance,
            TaggedSpeedMultiplier,
            InfectedVisionDistance
        }

        public enum MatchIntSettings
        {
            NumImposters,
            NumEmergencyMeetings,
            DiscussionTime,
            VotingTime,
            KillCooldown,
            TaskBarUpdateFrequency,
            LongTasks,
            ShortTasks,
            SabotageCooldown,
            ConfirmEjects,
            TagCooldown,
            TagTotalTasks,
            MaxInfected,
            TagNumTasksAssigned
        }

        public enum RoleBoolSettings
        {

        }

        public enum RoleFloatSettings
        {
            ChanceOfEngineer,
            MaxTimeInVentsEngineer,
            VentUseCooldownEngineer,
            ChanceOfVigilante
        }

        public enum RoleIntSettings
        {
            MaxEngineers,
            MaxVigilantes,
            VigilanteKillCooldown,
            VigilanteNumOfKills,
            MaxSheriff
        }
    }
}
