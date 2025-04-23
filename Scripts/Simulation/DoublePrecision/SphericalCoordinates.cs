
using Godot;

namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public class SphericalCoordinates
{
	public double Radius;
	
	public double Theta; //xz plane anglular displacement
	public double Phi; //y axis angular displacement
	
	public double Rho { get => Radius; set => Radius = value; } //Common mathematical symbol

	public double Azimuth { get => Theta; set => Theta = value; } //Alternative names used in astronomy
	public double Altitude { get => Phi; set => Phi = value; } //Alternative names used in astronomy
	
	public double RightAscension { get => Theta; set => Theta = value; } //Alternative names used in astronomy
	public double Declination { get => Phi; set => Phi = value; } //Alternative names used in astronomy
	
	public double Longitude { get => Theta; set => Theta = value; } //Planetary surface coordinates
	public double Latitude { get => Phi; set => Phi = value; } //Planetary surface coordinates

	public SphericalCoordinates(double radius, double theta, double phi)
	{
		Radius = radius;
		Theta = theta;
		Phi = phi;
	}

	public static SphericalCoordinates FromCartesian(Vector3d cartesianPos)
	{
		var radius = cartesianPos.Magnitude;
		var theta = Math.Atan2(cartesianPos.Z, cartesianPos.X);
		var phi = Math.Acos(cartesianPos.Y / radius);
		
		return new SphericalCoordinates(radius, theta, phi);
	}

	public Vector3d ToCartesian()
	{
		var x = Radius * Math.Sin(Phi) * Math.Cos(Theta);
		var z = Radius * Math.Sin(Phi) * Math.Sin(Theta);
		var y = Radius * Math.Cos(Phi);

		return new Vector3d(x,y,z);
	}

	public Vector3 ToEuler() => new(3 * MathF.PI / 2 - (float)Phi, -(float)Theta - MathF.PI / 2, 0);

	public static implicit operator string(SphericalCoordinates sphereCoords)
	{
		return $"r: {sphereCoords.Radius}, θ: {sphereCoords.Theta}, φ: {sphereCoords.Phi}";
	}
}
