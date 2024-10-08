using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Promise.Framework.API;
using Promise.Framework.Chart;
using Promise.Framework.UI;
using Promise.Framework.UI.Noteskins;
using Promise.Framework.Utilities;

namespace Promise.Framework.Objects;

[GlobalClass]
public partial class ChartController : Control
{
    /// <summary>
    /// The current stats on this ChartController.
    /// </summary>
    [ExportGroup("Status"), Export] public ChartStatistics Statistics { get; private set; } = new();

    /// <summary>
    /// The index of the current chart controller. Useful if you have multiple Chart Controllers.
    /// </summary>
    [Export] public int Index = 0;

    /// <summary>
    /// The current note skin assigned to this ChartController.
    /// </summary>
    [ExportGroup("Settings"), Export] public NoteSkin NoteSkin { get; private set; }

    /// <summary>
    /// The lanes assigned to this ChartController.
    /// </summary>
    [ExportGroup("References"), Export] public NoteLaneController[] Lanes { get; private set; }

    /// <summary>
    /// The HUD assigned to ths ChartController.
    /// </summary>
    [Export] public ChartHud ChartHud { get; private set; }

    /// <summary>
    /// The chart data for this ChartController.
    /// </summary>
    [Export] public IndividualChart Chart { get; private set; }

    /// <summary>
    /// The note scripts currently running on this chart controller.
    /// </summary>
    public Dictionary<string, INoteScript> NoteScripts { get; private set; } = new();

    [Signal] public delegate void NoteHitEventHandler(ChartController chartCtrl, NoteData noteData, NoteEventResult result, double distanceFromTime, bool held);
    [Signal] public delegate void NoteMissEventHandler(ChartController chartCtrl, NoteData noteData, NoteEventResult result, double distanceFromTime, bool held);
    [Signal] public delegate void PressedEventHandler(NoteLaneController noteLaneController);

    public void Initialize(int laneCount, IndividualChart chart, bool autoplay = true, float scrollSpeed = 1.0f, NoteSkin noteSkin = null, ChartHud chartHud = null)
    {
        NoteSkin = noteSkin ?? PromiseData.DefaultNoteSkin;
        ChartHud = chartHud ?? PromiseData.DefaultChartHud.Instantiate<ChartHud>();
        AddChild(ChartHud);
            
        Chart = chart;
        Lanes = new NoteLaneController[laneCount];

        Type[] noteScriptTypes = AppDomain.CurrentDomain.GetTypesWithInterface<INoteScript>();
        for (int i = 0; i < noteScriptTypes.Length; i++)
        {
            Type t = noteScriptTypes[i];
                
            Attribute[] attributes = Attribute.GetCustomAttributes(t);
            if (attributes.Count(x => x is NoteTypeBind) > 0)
            {
                NoteTypeBind typeBind = attributes.FirstOrDefault(x => x is NoteTypeBind) as NoteTypeBind;
                if (Chart.Notes.Count(x => x.Type == typeBind.NoteType) > 0)
                {
                    INoteScript noteScript = (INoteScript)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    NoteData[] matchingNotes = Chart.Notes.Where(x => x.Type == typeBind.NoteType).ToArray();
                    for (int j = 0; j < matchingNotes.Length; j++)
                        noteScript.OnNoteCreate(this, matchingNotes[j]);
			            
                    NoteScripts.Add(typeBind.NoteType, noteScript);
                }
            }
        }
            
        // Note count
        for (int n = 0; n < Chart.Notes.Length; n++)
        {
            if (Chart.Notes[n].ShouldMiss)
                continue;
	            
            Statistics.NoteCount++;
            if (Chart.Notes[n].Length > 0)
                Statistics.NoteCount++;
        }

        for (int i = 0; i < Lanes.Length; i++)
        {
            NoteLaneController noteCtrl = new NoteLaneController();
            noteCtrl.Initialize(Chart.Notes.Where(x => x.Lane == i).ToArray(), i, this, autoplay);
            noteCtrl.Position = new Vector2(i * NoteSkin.LaneSize - ((laneCount - 1) * NoteSkin.LaneSize / 2f), 0);
            noteCtrl.Name = "NoteLaneController " + i;
            noteCtrl.ScrollSpeed = scrollSpeed;

            AddChild(noteCtrl);
            Lanes[i] = noteCtrl;
        }
    }

    public void LoadNoteSkin(NoteSkin noteSkin)
    {
        NoteSkin = noteSkin;
            
        for (int i = 0; i < Lanes.Length; i++)
            Lanes[i].ChangeNoteSkin(noteSkin);
    }

