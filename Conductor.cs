using Godot;
using Promise.Framework.Chart;

namespace Promise.Framework;

/// <summary>
/// The global Conductor class, which keeps track of most of the time-related things.
/// </summary>
[GlobalClass]
public partial class Conductor : Node
{
    #region Singleton
    /// <summary>
    /// The current instance of the Conductor class.
    /// </summary>
    private static Conductor Instance;
    #endregion

    #region Status Variables
    /// <summary>
    /// The current BPM at the moment.
    /// </summary>
    public static double Bpm
    {
        get => Instance._Bpm;
        set => Instance._Bpm = value;
    }
    [ExportGroup("Status"), Export] private double _Bpm = 100;

    /// <summary>
    /// The index pointing to which BPM is currently set based on the bpms array.
    /// </summary>
    public static int BpmIndex
    {
        get => Instance._BpmIndex;
        set => Instance._BpmIndex = value;
    }
    [Export] private int _BpmIndex;

    /// <summary>
    /// Corrects the time so the chart can be accurate.
    /// </summary>
    public static double ChartOffset
    {
        get => Instance._ChartOffset;
        set => Instance._ChartOffset = value;
    }
    [Export] private double _ChartOffset;
    
    /// <summary>
    /// How fast the Conductor goes.
    /// </summary>
    public static double Speed
    {
        get => Instance._Speed;
        set => Instance._Speed = value;
    }
    [Export] private double _Speed = 1f;

    /// <summary>
    /// Is true when the Conductor has been started with Start() or Play(), false when either Pause() or Stop() is called.
    /// </summary>
    public static bool Playing
    {
        get => Instance._Playing;
    }
    [Export] private bool _Playing { get; set; }
    #endregion

    #region Time Variables
    /// <summary>
    /// The raw timestamp of this Conductor, without any corrections made to it.
    /// </summary>
    private double _RawTime => GetRawTime();
        
    /// <summary>
    /// The raw timestamp of this Conductor + the chart offset.
    /// </summary>
    private double _UncorrectedTime => GetUncorrectedTime();

    /// <summary>
    /// The current timestamp from when the time was last set.
    /// Equivalent to Conductor.songPosition in most other FNF projects.
    /// </summary>
    public static double Time
    {
        get => Instance._Time;
        set => Instance._Time = value;
    }
    private double _Time
    {
        get => GetTime(); 
        set => SetTime(value);
    }

    /// <summary>
    /// The current step according to the time, which also keeps BPM changes in mind.
    /// </summary>
    public static double CurrentMeasure => Instance._CurrentMeasure;
    private double _CurrentStep => GetCurrentStep();

    /// <summary>
    /// The current beat according to the time, which also keeps BPM changes in mind.
    /// </summary>
    public static double CurrentBeat => Instance._CurrentBeat;
    private double _CurrentBeat => GetCurrentBeat();

    /// <summary>
    /// The current measure according to the time, which also keeps BPM changes in mind.
    /// </summary>
    public static double CurrentStep => Instance._CurrentStep;
    private double _CurrentMeasure => GetCurrentMeasure();
    #endregion

    #region Other Variables
    /// <summary>
    /// All BPMs listed in the Conductor currently.
    /// </summary>
    public static BpmInfo[] Bpms
    {
        get => Instance._GetBpms();
        set => Instance._SetBpms(value);
    }
    [ExportGroup("Info")]  private BpmInfo[] _Bpms { get => _GetBpms(); set => _SetBpms(value); }
        
    /// <summary>
    /// A whole number that indicates the number of beats in each measure.
    /// </summary>
    public static int TimeSigNumerator
    {
        get => Instance._TimeSigNumerator;
        set => Instance._TimeSigNumerator = value;
    }
    private int _TimeSigNumerator = 4;
        
    /// <summary>
    /// A whole number representing what note values the beats indicated in TimeSigNumerator are.
    /// </summary>
    private int _TimeSigDenominator = 4;
    public static int TimeSigDenominator
    {
        get => Instance._TimeSigDenominator;
        set => Instance._TimeSigDenominator = value;
    }
    #endregion

    #region Private Fields
    private double _relativeStartTime;
    private double _relativeTimeOffset;
    private double _lastTime = double.NegativeInfinity;
    private double _delta;
    private double _time;
        
    private double _cachedStep;
    private double _cachedStepTime;

    private double _cachedBeat;
    private double _cachedBeatTime;

    private double _cachedMeasure;
    private double _cachedMeasureTime;
        
    private BpmInfo[] _bpms = { new() { Bpm = 100 } };
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs a new Conductor instance. Will not work if there is already one running.
    /// </summary>
    public Conductor()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
            
