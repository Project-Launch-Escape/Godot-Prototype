namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public readonly struct Angle
{
	private readonly double _theta;

	private Angle(double trueAnomaly)
	{
		_theta = trueAnomaly;
	}
	public static readonly Angle PiHalf = (Angle)(3.14159265358979323846 / 2);
	public static readonly Angle Pi = (Angle)3.14159265358979323846;
	public static readonly Angle Tau = (Angle)6.283185307179586476925;
	public static readonly Angle Zero = (Angle)0;

	public static Angle Abs(Angle theta) => (Angle)Math.Abs((double)theta);

	public static double Sin(Angle theta) => Math.Sin((double)theta);
	public static double Cos(Angle theta) => Math.Cos((double)theta);
	public static double Tan(Angle theta) => Math.Tan((double)theta);
	
	public static double Sinh(Angle theta) => Math.Sinh((double)theta);
	public static double Cosh(Angle theta) => Math.Cosh((double)theta);
	public static double Tanh(Angle theta) => Math.Tanh((double)theta);
	
	public static Angle Asin(double x) => (Angle)Math.Asin(x);
	public static Angle Acos(double x) => (Angle)Math.Acos(x);
	public static Angle Atan(double x) => (Angle)Math.Atan(x);
	public static Angle Atan2(double y, double x) => (Angle)Math.Atan2(y, x);
	
	public static Angle Asinh(double x) => (Angle)Math.Asinh(x);
	public static Angle Acosh(double x) => (Angle)Math.Acosh(x);
	public static Angle Atanh(double x) => (Angle)Math.Atanh(x);
	
	public static explicit operator double(Angle theta) => theta._theta;
	public static explicit operator Angle(double theta) => new(theta);
	
	public static implicit operator string(Angle theta) => (double)theta + "rad";

	public static Angle operator +(Angle theta1, Angle theta2) => (Angle)((double)theta1 + (double)theta2);
	public static Angle operator -(Angle theta1, Angle theta2) => (Angle)((double)theta1 - (double)theta2);
	public static Angle operator *(Angle theta1, Angle theta2) => (Angle)((double)theta1 * (double)theta2);
	public static double operator /(Angle theta1, Angle theta2) => (double)theta1 / (double)theta2;
	
	public static Angle operator *(double x, Angle theta) => (Angle)(x * (double)theta);
	
	public static Angle operator *(Angle theta, double x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, double x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, int x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, int x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, uint x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, uint x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, long x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, long x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, ulong x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, ulong x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, short x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, short x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, ushort x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, ushort x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, byte x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, byte x) => (Angle)((double)theta / x);
	
	public static Angle operator *(Angle theta, sbyte x) => (Angle)(x * (double)theta);
	public static Angle operator /(Angle theta, sbyte x) => (Angle)((double)theta / x);
}