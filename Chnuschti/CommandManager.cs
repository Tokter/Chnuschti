using System;
using System.Diagnostics;

namespace Chnuschti;

/// <summary>
/// Central hub that tells all commands,
/// “Something changed – please re-evaluate CanExecute.”
/// </summary>
public static class CommandManager
{
    /// <summary>
    /// Raised by the manager.  <br/>
    /// <para>
    /// Command implementations *forward* the add/remove of their own
    /// <c>CanExecuteChanged</c> to this event so the UI gets a single,
    /// co-ordinated refresh message.
    /// </para>
    /// </summary>
    public static event EventHandler? RequerySuggested;

    // --------------------------------------------------------------------

    /// <summary>Call when any condition that can influence a command’s
    /// CanExecute state has changed – for instance after user input or
    /// when a ViewModel property setter fires.</summary>
    public static void InvalidateRequerySuggested()
    {
        // Optional throttling so we don't spam the UI 1000×/sec.
        if (!Throttle.ShouldRaise()) return;

        RequerySuggested?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>Shortcut you can call from your input system
    /// (mouse, keyboard, touch) after it has processed an event.</summary>
    public static void NotifyInput() => InvalidateRequerySuggested();

    // --------------------------------------------------------------------
    // ----  Internal helpers  --------------------------------------------
    // --------------------------------------------------------------------

    private static class Throttle
    {
        // Limit to one raise every _interval (ms).
        private static readonly TimeSpan _interval = TimeSpan.FromMilliseconds(40);
        private static Stopwatch _sw = Stopwatch.StartNew();

        public static bool ShouldRaise()
        {
            if (_sw.Elapsed < _interval) return false;
            _sw.Restart();
            return true;
        }
    }
}
