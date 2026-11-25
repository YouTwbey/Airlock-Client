using System;

namespace AirlockClient.Data
{
    public static class Enums
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

        public static Enum GetSettingFromString(string setting)
        {
            return setting switch
            {
                "AnonymousVotes" => MatchBoolSettings.AnonymousVotes,
                "VisualTasks" => MatchBoolSettings.VisualTasks,
                "ChatDuringTasks" => MatchBoolSettings.AnonymousVotes,
                "AlwaysShowHands" => MatchBoolSettings.VisualTasks,
                "ReportBodies" => MatchBoolSettings.ReportBodies,
                "CrewmateVisionDistance" => MatchFloatSettings.CrewmateVisionDistance,
                "ImpostorVisionDistance" => MatchFloatSettings.ImpostorVisionDistance,
                "TaggedSpeedMultiplier" => MatchFloatSettings.TaggedSpeedMultiplier,
                "InfectedVisionDistance" => MatchFloatSettings.InfectedVisionDistance,
                "NumEmergencyMeetings" => MatchIntSettings.NumEmergencyMeetings,
                "DiscussionTime" => MatchIntSettings.DiscussionTime,
                "VotingTime" => MatchIntSettings.VotingTime,
                "TaskBarUpdateFrequency" => MatchIntSettings.TaskBarUpdateFrequency,
                "LongTasks" => MatchIntSettings.LongTasks,
                "ShortTasks" => MatchIntSettings.ShortTasks,
                "ConfirmEjects" => MatchIntSettings.ConfirmEjects,
                "TagCooldown" => MatchIntSettings.TagCooldown,
                "TagTotalTasks" => MatchIntSettings.TagTotalTasks,
                "MaxInfected" => MatchIntSettings.MaxInfected,
                "TagNumTasksAssigned" => MatchIntSettings.TagNumTasksAssigned,
                "TagTotalTasksPrevious" => MatchIntSettings.TagTotalTasksPrevious,
                "GuardianAngelImpostorSeesGuard" => RoleBoolSettings.GuardianAngelImpostorSeesGuard,
                "VIPImpostorsKnow" => RoleBoolSettings.VIPImpostorsKnow,
                "AllowDoorSabotage" => RoleBoolSettings.AllowDoorSabotage,
                "ChanceOfEngineer" => RoleFloatSettings.ChanceOfEngineer,
                "ChanceOfGuardianAngel" => RoleFloatSettings.ChanceOfGuardianAngel,
                "ChanceOfInfected" => RoleFloatSettings.ChanceOfInfected,
                "ChanceOfRevenger" => RoleFloatSettings.ChanceOfRevenger,
                "ChanceOfSheriff" => RoleFloatSettings.ChanceOfSheriff,
                "ChanceOfVigilante" => RoleFloatSettings.ChanceOfVigilante,
                "ChanceOfVIP" => RoleFloatSettings.ChanceOfVIP,
                "ChanceOfTracker" => RoleFloatSettings.ChanceOfTracker,
                "SheriffSpeedMultiplier" => RoleFloatSettings.SheriffSpeedMultiplier,
                "VentUseCooldownEngineer" => RoleIntSettings.VentUseCooldownEngineer,
                "MaxTimeInVentsEngineer" => RoleIntSettings.MaxTimeInVentsEngineer,
                "VigilanteKillCooldown" => RoleIntSettings.VigilanteKillCooldown,
                "VigilanteNumOfKills" => RoleIntSettings.VigilanteNumOfKills,
                "MaxEngineers" => RoleIntSettings.MaxEngineers,
                "MaxVigilantes" => RoleIntSettings.MaxVigilantes,
                "MaxGuardianAngels" => RoleIntSettings.MaxGuardianAngels,
                "GuardianAngelGuardDuration" => RoleIntSettings.GuardianAngelGuardDuration,
                "GuardianAngelGuardCooldown" => RoleIntSettings.GuardianAngelGuardCooldown,
                "MaxRevengers" => RoleIntSettings.MaxRevengers,
                "MaxTrackers" => RoleIntSettings.MaxTrackers,
                "TrackerCooldown" => RoleIntSettings.TrackerCooldown,
                "TrackerDuration" => RoleIntSettings.TrackerDuration,
                "TrackerPingFrequency" => RoleIntSettings.TrackerPingFrequency,
                "RevengerSelfKillCooldown" => RoleIntSettings.RevengerSelfKillCooldown,
                "RevengerKillCooldown" => RoleIntSettings.RevengerKillCooldown,
                "RevengerAudioDelay" => RoleIntSettings.RevengerAudioDelay,
                "RevengerNumOfKills" => RoleIntSettings.RevengerNumOfKills,
                "MaxVIPs" => RoleIntSettings.MaxVIPs,
                "ScanCooldown" => RoleIntSettings.ScanCooldown,
                "VIPNumOfScans" => RoleIntSettings.VIPNumOfScans,
                "MaxSheriff" => RoleIntSettings.MaxSheriff,
                "NumImposters" => RoleIntSettings.NumImposters,
                "KillCooldown" => RoleIntSettings.KillCooldown,
                "SabotageCooldown" => RoleIntSettings.SabotageCooldown,
                _ => null
            };
        }
    }
}
