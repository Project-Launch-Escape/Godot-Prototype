using System.Text;
using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface;
public partial class DebugUiController : Node
{
	[ExportCategory("UI References")] 
	[Export] public Label CurrentTime;
	[Export] public Label CurrentRate;
	[Export] public Label Paused;
	[Export] public Label CurrentSOIs;
	[Export] public Label FPS;
	[Export] public Label DistanceToCelestial;

	private const double Minute = 60;
	private const double Hour = Minute * 60;
	private const double Day = Hour * 24;
	private const double Year = Day * 365.2422d;

	private int celestialIndex;
	
	public override void _Process(double delta)
	{
		UpdateCurrentTime(GlobalValues.Time);
		UpdateTimeScale(GlobalValues.TimeScale);
		UpdateFPS(delta);
		UpdateDistance();
		Paused.Text = $"Paused: {GlobalValues.Paused}";
	}

	public void UpdateSOIs(List<CelestialScript> currentSOIs)
	{
		string sOIText = "Current SOIs: ";
		for (int i = 0; i < currentSOIs.Count; i++)
		{
			sOIText += currentSOIs[i].Name + " ";
		}

		CurrentSOIs.Text = sOIText;
	}

	private void UpdateFPS(double delta)
	{
		FPS.Text = $"FPS: {1 / delta}";
	}
	
	private void UpdateCurrentTime(double time)
	{
		var days = time % Year;
		var years = (long)((time - days) / Year);
		CurrentTime.Text = $"Current Time: {years} yr, {days/Day:F1} d";
	}

	private void UpdateTimeScale(double scale)
	{
		if (scale < Minute)
		{
			CurrentRate.Text = $"Current Rate: {scale:F1} s/s";
		} else if (scale < Hour)
		{
			CurrentRate.Text = $"Current Rate: {scale/Minute:F1} min/s";
		} else if (scale < Day)
		{
			CurrentRate.Text = $"Current Rate: {scale/Hour:F1} hr/s";
		} else if (scale < Year)
		{
			CurrentRate.Text = $"Current Rate: {scale/Day:F1} day/s";
		}
		else
		{
			CurrentRate.Text = $"Current Rate: {scale/Year:F1} yr/s";
		}
	}

	private void UpdateDistance()
	{
		CelestialScript targetCelestial = GlobalValues.AllCelestials[celestialIndex];
		float Dist = targetCelestial.NestedPos.LocalPosition.DistanceTo(NestedPosition.ConvertPositionReference(Freecam.NestedPos, targetCelestial.NestedPos) * targetCelestial.CoordLayer.GetConversionFactor(0));
		DistanceToCelestial.Text = $"Distance to {targetCelestial.Name}: {Dist/1000}Km";
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Minus && celestialIndex > 0)
			{
				celestialIndex--;
			}
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Equal)
			{
				celestialIndex++;
				celestialIndex %= GlobalValues.AllCelestials.Count;
			}
		}
	}
}
