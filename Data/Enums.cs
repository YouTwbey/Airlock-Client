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
            ReportBodies
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
            NumEmergencyMeetings,
            DiscussionTime,
            VotingTime,
            TaskBarUpdateFrequency,
            LongTasks,
            ShortTasks,
            ConfirmEjects,
            TagCooldown,
            TagTotalTasks,
            MaxInfected,
            TagNumTasksAssigned,
            TagTotalTasksPrevious
        }

        public enum RoleBoolSettings
        {
            GuardianAngelImpostorSeesGuard,
            VIPImpostorsKnow,
            AllowDoorSabotage
        }

        public enum RoleFloatSettings
        {
            ChanceOfEngineer,
            ChanceOfGuardianAngel,
            ChanceOfInfected,
            ChanceOfRevenger,
            ChanceOfSheriff,
            ChanceOfVigilante,
            ChanceOfVIP,
            ChanceOfTracker,
            SheriffSpeedMultiplier
        }

        public enum RoleIntSettings
        {
            VentUseCooldownEngineer,
            MaxTimeInVentsEngineer,
            VigilanteKillCooldown,
            VigilanteNumOfKills,
            MaxEngineers,
            MaxVigilantes,
            MaxGuardianAngels,
            GuardianAngelGuardDuration,
            GuardianAngelGuardCooldown,
            MaxRevengers,
            MaxTrackers,
            TrackerCooldown,
            TrackerDuration,
            TrackerPingFrequency,
            RevengerSelfKillCooldown,
            RevengerKillCooldown,
            RevengerAudioDelay,
            RevengerNumOfKills,
            MaxVIPs,
            ScanCooldown,
            VIPNumOfScans,
            MaxSheriff,
            NumImposters,
            KillCooldown,
            SabotageCooldown
        }
    }
}
