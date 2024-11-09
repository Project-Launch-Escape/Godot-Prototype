using Godot;
using System;
using System.Collections.Generic;
using CustomTypes;

public partial class GlobalValues : Node
{
	[Export] public float TimeScale = 1;
	public static float time;
	public static float G = 0.000000000066743f;
	public static float[] Units = { 1f, 281474976710656f, 4294967296f, 1024f};
	public static float Scale = 0.000000000232830643653869625849394142f; // 1/2^32
	public static List<CelestialScript> AllCelestials = new List<CelestialScript>();

	public static float GetRefConversionFactor(CoordinateSpace StartingLayer, CoordinateSpace ConversionLayer)
	{
		if ((int)StartingLayer > Units.Length-1)
		{
			StartingLayer = (CoordinateSpace)(Units.Length-1);
		}
		if ((int)ConversionLayer > Units.Length-1)
		{
			ConversionLayer = (CoordinateSpace)(Units.Length-1);
		}
		
		return Units[(int)StartingLayer] / Units[(int)ConversionLayer];
	}
	public static void RecieveCelestials(CelestialScript Celestial)
	{
		if (!AllCelestials.Contains(Celestial))
		{
			AllCelestials.Add(Celestial);
		}
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
