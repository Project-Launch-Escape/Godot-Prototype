using Godot;

namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public class Vector3d
{
	public double X;
	public double Y;
	public double Z;

	public double Magnitude
	{
		get => GetMagnitude();
		set
		{
			var normalized = Normalized();
			X = value * normalized.X;
			Y = value * normalized.Y;
			Z = value * normalized.Z;
		}
	}
	
	public static readonly Vector3d Zero = new (0, 0, 0);
	public static readonly Vector3d One = new (1, 1, 1);
	
	public static readonly Vector3d Up = new (0, 1, 0);
	public static readonly Vector3d Down = new (0, -1, 0);
	public static readonly Vector3d Left = new (-1, 0, 0);
	public static readonly Vector3d Right = new (1, 0, 0);
	public static readonly Vector3d Forward = new (0, 0, 1);
	public static readonly Vector3d Backward = new (0, 0, -1);
	
	// Unit Vectors commonly used in astronomy
	public static readonly Vector3d K = new (0, 1, 0);
	public static readonly Vector3d I = new (0, 0, 1);
	public static readonly Vector3d J = new (1, 0, 0);
	

	public Vector3d(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3d(Vector3d vector3d)
	{
		X = vector3d.X;
		Y = vector3d.Y;
		Z = vector3d.Z;
	}
	
	public Vector3d() // Base Case
	{
		X = 0;
		Y = 0;
		Z = 0;
	}

	public static Vector3d FromSpherical(double rho,double theta, double phi)
	{
		var x = rho * Math.Sin(phi) * Math.Sin(theta);
		var z = rho * Math.Sin(phi) * Math.Cos(theta);
		var y = rho * Math.Cos(phi);

		return new Vector3d(x,y,z);
	}

	public static Vector3d FromSpherical(SphericalCoordinates sphericalcoords) =>
		FromSpherical(sphericalcoords.Rho, sphericalcoords.Theta, sphericalcoords.Phi);

	private double GetMagnitude() => Math.Sqrt(X * X + Y * Y + Z * Z);

	public double MagnitudeSquared() => X * X + Y * Y + Z * Z;

	public Vector3d Normalized()
	{
		var magnitudeSquared = MagnitudeSquared();
		if (magnitudeSquared != 0) return this / Math.Sqrt(magnitudeSquared);
		return Zero;
	}
	public Vector3d AsMagnitude(double magnitude) => Normalized() * magnitude;

	public static double Distance(Vector3d v1, Vector3d v2) => (v1 - v2).GetMagnitude();

	public double DistanceTo(Vector3d v2) => (this - v2).GetMagnitude();

	public Vector3d Cross(Vector3d with) => new(Y * with.Z - Z * with.Y, Z * with.X - X * with.Z, X * with.Y - Y * with.X);
	public double Dot(Vector3d with) => X * with.X + Y * with.Y + Z * with.Z;

	public static implicit operator string(Vector3d vector3d) => $"{vector3d.X}, {vector3d.Y}, {vector3d.Z}";

	public static implicit operator Vector3d(Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
	public static explicit operator Vector3(Vector3d vector3d) => new((float)vector3d.X, (float)vector3d.Y, (float)vector3d.Z);

	public static Vector3d operator +(Vector3d v1, Vector3d v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
	public static Vector3d operator -(Vector3d v1, Vector3d v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
	public static Vector3d operator *(Vector3d v1, Vector3d v2) => new(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
	public static Vector3d operator /(Vector3d v1, Vector3d v2) => new(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

	public static Vector3d operator *(Vector3d v1, double scalar) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(Vector3d v1, double scalar) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);
	
	public static Vector3d operator *(Vector3d v1, float scalar) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(Vector3d v1, float scalar) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);

	public static Vector3d operator *(Vector3d v1, int scalar) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(Vector3d v1, int scalar) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);

	public static Vector3d operator *(double scalar, Vector3d v1) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(double scalar, Vector3d v1) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);

	public static Vector3d operator *(float scalar, Vector3d v1) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(float scalar, Vector3d v1) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);

	public static Vector3d operator *(int scalar, Vector3d v1) => new(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	public static Vector3d operator /(int scalar, Vector3d v1) => new(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);

	public static bool operator ==(Vector3d v1, Vector3d v2) => v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
	public static bool operator !=(Vector3d v1, Vector3d v2) => v1.X != v2.X && v1.Y != v2.Y && v1.Z != v2.Z;

	public static Vector3d operator -(Vector3d v1) => new(v1.X * -1, v1.Y * -1, v1.Z * -1);
	public static Vector3d operator +(Vector3d v1) => v1;

	public bool Equals(Vector3d other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
	}

	public override bool Equals(object obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(this, obj)) return true;
		return obj.GetType() == GetType() && Equals((Vector3d)obj);
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y, Z);
	}
}
