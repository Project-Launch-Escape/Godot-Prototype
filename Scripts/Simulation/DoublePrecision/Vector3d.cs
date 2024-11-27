using Godot;

namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public class Vector3d
{
	protected bool Equals(Vector3d other)
	{
		return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
	}

	public override bool Equals(object obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Vector3d)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y, Z);
	}

	public double X;
	public double Y;
	public double Z;
	
	public static Vector3d Zero = new (0, 0, 0);
	public static Vector3d One { get; } = new (1, 1, 1);
	public static Vector3d Up { get; } = new (0, 1, 0);
	public static Vector3d Down { get; } = new (0, -1, 0);
	public static Vector3d Left { get; } = new (-1, 0, 0);
	public static Vector3d Right { get; } = new (1, 0, 0);
	public static Vector3d Forward { get; } = new (0, 0, 1);
	public static Vector3d Backward { get; } = new (0, 0, -1);

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

	public double Magnitude()
	{
		return Math.Sqrt(X * X + Y * Y + Z * Z);
	}
	public double MagnitudeSquared()
	{
		return X * X + Y * Y + Z * Z;
	}
	public Vector3d Normalized()
	{
		var magnitudeSquared = MagnitudeSquared();
		if (magnitudeSquared != 0) return this / Math.Sqrt(magnitudeSquared);
		return Zero;
	}
	public static double Distance(Vector3d v1, Vector3d v2)
	{
		return (v1 - v2).Magnitude();
	}
	public double DistanceTo(Vector3d v2)
	{
		return (this - v2).Magnitude();
	}

	public Vector3d Cross(Vector3d with)
	{
		return new Vector3d(Y * with.Z - Z * with.Y, Z * with.X - X * with.Z, X * with.Y - Y * with.X);
	}
	public double Dot(Vector3d with)
	{
		return X * with.X + Y * with.Y + Z * with.Z;
	}
	
	public static implicit operator string(Vector3d vector3d)
	{
		return $"{vector3d.X}, {vector3d.Y}, {vector3d.Z}";
	}
	
	public static implicit operator Vector3d(Vector3 vector3)
	{
		return new Vector3d(vector3.X, vector3.Y, vector3.Z);
	}
	public static explicit operator Vector3(Vector3d vector3d)
	{
		return new Vector3((float)vector3d.X, (float)vector3d.Y, (float)vector3d.Z);
	}
	
	public static Vector3d operator +(Vector3d v1, Vector3d v2)
	{
		return new Vector3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
	}
	public static Vector3d operator -(Vector3d v1, Vector3d v2)
	{
		return new Vector3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
	}
	public static Vector3d operator *(Vector3d v1, Vector3d v2)
	{
		return new Vector3d(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
	}
	public static Vector3d operator /(Vector3d v1, Vector3d v2)
	{
		return new Vector3d(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
	}
	
	public static Vector3d operator *(Vector3d v1, double scalar)
	{
		return new Vector3d(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	}
	public static Vector3d operator /(Vector3d v1, double scalar)
	{
		return new Vector3d(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);
	}
	public static Vector3d operator *(Vector3d v1, float scalar)
	{
		return new Vector3d(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	}
	public static Vector3d operator /(Vector3d v1, float scalar)
	{
		return new Vector3d(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);
	}
	public static Vector3d operator *(Vector3d v1, int scalar)
	{
		return new Vector3d(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
	}
	public static Vector3d operator /(Vector3d v1, int scalar)
	{
		return new Vector3d(v1.X / scalar, v1.Y / scalar, v1.Z / scalar);
	}
	
	public static bool operator ==(Vector3d v1, Vector3d v2)
	{
		return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
	}
	public static bool operator !=(Vector3d v1, Vector3d v2)
	{
		return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
	}
	
	public static Vector3d operator -(Vector3d v1)
	{
		return new Vector3d(v1.X * -1, v1.Y * -1, v1.Z * -1);
	}
	public static Vector3d operator +(Vector3d v1)
	{
		return v1;
	}
}
