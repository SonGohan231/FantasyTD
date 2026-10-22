using System;

namespace FantasyTD.V2.Core
{
    public enum FeedbackKind
    {
        None, Plant, Upgrade, Hit, Kill, Leak, Income, Wave, WaveCleared, Victory, Defeat
    }

    /// <summary>
    /// Engine-agnostic presentation contract: sim emits authoritative battle events,
    /// Unity shows feedback without changing combat or economy state.
    /// </summary>
    public struct FeedbackCue
    {
        public readonly FeedbackKind Kind;
        public readonly string Message;
        public readonly bool Audible;

        public FeedbackCue(FeedbackKind kind, string message, bool audible)
        {
            Kind = kind;
            Message = message;
            Audible = audible;
        }
    }

    public static class BattleFeedbackCatalog
    {
        public static FeedbackCue Map(BattleEvent battleEvent)
        {
            switch (battleEvent.Kind)
            {
                case BattleEventKind.TowerPlaced:
                    return new FeedbackCue(FeedbackKind.Plant,
                        battleEvent.Value.ToString(), true);
                case BattleEventKind.TowerUpgraded:
                    return new FeedbackCue(FeedbackKind.Upgrade,
                        "ULEPSZENIE " + battleEvent.Value, true);
                case BattleEventKind.EnemyDamaged:
                    return new FeedbackCue(FeedbackKind.Hit,
                        "-" + battleEvent.Value, true);
                case BattleEventKind.EnemyKilled:
                    return new FeedbackCue(FeedbackKind.Kill,
                        "+" + battleEvent.Value, true);
                case BattleEventKind.EnemyLeaked:
                    return new FeedbackCue(FeedbackKind.Leak,
                        "-" + battleEvent.Value, true);
                case BattleEventKind.Income:
                    return new FeedbackCue(FeedbackKind.Income,
                        "+" + battleEvent.Value, false);
                case BattleEventKind.WaveStarted:
                    return new FeedbackCue(FeedbackKind.Wave,
                        "FALA " + battleEvent.WaveNumber, true);
                case BattleEventKind.WaveCleared:
                    return new FeedbackCue(FeedbackKind.WaveCleared,
                        "FALA OBRONIONA", true);
                case BattleEventKind.Victory:
                    return new FeedbackCue(FeedbackKind.Victory,
                        "ZWYCIESTWO", true);
                case BattleEventKind.Defeat:
                    return new FeedbackCue(FeedbackKind.Defeat,
                        "PORAZKA", true);
                default:
                    return new FeedbackCue(FeedbackKind.None, string.Empty, false);
            }
        }
    }
}
