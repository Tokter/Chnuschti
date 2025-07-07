using SkiaSharp;
using System.Numerics;

namespace Chnuschti;

public enum AnimationType
{
    Linear, EaseIn, EaseOut, EaseInOut, Bounce, Elastic,
    Custom            // supply your own easing delegate
}

public delegate double EasingFunction(double from, double to, double progress);

public interface IAnimation
{
    AnimationType AnimationType { get; }
    string Name { get; }
    bool IsRunning { get; }
    void Start(object from, object to);
    void Stop();
    void Update(double deltaSeconds);
}

/// <summary>
/// Represents the base class for animations that interpolate between two values of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>This class provides core functionality for animations, including support for easing functions,
/// looping, ping-pong behavior, and event notification upon completion. Derived classes must implement the <see
/// cref="GetAnimationFunctionCore(Func{double, double, double, double})"/> method to define how interpolation between
/// values of type <typeparamref name="T"/> is performed.</remarks>
/// <typeparam name="T">The type of the values being animated.</typeparam>
public abstract class AnimationBase<T> : IAnimation
{
    /// <summary>
    /// This dictionary contains the default easing functions used for animation types. (shared by all instances)
    /// </summary>
    private static readonly Dictionary<AnimationType, EasingFunction> _easing =
        new()
        {
            [AnimationType.Linear] = Linear,
            [AnimationType.EaseIn] = EaseIn,
            [AnimationType.EaseOut] = EaseOut,
            [AnimationType.EaseInOut] = EaseInOut,
            [AnimationType.Bounce] = Bounce,
            [AnimationType.Elastic] = Elastic
        };


    private readonly Action<T> _setter;            // immutable
    private readonly Func<T, T, double, T> _interpolate;   // cached delegate

    private T? _from;
    private T? _to;
    private double _elapsed;                       // seconds

    // ─ looping flags
    private bool _initialReverse;
    private bool _pingPong;
    private int _loopsRemaining;                 // 0 = none,  -1 = infinite

    public string Name { get; }
    public TimeSpan Duration { get; }
    public AnimationType AnimationType { get; }
    public bool IsRunning { get; private set; }
    public event EventHandler? Completed;
    public T From => _from!;
    public T To => _to!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationBase"/> class with the specified name, duration, setter,
    /// animation type, and optional custom easing function.
    /// </summary>
    /// <remarks>This constructor initializes the animation with the specified parameters and determines the
    /// interpolation function based on the provided easing function or the default easing for the specified animation
    /// type.</remarks>
    /// <param name="name">The name of the animation. This is used to identify the animation instance.</param>
    /// <param name="duration">The duration of the animation. Must be greater than <see cref="TimeSpan.Zero"/>.</param>
    /// <param name="setter">An action that applies the animation's calculated value to the target property or object.</param>
    /// <param name="animationType">The type of animation to use. Defaults to <see cref="AnimationType.Linear"/>.</param>
    /// <param name="customEasing">An optional custom easing function to define the animation's progression. Required if <paramref
    /// name="animationType"/> is set to <see cref="AnimationType.Custom"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="duration"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="customEasing"/> is null when <paramref name="animationType"/> is set to <see
    /// cref="AnimationType.Custom"/>.</exception>
    protected AnimationBase(
        string name,
        TimeSpan duration,
        Action<T> setter,
        AnimationType animationType = AnimationType.Linear,
        EasingFunction? customEasing = null)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be > 0.", nameof(duration));

        Name = name;
        Duration = duration;
        _setter = setter;
        AnimationType = animationType;

        var easing = animationType == AnimationType.Custom
            ? customEasing ??
              throw new ArgumentNullException(nameof(customEasing), "Custom easing must be supplied.")
            : _easing[animationType];

