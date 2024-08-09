using System.Collections.Generic;
using Godot;

namespace Promise.Framework.UI
{
	/// <summary>
	/// Used to show the current combo for a ChartController.
	/// </summary>
    public partial class ComboDisplay : Control
    {
	    #region Exported Variables
	    /// <summary>
	    /// The spacing for every combo sprite.
	    /// </summary>
        [ExportGroup("Settings"), Export] public float Spacing = 100;

	    /// <summary>
	    /// The textures supplied for this ComboDisplay. Should go from 0 to 9.
	    /// </summary>
        [ExportGroup("Textures"), Export] public Texture2D[] Textures;

	    /// <summary>
	    /// A list of every tween running for the combo sprites. Clears itself when UpdateCombo is run.
	    /// </summary>
        protected List<Tween> TweenList = new List<Tween>();
	    #endregion

	    #region Public Methods
        /// <summary>
        /// Is ran every time the linked ChartController either hits or misses a note.
        /// </summary>
        /// <param name="combo">The current combo</param>
        public void UpdateCombo(uint combo)
        {
	        for (int i = 0; i < TweenList.Count; i++)
		        TweenList[i].Kill();

	        TweenList.Clear();

	        int[] splitDigits = new int[combo.ToString().Length];
	        for (int i = 0; i < splitDigits.Length; i++)
		        splitDigits[i] = int.Parse(combo.ToString().ToCharArray()[i].ToString());

	        int childCount = GetChildCount();
	        for (int i = 0; i < childCount; i++)
		        GetChild<TextureRect>(i).Modulate = new Color(0, 0, 0, 0);

	        if (splitDigits.Length <= childCount)
	        {
		        for (int i = 0; i < splitDigits.Length; i++)
		        {
			        TextureRect comboSpr = GetChild<TextureRect>(i);

			        comboSpr.Texture = Textures[splitDigits[i]];
			        comboSpr.Size = comboSpr.Texture.GetSize();
			        comboSpr.PivotOffset =
				        new Vector2(comboSpr.Texture.GetWidth() / 2f, comboSpr.Texture.GetHeight() / 2f);
			        comboSpr.Modulate = new Color(1, 1, 1, 0.5f);
			        comboSpr.Scale = Vector2.One;
		        }
	        }
	        else if (splitDigits.Length > childCount)
	        {
		        for (int i = 0; i < childCount; i++)
		        {
			        TextureRect comboSpr = GetChild<TextureRect>(i);

			        comboSpr.Texture = Textures[splitDigits[i]];
			        comboSpr.Size = comboSpr.Texture.GetSize();
			        comboSpr.PivotOffset =
				        new Vector2(comboSpr.Texture.GetWidth() / 2f, comboSpr.Texture.GetHeight() / 2f);
			        comboSpr.Modulate = new Color(1, 1, 1, 0.5f);
			        comboSpr.Scale = Vector2.One;
		        }

		        for (int i = childCount; i < splitDigits.Length; i++)
		        {
			        TextureRect comboSpr = new TextureRect();

			        comboSpr.Texture = Textures[splitDigits[i]];
			        comboSpr.Size = comboSpr.Texture.GetSize();
			        comboSpr.PivotOffset =
				        new Vector2(comboSpr.Texture.GetWidth() / 2f, comboSpr.Texture.GetHeight() / 2f);
			        comboSpr.Modulate = new Color(1, 1, 1, 0.5f);
			        comboSpr.Scale = Vector2.One;
			        comboSpr.UseParentMaterial = true;
			        AddChild(comboSpr);
		        }
	        }

	        float generalSize = Spacing;
	        for (int i = 0; i < splitDigits.Length; i++)
	        {
		        TextureRect comboSpr = GetChild<TextureRect>(i);
		        comboSpr.Position = new Vector2(i * generalSize - ((splitDigits.Length - 1) * generalSize / 2), 0) -
		                            comboSpr.PivotOffset;
		        
		        ComboAnimation(comboSpr);
	        }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Runs this animation for every combo sprite shown on-screen. Can be overridden by inheriting classes.
        /// </summary>
        /// <param name="comboSpr">The combo sprite</param>
        public virtual void ComboAnimation(TextureRect comboSpr)
        {
	        comboSpr.Modulate = new Color(comboSpr.Modulate.R, comboSpr.Modulate.G, comboSpr.Modulate.B, 0.5f);
	        comboSpr.Scale = new Vector2(1.2f, 1.2f);

	        Tween comboTwn = comboSpr.CreateTween();
	        comboTwn.TweenProperty(comboSpr, "scale", Vector2.One, 0.2);
	        comboTwn.TweenProperty(comboSpr, "modulate", new Color(1f, 1f, 1f, 0f), 1).SetDelay(0.8);
	        TweenList.Add(comboTwn);
        }
        #endregion
    }
}