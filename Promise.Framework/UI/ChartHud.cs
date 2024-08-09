using Godot;
using Godot.Collections;

namespace Promise.Framework.UI
{
    /// <summary>
    /// A reference to other UI elements for a specific ChartController.
    /// </summary>
    public partial class ChartHud : Control
    {
        /// <summary>
        /// A reference to the combo display.
        /// </summary>
        [ExportGroup("References"), Export] public ComboDisplay ComboDisplay;
        
        /// <summary>
        /// A reference to the judgment sprite.
        /// </summary>
        [Export] public Judgment Judgment;
        
        /// <summary>
        /// A reference to the HitDistance label.
        /// </summary>
        [Export] public HitDistance HitDistance;

        /// <summary>
        /// Triggers in the case the ChartController ever changes scroll paths.
        /// </summary>
        /// <param name="downScroll">If the ChartController is down scroll.</param>
        public void SwitchDirection(bool downScroll)
        {
            Array<Node> children = GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Node2D node2DChild)
                    node2DChild.Position = new Vector2(node2DChild.Position.X, Mathf.Abs(node2DChild.Position.Y) * (downScroll ? -1f : 1f));
                else if (children[i] is Control controlChild)
                    controlChild.Position = new Vector2(controlChild.Position.X, Mathf.Abs(controlChild.Position.Y) * (downScroll ? -1f : 1f));
            }
        }
    }
}