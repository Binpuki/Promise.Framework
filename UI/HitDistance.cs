using System;
using System.Linq;
using Godot;

namespace Promise.Framework.UI;

/// <summary>
/// Displays the distance from where the note was supposed to be hit with the chart controller.
/// </summary>
public partial class HitDistance : Control
{
    #region Exported Variables
    /// <summary>
    /// The animation to play upon a perfect hit.
    /// </summary>
    [ExportGroup("Settings"), Export] public string Perfect = "perfect";
        
    /// <summary>
    /// The animation to play upon a great hit.
    /// </summary>
    [Export] public string Great = "great";
        
    /// <summary>
    /// The animation to play upon a good hit.
    /// </summary>
    [Export] public string Good = "good";
        
    /// <summary>
    /// The animation to play upon a bad hit.
    /// </summary>
    [Export] public string Bad = "bad";
        
    /// <summary>
    /// The animation to play upon missing.
    /// </summary>
    [Export] public string Miss = "miss";

    /// <summary>
    /// A reference to the AnimationPlayer.
    /// </summary>
    [ExportGroup("References"), Export] public AnimationPlayer AnimationPlayer;
        
    /// <summary>
    /// A reference to the Label.
    /// </summary>
    [Export] public Label Label;
    #endregion

    #region Private Variables
    private NoteHitType _lastRating = NoteHitType.Perfect;
    #endregion
        
    #region Public Methods
    /// <summary>
    /// Plays the corresponding animation from the provided rating. Can be overridden to have custom behaviour.
    /// </summary>
    /// <param name="distance">The distance from when the note was supposed to be hit, in seconds.</param>
    /// <param name="rating">The rating</param>
    public virtual void Play(double distance, NoteHitType rating)
    {
        Label.Text = $"{distance:0.00} ms";
        if (Math.Abs(distance) > PromiseData.HitWindows.Last())
            Label.Text = $"Too {(distance < 0 ? "late!" : "early!")}";

        if (rating > _lastRating)
            _lastRating = rating;

        string anim = Perfect;
        switch (_lastRating)
        {
            case NoteHitType.Great: anim = Great;   break;
            case NoteHitType.Good:  anim = Good;    break;
            case NoteHitType.Bad:   anim = Bad;     break;
            case NoteHitType.Miss:  anim = Miss;    break;
        }

        if (AnimationPlayer == null) 
            return;
            
        AnimationPlayer.Play(anim);
        AnimationPlayer.Seek(0, true);
    }

    /// <summary>
    /// Resets the rating on this HitDistance instance.
    /// </summary>
    public void ResetRating()
    {
        _lastRating = NoteHitType.Perfect;
    }
    #endregion
}