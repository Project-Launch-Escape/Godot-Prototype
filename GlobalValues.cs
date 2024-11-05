using Godot;
using System;

public partial class GlobalValues : Node
{
	[Export] public float TimeScale = 1;
	public static float time;
	public static float G = 0.000000000066743f;
	public static float[] Units = { 1f, 9007199254740992f, 137438953472f, 1024f};
	public static float Scale = 0.00000000001f;

	public static float GetRefConversionFactor(int StartingRef, int ConversionRef)
	{
		if (StartingRef >= 4)
		{
			StartingRef = 3;
		}
		if (ConversionRef >= 4)
		{
			ConversionRef = 3;
		}
		return Units[StartingRef] / Units[ConversionRef];
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = System.Convert.ToSingle(delta);
		time += dt * TimeScale;
	}
}