        _interpolate = GetAnimationFunctionCore(easing);
    }

    /// <summary>
    /// Reverses the direction of the animation sequence.
    /// </summary>
    /// <remarks>Calling this method toggles the animation's reverse state. Subsequent calls will alternate
    /// the direction.</remarks>
    /// <returns>The current instance of <see cref="AnimationBase{T}"/>, allowing for method chaining.</returns>
    public AnimationBase<T> Reverse()
    {
        _initialReverse = !_initialReverse;
        return this;
    }

    /// <summary>
    /// Enables repeating the animation for a specified number of times.
    /// </summary>
    /// <param name="times">
    /// <c>-1</c> = infinite; <c>0</c> = no repeats; <c>n&gt;0</c> = play <c>n</c> extra times.
    /// </param>
    public AnimationBase<T> Repeat(int times = -1)
    {
        _loopsRemaining = times;
        return this;
    }

    /// <summary>
    /// Enables ping-pong behavior for the animation, causing it to alternate between forward and reverse playback.
    /// </summary>
    /// <remarks>When ping-pong behavior is enabled, the animation will reverse direction upon reaching the
    /// end, creating a looping effect that alternates between forward and reverse playback. This method modifies the
    /// internal state of the animation and returns the current instance to allow method chaining.</remarks>
    /// <returns>The current instance of <see cref="AnimationBase{T}"/>, with ping-pong behavior enabled.</returns>
    public AnimationBase<T> PingPong()
    {
        _pingPong = true;
        return this;
    }

    /// <summary>
    /// Starts the animation with the specified start and end values.
    /// </summary>
    /// <remarks>If the animation is configured to start in reverse, the <paramref name="from"/> and <paramref
    /// name="to"/> values will be swapped. The animation begins immediately, and the first frame is set to the
    /// <paramref name="from"/> value.</remarks>
    /// <param name="from">The initial value of the animation.</param>
    /// <param name="to">The final value of the animation.</param>
    /// <returns>The current instance of the animation, allowing for method chaining.</returns>
    public AnimationBase<T> Start(T from, T to)
    {
        if (_initialReverse) (from, to) = (to, from);

        _from = from;
        _to = to;
        _elapsed = 0;
        IsRunning = true;
        OnStarted();
        _setter(from);                 // ensure first frame visible

        return this;
    }

    void IAnimation.Start(object from, object to)
    {
        Start((T)from, (T)to);
    }

    /// <summary>
    /// Invoked when the animation starts. This method can be overridden in derived classes  to perform additional
    /// actions during the animation start event.
    /// </summary>
    protected virtual void OnStarted()
    {
    }

    /// <summary>
    /// Stops the operation and sets the running state to inactive.
    /// </summary>
    /// <remarks>This method sets the <see cref="IsRunning"/> property to <see langword="false"/>,  indicating
    /// that the operation is no longer active.</remarks>
    public void Stop() => IsRunning = false;

    /// <summary>
    /// Resumes the animation from its current state.
    /// </summary>
    /// <remarks>The animation must be started before it can be resumed. Attempting to resume an animation
    /// that has not been started will result in an exception.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if the animation has not been started prior to calling this method.</exception>
    public void Resume()
    {
        if (_from is null || _to is null) throw new InvalidOperationException("Animation must be started before resuming.");
        IsRunning = true;
    }

    /// <summary>
    /// Updates the progress of the animation based on the elapsed time.
    /// </summary>
    /// <remarks>This method advances the animation by the specified elapsed time. If the animation completes
    /// during the update,  the final value is set, the animation stops, and the <see cref="Completed"/> event is
    /// raised. If the animation is  configured to loop, it will restart instead of stopping.</remarks>
    /// <param name="deltaSeconds">The time, in seconds, that has elapsed since the last update. Must be a positive value.</param>
    public void Update(double deltaSeconds)
    {
        if (!IsRunning) return;

        _elapsed += deltaSeconds;
        double progress = _elapsed / Duration.TotalSeconds;

        if (progress >= 1.0)
        {
            _setter(_to!);             // snap to exact final value

            if (HandleLoop()) return;  // restarted → exit now

            Stop();
            Completed?.Invoke(this, EventArgs.Empty);
            return;
        }

        _setter(_interpolate(_from!, _to!, progress));
    }

    /// <summary>
    /// Handles the state transition for a looping operation, updating the direction and loop count.
    /// </summary>
    /// <remarks>This method updates the direction of the operation if ping-pong mode is enabled and
    /// decrements the remaining loop count. It resets the elapsed time for the next iteration. If the loop count
    /// reaches zero, the method returns <see langword="false"/>.</remarks>
    /// <returns><see langword="true"/> if the loop has been restarted; otherwise, <see langword="false"/> if no loops remain.</returns>
    private bool HandleLoop()
    {
        // ping-pong swaps direction every iteration
        if (_pingPong) (_from, _to) = (_to!, _from!);

        if (_loopsRemaining == 0) return false;     // no loops left
        if (_loopsRemaining > 0) _loopsRemaining--;

        _elapsed = 0;
        return true;                                // restarted
    }

    /// <summary>
    /// Creates a function that interpolates between two values of type <typeparamref name="T"/>  based on a specified
    /// easing function. Derived classes must implement this method to provide the specific
    /// interpolation logic for the type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="easing">The easing function used to control the interpolation behavior.</param>
    /// <returns>A delegate that takes two values of type <typeparamref name="T"/>, a progress value  between 0.0 and 1.0, and
    /// returns the interpolated value of type <typeparamref name="T"/>.</returns>
    protected abstract Func<T, T, double, T> GetAnimationFunctionCore(EasingFunction easing);

    #region Easing functions

    private static double Linear(double f, double t, double p) => f + (t - f) * p;

    private static double EaseIn(double f, double t, double p) => f + (t - f) * Math.Pow(p, 2);

    private static double EaseOut(double f, double t, double p) => f + (t - f) * (1 - Math.Pow(1 - p, 2));

    private static double EaseInOut(double f, double t, double p)
        => p < .5
           ? f + (t - f) * Math.Pow(p * 2, 2) / 2
           : f + (t - f) * (1 - Math.Pow(1 - (p - .5) * 2, 2) / 2);

    private static double Bounce(double f, double t, double p)
    {
        if (p < 0.363636) return f + (t - f) * (7.5625 * p * p);
        if (p < 0.727272)
        {
            p -= 0.545454;
            return f + (t - f) * (7.5625 * p * p + 0.75);
        }
        if (p < 0.909090)
        {
            p -= 0.818181;
            return f + (t - f) * (7.5625 * p * p + 0.9375);
        }
        p -= 0.954545;
        return f + (t - f) * (7.5625 * p * p + 0.984375);
    }

    private static double Elastic(double f, double t, double p)
    {
        const double period = 0.3;
        if (p == 0 || p == 1) return f + (t - f) * p;

        double s = period / 4;
        return f + (t - f) * Math.Pow(2, -10 * p)
                    * Math.Sin((p - s) * (2 * Math.PI) / period)
             + (t - f);
    }

    #endregion
}

