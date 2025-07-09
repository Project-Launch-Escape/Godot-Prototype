
using System;
using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public class SphericalCoordinatesF
{
	public float Radius;
	
	public float Theta; //xz plane anglular displacement
	public float Phi; //y axis angular displacement
	
	public float Rho { get => Radius; set => Radius = value; } //Common mathematical symbol

	public float Azimuth { get => Theta; set => Theta = value; } //Alternative names used in astronomy
	public float Altitude { get => Phi; set => Phi = value; } //Alternative names used in astronomy
	
	public float RightAscension { get => Theta; set => Theta = value; } //Alternative names used in astronomy
	public float Declination { get => Phi; set => Phi = value; } //Alternative names used in astronomy
	
	public float Longitude { get => Theta; set => Theta = value; } //Planetary surface coordinates
	public float Latitude { get => Phi; set => Phi = value; } //Planetary surface coordinates

	public SphericalCoordinatesF(float radius, float theta, float phi)
	{
		Radius = radius;
		Theta = theta;
		Phi = phi;
	}

	public static SphericalCoordinatesF FromCartesian(Vector3 cartesianPos)
	{
		var radius = cartesianPos.Length();
		var theta = Mathf.Atan2(cartesianPos.Z, cartesianPos.X);
		var phi = Mathf.Acos(cartesianPos.Y / radius);
		
		return new SphericalCoordinatesF(radius, theta, phi);
	}

	public Vector3 ToCartesian()
	{
		var x = Radius * Mathf.Sin(Phi) * Mathf.Cos(Theta);
		var z = Radius * Mathf.Sin(Phi) * Mathf.Sin(Theta);
		var y = Radius * Mathf.Cos(Phi);

		return new Vector3(x,y,z);
	}

	public static SphericalCoordinatesF FromEuler(Vector3 eulerAngles) =>
		new(1, 3 * MathF.PI / 2 - eulerAngles.X, -eulerAngles.Y - MathF.PI / 2);

	public Vector3 ToEuler() => new(3 * MathF.PI / 2 - Phi, -Theta - MathF.PI / 2, 0);

	public static implicit operator string(SphericalCoordinatesF sphereCoords)
	{
		return $"r: {sphereCoords.Radius}, θ: {sphereCoords.Theta}, φ: {sphereCoords.Phi}";
	}
}
