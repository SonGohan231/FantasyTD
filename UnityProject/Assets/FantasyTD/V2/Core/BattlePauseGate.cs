namespace FantasyTD.V2.Core
{
    /// <summary>
    /// Minimal engine-independent gate. Rendering and UI can continue while
    /// simulation time is stopped; resetting a battle always clears the pause.
    /// </summary>
    public sealed class BattlePauseGate
    {
        public bool IsPaused { get; private set; }
        public bool CanAdvance { get { return !IsPaused; } }

        public void Toggle()
        {
            IsPaused = !IsPaused;
        }

        public void Reset()
        {
            IsPaused = false;
        }
    }
}