/// <summary>
/// Represents a numeric animation that interpolates between two values over a specified duration.
/// </summary>
/// <remarks>This class provides functionality for animating numeric values using various animation types and
/// easing functions. The animation progresses from a starting value to an ending value, applying the specified easing
/// function to calculate intermediate values.</remarks>
/// <typeparam name="T">The numeric type used for the animation. Must implement <see cref="INumber{T}"/>.</typeparam>
public sealed class AnimationNumeric<T> : AnimationBase<T> where T : INumber<T>
{
    public AnimationNumeric(
        string name,
        TimeSpan duration,
        Action<T> setter,
        AnimationType animationType = AnimationType.Linear,
        EasingFunction? custom = null) : base(name, duration, setter, animationType, custom) { }

    private double _df = default!;
    private double _dt = default!;
    protected override void OnStarted()
    {
        _df = double.CreateSaturating(From);
        _dt = double.CreateSaturating(To);
    }

    protected override Func<T, T, double, T> GetAnimationFunctionCore(EasingFunction easing) => (from, to, p) =>
    {
        double res = easing(_df, _dt, p);
        return T.CreateSaturating(res);
    };
}

/// <summary>
/// Represents an animation that interpolates between two <see cref="SKColor"/> values over a specified duration.
/// </summary>
/// <remarks>This class provides functionality to animate color transitions using a specified easing function and
/// animation type. The animation updates the color value by invoking the provided setter action during the animation's
/// progress.</remarks>
public sealed class AnimationColor : AnimationBase<SKColor>
{
    public AnimationColor(
        string name,
        TimeSpan duration,
        Action<SKColor> setter,
        AnimationType animationType = AnimationType.Linear,
        EasingFunction? custom = null) : base(name, duration, setter, animationType, custom) { }

    protected override Func<SKColor, SKColor, double, SKColor> GetAnimationFunctionCore(EasingFunction easing)
    {
        // Extract a single 8-bit channel from a packed RGBA uint.
        static byte Channel(uint v, int shift) => (byte)(v >> shift);

        // Avoid GC pressure by working with the underlying uint
        return (from, to, p) =>
        {
            uint f = (uint)From;               // implicit SKColor → uint
            uint t = (uint)To;

            byte r = ToByte(easing(Channel(f, 24), Channel(t, 24), p));
            byte g = ToByte(easing(Channel(f, 16), Channel(t, 16), p));
            byte b = ToByte(easing(Channel(f, 8), Channel(t, 8), p));
            byte a = ToByte(easing(Channel(f, 0), Channel(t, 0), p));

            // repack RRGGBBAA
            uint packed = ((uint)r << 24) | ((uint)g << 16)
                        | ((uint)b << 8) | (uint)a;

            return packed;               // implicit uint → SKColor (no ctor call)
        };

        static byte ToByte(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);

    }
}

public sealed class AnimationGroup
{
    private readonly Dictionary<string, IAnimation> _members = new();

    public IReadOnlyCollection<IAnimation> Members => _members.Values;

    public AnimationGroup Add(IAnimation anim)
    {
        _members.Add(anim.Name, anim);
        return this;
    }

    public AnimationGroup Remove(IAnimation anim)
    {
        _members.Remove(anim.Name);
        return this;
    }

    public AnimationGroup Remove(string name)
    {
        if (_members.ContainsKey(name)) _members.Remove(name);
        return this;
    }

    public IAnimation this[string name] => _members[name];

    public void StopAll()
    {
        foreach (var a in _members.Values) a.Stop();
    }

    public void UpdateAll(double deltaSeconds)
    {
        foreach (var a in _members.Values) a.Update(deltaSeconds);
    }
}
