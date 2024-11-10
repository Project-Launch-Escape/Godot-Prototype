using System.Text;
using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public partial class DebugUiController : Node
{
    [ExportCategory("Node References")] [Export]
    public GlobalValues GlobalValues;

    [ExportCategory("UI References")] [Export]
    public Label CurrentTime;

    [Export]
    public Label CurrentRate;

    [Export]
    public Label Paused;



    private const float Minute = 60;
    private const float Hour = Minute * 60;
    private const float Day = Hour * 24;
    private const float Year = Day * 365;
    public override void _Process(double delta)
    {
        UpdateCurrentTime(GlobalValues.Time);
        UpdateTimeScale(GlobalValues.TimeScale);
        
        
        Paused.Text = $"Paused: {GlobalValues.Paused}";
    }


    private void UpdateCurrentTime(float time)
    {
        var days = time % Year;
        var years = (long)((time - days) / Year);
        CurrentTime.Text = $"Current Time: {years} yr, {days/Day:F1} d";
    }

    private void UpdateTimeScale(float scale)
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
}