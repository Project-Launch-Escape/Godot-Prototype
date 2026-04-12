using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;

namespace GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

public partial class CelestialMarker : OrbitMarker
{
	[Export] private Label _nameText;
	[Export] private Control _hoverDisplay;
	
	public override void _Ready()
	{
		AddToIconList();
		
		_nameText.Text = ParentOrbit.OrbitingObject.Name;
		if (ParentOrbit.OrbitingObject is Celestial celestial)
		{

			Modulate = celestial.IconColor;
		}
	}

	public override void _Process(double delta)
	{
		_renderspacePosition = ((Node3D)ParentOrbit.OrbitingObject).GlobalPosition;
		Visible = GetVisibility();
		base._Process(delta);
	}
	
	
	public override void OnHoverEnter()
	{
		Scale = Vector2.One * 1.25f;
		_hoverDisplay.Visible = true;
	}
	public override void OnHoverExit()
	{
		Scale = Vector2.One * 1f;
		_hoverDisplay.Visible = false;
	}
}
