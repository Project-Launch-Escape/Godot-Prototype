using System.Text;
using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface;
public partial class DebugUiController : Node
{
	[ExportCategory("UI References")] 
	[Export] public Label FPS;
	[Export] public Label Paused;
	[Export] public Label CurrentTime;
	[Export] public Label CurrentRate;
	[Export] public Label CurrentSpeed;
	[Export] public Label CurrentSOIs;
	[Export] public Label DistanceToCelestial;

	private const double Minute = 60;
	private const double Hour = Minute * 60;
	private const double Day = Hour * 24;
	private const double Year = Day * 365.2422d;

	private int _celestialIndex;
	private float[] _fPSPrev = new float[120];
	private int _frame;
	public override void _Process(double delta)
	{
		UpdateFPS(delta);
		Paused.Text = $"Paused: {GlobalValues.Paused}";
		UpdateCurrentTime(GlobalValues.Time);
		UpdateTimeScale(GlobalValues.TimeScale);
		UpdateSpeed(Freecam.VelocityMultiplier);
		UpdateDistance();
	}
	
	private void UpdateFPS(double delta)
	{
		_fPSPrev[_frame] = (float)(1 / delta);
		FPS.Text = $"FPS: {_fPSPrev.Average():F1}";
		_frame = (_frame + 1) % _fPSPrev.Length;
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
	
	private void UpdateSpeed(float speed)
	{
		CurrentSpeed.Text = $"Current Speed: {speed/1000:F1} km/s";
	}
	
	public void UpdateSOIs(List<CelestialScript> currentSOIs)
	{
		var sOIText = "Current SOIs: ";
		for (int i = 0; i < currentSOIs.Count; i++)
		{
			sOIText += currentSOIs[i].Name + ", ";
		}

		CurrentSOIs.Text = sOIText;
	}

	private void UpdateDistance()
	{
		var targetCelestial = GlobalValues.AllCelestials[_celestialIndex];
		var dist = targetCelestial.NestedPos.LocalPosition.DistanceTo(NestedPosition.ConvertPositionReference(Freecam.NestedPos, targetCelestial.NestedPos, CoordinateSpace.RenderSpace));
		DistanceToCelestial.Text = $"Distance to {targetCelestial.Name}: {dist/1000}km";
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Minus && _celestialIndex > 0)
			{
				_celestialIndex--;
			}
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Equal)
			{
				_celestialIndex++;
				_celestialIndex %= GlobalValues.AllCelestials.Count;
			}
		}
	}
}
