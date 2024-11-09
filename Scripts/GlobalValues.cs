using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts;

public partial class GlobalValues : Node
{
	[Export] public float TimeScale = 1;
	public static float Time;
	public const float G = 0.000000000066743f;
	public static readonly float[] Units = [1f, 281474976710656f, 4294967296f, 1024f];
	public const float Scale = 0.000000000232830643653869625849394142f; // 1/2^32
	public static readonly List<CelestialScript> AllCelestials = [];

	private bool _paused = false;

	public static float GetRefConversionFactor(CoordinateSpace startingLayer, CoordinateSpace conversionLayer)
	{
		if ((int)startingLayer > Units.Length-1)
		{
			startingLayer = (CoordinateSpace)(Units.Length-1);
		}
		if ((int)conversionLayer > Units.Length-1)
		{
			conversionLayer = (CoordinateSpace)(Units.Length-1);
		}
		
		return Units[(int)startingLayer] / Units[(int)conversionLayer];
	}
	public static void ReceiveCelestials(CelestialScript celestial)
	{
		if (!AllCelestials.Contains(celestial))
		{
			AllCelestials.Add(celestial);
		}
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_paused) return;
		var dt = Convert.ToSingle(delta);
		Time += dt * TimeScale;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Space)
			{
				_paused = !_paused;
			}
		}
	}
}