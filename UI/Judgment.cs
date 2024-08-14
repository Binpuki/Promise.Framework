using Godot;

namespace Promise.Framework.UI;

/// <summary>
/// Plays an animation that displays what type of rating the player got.
/// </summary>
public partial class Judgment : Control
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Plays the corresponding animation from the provided rating. Can be overridden to have custom behaviour.
    /// </summary>
    /// <param name="rating">The rating.</param>
    public virtual void Play(NoteHitType rating)
    {
        string anim = Perfect;
        switch (rating)
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
    #endregion
}