
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.Simulation.Physics;

public abstract class OrbitalElements
{
    public ConicType OrbitType;

    public double p; //semi-parameter (semi-latus rectum)
    public double e; //eccentricity

    public double w; // argument of periapsis
    public double i; // inlination
    public double l; // longitude of ascending node

    public double n; // mean motion (rad/s)
    
    public double T; // Time of periapsis passage
    
    private double _anomalyPrev; // Eccentric, parabolic, or hyperbolic anomaly (depending on OrbitType)
    
    
    public double Apoapsis => GetApoapsis();
    public double Periapsis => GetPeriapsis();

    public double a => GetSemiMajorAxis();
    public double b => GetSemiMinorAxis();

    public Range DistanceRange => GetDistanceRange();

    public Vector3d NormalVector => GetNormalVector();
    
    
    private double GetApoapsis()
    {
        if (OrbitType is ConicType.Parabolic or ConicType.Hyperbolic)
        {
            return double.PositiveInfinity;
        }
        return p / (1 - e);
    }
    private double GetPeriapsis() => p / (1 + e);

    private double GetSemiMajorAxis() => p / (1 - e * e);
    private double GetSemiMinorAxis() => p / Math.Sqrt(1 - e * e);

    private Range GetDistanceRange() => new (Periapsis, Apoapsis);

    private Vector3d GetNormalVector() => new (Math.Cos(w) * Math.Sin(i), Math.Cos(i), Math.Sin(w) * Math.Sin(i));

    
    public double TrueAnomalyFromMeanAnomaly(double m, bool useAnomalyPrev = false) => TrueAnomalyFromAnomaly(AnomalyFromMeanAnomaly(m, useAnomalyPrev));

	public double MeanAnomalyFromTrueAnomaly(double v) => MeanAnomalyFromAnomaly(AnomalyFromTrueAnomaly(v));

	public double TrueAnomalyFromTime(double time, bool useAnomalyPrev = false) => TrueAnomalyFromMeanAnomaly(MeanAnomalyFromTime(time), useAnomalyPrev);

	public double MeanAnomalyFromTime(double time) => n * (time - T);

	public double AnomalyFromTime(double time, bool useAnomalyPrev = false) => AnomalyFromMeanAnomaly(MeanAnomalyFromTime(time), useAnomalyPrev);

	public double TimeFromMeanAnomaly(double m) => m / n + T;

	public double TimeFromAnomaly(double anomaly) => TimeFromMeanAnomaly(MeanAnomalyFromAnomaly(anomaly));

	public double TimeFromTrueAnomaly(double v) => TimeFromAnomaly(AnomalyFromTrueAnomaly(v));
	 
	// Only gives values from 0-pi, subtract the result from Tau to get the pi-Tau answer
	public double TrueAnomalyFromDistance(double distance) => Math.Acos((p / distance - 1) / e);

	public double TrueAnomalyFromAnomaly(double anomaly)
	{
		switch (OrbitType)
		{
			case ConicType.Circular:
				return anomaly;
			case ConicType.Elliptical:
			{
				var sinv = Math.Sqrt(1 - e * e) * Math.Sin(anomaly) / (1 - e * Math.Cos(anomaly));
				var cosv = (Math.Cos(anomaly) - e) / (1 - e * Math.Cos(anomaly));
				return Math.Atan2(sinv, cosv);
			}
			case ConicType.Hyperbolic:
			{
				var sinv = -(Math.Sqrt(e * e - 1) * Math.Sinh(anomaly)) / (1 - e * Math.Cosh(anomaly));
				var cosv = (Math.Cosh(anomaly) - e) / (1 - e * Math.Cosh(anomaly));
				return Math.Atan2(sinv, cosv);
			} 
			case ConicType.Parabolic:
				return 2 * Math.Atan(2 * anomaly);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public double AnomalyFromMeanAnomaly(double m, bool useAnomalyPrev = false)
	{
		var anomaly = useAnomalyPrev? _anomalyPrev : m;

		const float tolerance = 0.00001f;
		
		switch (OrbitType)
		{
			case ConicType.Circular:
				if (useAnomalyPrev) _anomalyPrev = m;
				return m;
			case ConicType.Elliptical:
			{
				for (int j = 0; j < 10; j++)
				{
					var dE = (m - anomaly + e * Math.Sin(anomaly)) / (1 - e * Math.Cos(anomaly));
					anomaly += dE;
					
					if (Math.Abs(dE) < tolerance) break;
				}
				if (useAnomalyPrev) _anomalyPrev = anomaly;
				return anomaly;
			}
			case ConicType.Hyperbolic:
			{
				for (int j = 0; j < 10; j++)
				{
					var dH = (m + anomaly - e * Math.Sinh(anomaly)) / (e * Math.Cosh(anomaly) - 1);
					anomaly += dH;
					
					if (Math.Abs(dH) < tolerance) break;
				}
				if (useAnomalyPrev) _anomalyPrev = anomaly;
				return anomaly;
			}
			case ConicType.Parabolic:
				return Math.Tan(Math.PI / 2 - 2 * Math.Atan(Math.Pow(Math.Tan(Math.PI /2 - Math.Atan(1.5 * m)) / 2, 1d / 3d)));
			
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public double AnomalyFromTrueAnomaly(double v)
	{
		switch (OrbitType)
		{
			case ConicType.Circular:
				return v;
			
			case ConicType.Elliptical:
				var sinE = Math.Sin(v) * Math.Sqrt(1 - e * e) / (1 + e * Math.Cos(v));
				var cosE = (e + Math.Cos(v)) / (1 + e * Math.Cos(v));
				return Math.Atan2(sinE, cosE);
			
			case ConicType.Hyperbolic:
				var sinhH = Math.Sin(v) * Math.Sqrt(e * e - 1) / (1 + e * Math.Cos(v));
				return Math.Asinh(sinhH);
			
			case ConicType.Parabolic:
				return Math.Tan(v / 2);
			
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public double MeanAnomalyFromAnomaly(double anomaly)
	{
		return OrbitType switch
		{
			ConicType.Circular => anomaly,
			ConicType.Elliptical => anomaly - e * Math.Sin(anomaly),
			ConicType.Hyperbolic => e * Math.Sinh(anomaly) - anomaly,
			ConicType.Parabolic => anomaly + anomaly * anomaly * anomaly / 3,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	protected ConicType GetConicType()
	{
		const double tolerance = 0.00001;
		if (e < tolerance)
		{
			return ConicType.Circular;
		}
		if (Math.Abs(e - 1) < tolerance)
		{
			return ConicType.Parabolic;
		}
		if (e < 1)
		{
			return ConicType.Elliptical;
		}
		return ConicType.Hyperbolic;
	}
    
}