    public void LoadChartHud(ChartHud chartHud, bool downScroll)
    {
        ChartHud?.QueueFree();

        ChartHud = chartHud;
        ChartHud.SwitchDirection(downScroll);
        AddChild(ChartHud);
    }

    public void OnNotePress(NoteData noteData, NoteHitType hitType, double distanceFromTime, bool held = false)
    {
        NoteHitType hit = hitType;
			
        NoteEventResult noteEventResult = new NoteEventResult(hit);
        string noteType = noteData.Type;
        INoteScript noteScript = NoteScripts.ContainsKey(noteType)
            ? NoteScripts[noteType]
            : null;

        if (noteScript != null)
        {
            if (hitType == NoteHitType.Miss)
                noteEventResult += noteScript.OnNoteMiss(this, noteData, held);
            
            if (noteEventResult.Hit != NoteHitType.Miss && noteEventResult.Hit != NoteHitType.None)
            {
                if (!held)
                    noteEventResult += noteScript.OnNoteHit(this, noteData, hit);
                else
                    noteEventResult += noteScript.OnNoteHeld(this, noteData, hit);
            }
				
            hit = noteEventResult.Hit;
        }
			
        switch (hit)
        {
            case NoteHitType.Miss:
                OnNoteMiss(noteData, noteEventResult, distanceFromTime, held);
                return;
            case NoteHitType.None:
                noteEventResult.Free();
                return;
            default:
                OnNoteHit(noteData, noteEventResult, distanceFromTime, held);
                return;
        }
    }
        
    public void OnNoteHit(NoteData noteData, NoteEventResult result, double distanceFromTime, bool held = false)
    {
        if (!result.HasFlag(NoteEventProcessFlags.Health))
        {
            float health = 0f;
            switch (result.Hit)
            {
                case NoteHitType.Perfect:
                    Statistics.PerfectHits++;
                    health = 0.1f;
                    break;
                case NoteHitType.Great:
                    Statistics.GreatHits++;
                    health = 0.08f;
                    break;
                case NoteHitType.Good:
                    Statistics.GoodHits++;
                    health = 0.04f;
                    break;
                case NoteHitType.Bad:
                    Statistics.BadHits++;
                    health = -0.04f;
                    break;
            }

            if (held)
                health *= 0.75f;

            Statistics.Health += health;

            if (Statistics.Health > Statistics.MaxHealth)
                Statistics.Health = Statistics.MaxHealth;
            if (Statistics.Health < 0)
                Statistics.Health = 0;
        }

        if (!result.HasFlag(NoteEventProcessFlags.Score))
        {
            Statistics.Combo++;
            Statistics.HighestCombo = Statistics.Combo > Statistics.HighestCombo ? Statistics.Combo : Statistics.HighestCombo;
                
            if (Statistics.MissStreak > 0)
                ChartHud?.HitDistance?.ResetRating();
                    
            Statistics.MissStreak = 0;	
        }
				
        UpdateChartHud(result.Hit, distanceFromTime, Statistics.Combo);
        EmitSignal(SignalName.NoteHit, this, noteData, result, distanceFromTime, held);
        result.Free();
    }

    public void OnNoteMiss(NoteData noteData, NoteEventResult result, double distanceFromTime, bool held = false)
    {
        if (!result.HasFlag(NoteEventProcessFlags.Health))
            Statistics.Health += (-0.1f - (Statistics.MissStreak * 0.08f)) * (noteData.Length > 0 ? 0.5f : 1f);

        if (!result.HasFlag(NoteEventProcessFlags.Score))
        {
            Statistics.Combo = 0;
            Statistics.Misses++;
            Statistics.MissStreak++;	
        }
            
        UpdateChartHud(NoteHitType.Miss, distanceFromTime, Statistics.Combo);
        EmitSignal(SignalName.NoteMiss, this, noteData, result, distanceFromTime, held);
        result.Free();
    }
        
    public void OnLanePress(NoteLaneController lane)
    {
        EmitSignal(SignalName.Pressed, lane);
    }
        
    public void UpdateChartHud(NoteHitType hitType, double distanceFromTime, uint combo)
    {
        if (ChartHud == null)
            return;

        ChartHud.ComboDisplay?.UpdateCombo(combo);
        ChartHud.Judgment?.Play(hitType);
        ChartHud.HitDistance?.Play(distanceFromTime, hitType);
    }
}