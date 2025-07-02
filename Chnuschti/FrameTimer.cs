using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

/// <summary>
/// Tracks frame timing information, including delta time, total elapsed time, and smoothed frames per second (FPS).
/// </summary>
/// <remarks>This class is designed to be used in real-time applications, such as games or simulations, where
/// frame timing is critical. Call <see cref="Tick"/> once per frame to update the timing values. The FPS value is
/// smoothed using a simple moving average to reduce fluctuations.</remarks>
public sealed class FrameTimer
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private double _prevTime;               // seconds
    private double _accumTime;              // seconds in current bucket
    private int _accumFrames;               // frames in current bucket
    private readonly double _window = 0.5d; // <- length of the averaging window

    public double DeltaTime { get; private set; }      // seconds
    public double TotalTime => _sw.Elapsed.TotalSeconds;
    public float Fps { get; private set; }

    public FrameTimer(double avgWindow = 0.5d)
    {
        _prevTime = _sw.Elapsed.TotalSeconds;
        _accumTime = 0d;
        _accumFrames = 0;
        _window = avgWindow;
        Fps = 0f;
    }

    /// Call once per frame *before* you update / render.
    public void Tick()
    {
        double now = _sw.Elapsed.TotalSeconds;
        DeltaTime = now - _prevTime;
        _prevTime = now;

        // accumulate in the current bucket
        _accumTime += DeltaTime;
        _accumFrames += 1;

        // when the bucket is “full” (≥ window seconds) publish FPS
        if (_accumTime >= _window)
        {
            Fps = (float)(_accumFrames / _accumTime);

            // start a new bucket but keep the excess time so windows
            // line up perfectly even with variable dt
            _accumTime %= _window;
            _accumFrames = 0;
        }
    }
}