        Instance = this;
    }
    #endregion
    
    #region Static Methods
    /// <summary>
    /// Starts the Conductor at the time provided.
    /// </summary>
    /// <param name="time">The time the Conductor starts at. Default is 0</param>
    public static void Start(double time = 0d) => Instance._Start(time);
    
    /// <summary>
    /// Resumes the Conductor at the last time it was paused at.
    /// </summary>
    public static void Play() => Instance._Play();
    
    /// <summary>
    /// Stops the Conductor from ticking, but keeps the current time.
    /// </summary>
    public static void Pause() => Instance._Pause();
    
    /// <summary>
    /// Stops the Conductor entirely, resetting its time to 0.
    /// </summary>
    public static void Stop() => Instance._Stop();
    
    /// <summary>
    /// Resets the Conductor, wiping all its fields with its default values.
    /// </summary>
    public static void Reset() => Instance._Reset();
    
    /// <summary>
    /// Gets the raw time of this Conductor, without any corrections made to it.
    /// </summary>
    /// <returns>The raw time, in seconds</returns>
    public static double GetRawTime() => Instance._GetRawTime();
    
    /// <summary>
    /// Gets the raw time of this Conductor + the chart offset.
    /// </summary>
    /// <returns>The raw time + the chart offset, in seconds</returns>
    public static double GetUncorrectedTime() => Instance._GetUncorrectedTime();
    
    /// <summary>
    /// Gets the calculated time of this Conductor.
    /// </summary>
    /// <returns>The raw time + the chart offset multiplied by the speed, in seconds</returns>
    public static double GetTime() => Instance._GetTime();
    
    /// <summary>
    /// Sets the time of this Conductor.
    /// </summary>
    /// <param name="time">The time to set it to, in seconds.</param>
    public static void SetTime(double time) => Instance._SetTime(time);
    
    /// <summary>
    /// Gets the current step of this Conductor, with decimals.
    /// </summary>
    /// <returns>The current step</returns>
    public static double GetCurrentStep() => Instance._GetCurrentStep();
    
    /// <summary>
    /// Gets the current beat of this Conductor, with decimals.
    /// </summary>
    /// <returns>The current beat</returns>
    public static double GetCurrentBeat() => Instance._GetCurrentBeat();
    
    /// <summary>
    /// Gets the current measure of this Conductor, with decimals.
    /// </summary>
    /// <returns>The current measure</returns>
    public static double GetCurrentMeasure() => Instance._GetCurrentMeasure();
    #endregion
    
    #region Private Methods
        
    #region Process Method
    public override void _Process(double delta)
    {
        if (!Playing)
        {
            _relativeStartTime = Godot.Time.GetTicksMsec() / 1000d;
            _relativeTimeOffset = _time;
        }

        base._Process(delta);
            
        // Handles bpm changing
        if (BpmIndex < Bpms.Length - 1 && Bpms[BpmIndex + 1].MsTime / 1000f <= Time)
        {
            BpmIndex++;
            Bpm = Bpms[BpmIndex].Bpm;
        }
    }
    #endregion
    
    private void _Start(double time = 0d)
    {
        Play();
        Time = time;
    }
    
    private void _Play() => _Playing = true;
    
    private void _Pause()
    {
        _time = _RawTime;
        _Playing = false;
    }
    
    private void _Stop()
    {
        Time = 0;
        Pause();
    }
    
    private void _Reset()
    {
        TimeSigNumerator = TimeSigDenominator = 4;
        Bpms = new BpmInfo[] { new() { Bpm = 100 } };
        Bpm = Bpms[0].Bpm;
        BpmIndex = 0;
        ChartOffset = 0;
        Speed = 1f;
        Stop();
    }
    #endregion
        
    #region Getters and Setters
    private double _GetRawTime() =>
        Playing ? Godot.Time.GetTicksMsec() / 1000d - _relativeStartTime + _relativeTimeOffset :
        _time != 0d ? _time : 0d;
    
    private double _GetUncorrectedTime() => _RawTime + ChartOffset / 1000d;
    
    private double _GetTime() => _UncorrectedTime * Speed;
    
    private void _SetTime(double time)
    {
        _time = time;
        _relativeStartTime = Godot.Time.GetTicksMsec() / 1000d;
        _relativeTimeOffset = time;
    }
    
    private double _GetCurrentStep()
    {
        if (_cachedStepTime == Time)
            return _cachedStep;

        if (Bpms.Length <= 1)
            return Time / (60d / (Bpm * TimeSigDenominator));

        _cachedStepTime = Time;
        _cachedStep = (Time - Bpms[BpmIndex].MsTime / 1000d) / (60 / (Bpm * TimeSigDenominator)) + (Bpms[BpmIndex].Time * TimeSigNumerator * TimeSigDenominator);
        return _cachedStep;
    }
    
    private double _GetCurrentBeat()
    {
        if (_cachedBeatTime == Time)
            return _cachedBeat;

        if (Bpms.Length <= 1)
            return Time / (60d / Bpm);

        _cachedBeatTime = Time;
        _cachedBeat = (Time - Bpms[BpmIndex].MsTime / 1000d) / (60d / Bpm) + Bpms[BpmIndex].Time * TimeSigNumerator;
        return _cachedBeat;
    }
    
    private double _GetCurrentMeasure()
    {
        if (_cachedMeasureTime == Time)
            return _cachedMeasure;

        if (Bpms.Length <= 1)
            return Time / (60d / (Bpm / TimeSigNumerator));

        _cachedMeasureTime = Time;
        _cachedMeasure = (Time - Bpms[BpmIndex].MsTime / 1000d) / (60d / (Bpm / TimeSigNumerator)) + Bpms[BpmIndex].Time;
        return _cachedMeasure;
    }
    
    private BpmInfo[] _GetBpms() => _bpms;
    
    private void _SetBpms(BpmInfo[] data)
    {
        _bpms = data;
        Bpm = Bpms[0].Bpm;
    }
    #endregion
        
    #region GDScript Compatibility
    /// <summary>
    /// The current BPM at the moment.
    /// </summary>
    /// <returns>The current BPM</returns>
    public double _GetBpm() => Bpm;
        
    /// <summary>
    /// A boolean for if the Conductor is playing or not.
    /// </summary>
    /// <returns>True when the Conductor has been started with Start() or Play(), false when either Pause() or Stop() is called.</returns>
    public bool _IsPlaying() => Playing;
    #endregion
}