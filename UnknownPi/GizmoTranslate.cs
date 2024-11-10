using Godot;
using System;

public partial class GizmoTranslate : Node3D
{
	private const float RayLength = 1000.0f;
	private int ispressed = -1;
	private int ispressedR = -1;
	private bool status =  false;
	private Node collider;
	private uint mask = 0b00000000_00000000_00000000_00000001;
	private Vector3 startRotation;
	public override void _Ready()
	{
		this.Hide();
	}
	public override void _Process(double delta)
	{		
	}
	
	public override void _Input(InputEvent @event)
	{
	if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
	{
		var camera3D = (Camera3D)(GetViewport().GetCamera3D());
		var from = camera3D.ProjectRayOrigin(eventMouseButton.Position);
		var to = from + camera3D.ProjectRayNormal(eventMouseButton.Position) * RayLength;
		var spaceState = GetWorld3D().DirectSpaceState;
		
		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = true;
		query.CollisionMask = mask;
		var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
		if (result.Count==0) {this.Hide();mask = 0b00001;return;}
		if (mask != 0b10000){mask = 0b10000;collider = (Node)result["collider"];this.Show();this.Position = ((Node3D)collider).GlobalPosition;this.Rotation =new Vector3(0,0,0);startRotation=((Node3D)collider).Rotation;return;}
		
		if ((Node)result["collider"] is StaticBody3D){
			
			StaticBody3D res = (StaticBody3D)result["collider"];
			
			Node3D p = (Node3D)res.GetParent();
			switch (res.Name)
			{
			case "RED"  :ispressed = 0;break;
			case "GREEN":ispressed = 1;break;
			case "BLUE" :ispressed = 2;break;
			
			case "Rotation Handle Red"  :ispressedR = 0;break;
			case "Rotation Handle Green"  :ispressedR = 1;break;
			case "Rotation Handle Blue"  :ispressedR = 2;break;
			}
			
		}
	} else if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && !buttonEvent.Pressed)
		{
			ispressed = -1;
			ispressedR = -1;
			this.Rotation =new Vector3(0,0,0);startRotation=((Node3D)collider).Rotation;
		}
		if (@event is InputEventMouse eve)
		{
		if (ispressed != -1){
			Node3D Gizmos = this;//((Node3D)GetNode("../GizmoTranslate"));
			Camera3D Camera = (Camera3D)(GetViewport().GetCamera3D());
			
			Vector3 A = new Vector3(0,0,0);
			if (ispressed == 0){ A = Vector3.Right;} // Red
			if (ispressed == 1){ A = Vector3.Up;} // Green
			if (ispressed == 2){ A = Vector3.Back;} // Blue
			Vector3 G = Gizmos.Position;
			Vector3 R = G+A;
			
			Vector3 C = Camera.GlobalPosition;
			C = C.Rotated(Vector3.Right, Mathf.Pi / 2);
			// The Plan formed by GRC is the one is which the gizmo is and is facing camera
			Vector3 N = (C-G).Normalized(); // Normal Vector of the plane,
			
			StaticBody3D plane = ((StaticBody3D)Gizmos.GetChild(ispressed));
			plane.LookAt(Gizmos.Position+Camera.ProjectRayNormal(eve.Position).Normalized(),A);
			Vector3 r = plane.Rotation * A;
			plane.Rotation = r;
			
			var from = Camera.ProjectRayOrigin(eve.Position);
			var to = from + Camera.ProjectRayNormal(eve.Position) * RayLength;
			var spaceState = GetWorld3D().DirectSpaceState;
			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithBodies =true;
			if (ispressed == 0){query.CollisionMask =  0b0010; }//0b00000000_00000000_00000000_00000010
			if (ispressed == 1){query.CollisionMask =  0b0100; } // Raycast Mask , so that it only collides with the right plane
			if (ispressed == 2){query.CollisionMask =  0b1000; }
			var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
			if (((StaticBody3D)result["collider"]).Name=="RedPlane" || ((StaticBody3D)result["collider"]).Name=="GreenPlane" || ((StaticBody3D)result["collider"]).Name=="BluePlane"){
				Vector3 p = (Vector3)result["position"]-Gizmos.Position; // Change Of Position
				p*=A; // Only Change Position On Axis
				Gizmos.Position += p ; // Apply Change of Position
				((Node3D)collider).GlobalPosition = Gizmos.Position;
			}
		}
		if (ispressedR != -1){
			Node3D Gizmos = this;//((Node3D)GetNode("../GizmoTranslate"));
			Camera3D Camera = (Camera3D)(GetViewport().GetCamera3D());
			
			Vector3 A = new Vector3(0,0,0);
			if (ispressedR == 0){ A = Vector3.Right;} // Red
			if (ispressedR == 1){ A = Vector3.Up;} // Green
			if (ispressedR == 2){ A = Vector3.Back;} // Blue
			Vector3 G = Gizmos.Position;
			Vector3 R = G+A;
			
			Vector3 C = Camera.GlobalPosition;
			//C = C.Rotated(Vector3.Right, Mathf.Pi / 2);
			// The Plan formed by GRC is the one is which the gizmo is and is facing camera
			//Vector3 N = (C-G).Normalized(); // Normal Vector of the plane,
			
			StaticBody3D plane = ((StaticBody3D)Gizmos.GetChild(ispressed));
			//plane.LookAt(Gizmos.Position+Camera.ProjectRayNormal(eve.Position).Normalized(),A);
			//Vector3 r = plane.Rotation * A;
			//plane.Rotation = r;
			
			var from = Camera.ProjectRayOrigin(eve.Position);
			var to = from + Camera.ProjectRayNormal(eve.Position) * RayLength;
			var spaceState = GetWorld3D().DirectSpaceState;
			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithBodies =true;
			if (ispressedR == 0){query.CollisionMask =  0b00100000; }
			if (ispressedR == 1){query.CollisionMask =  0b01000000; }
			if (ispressedR == 2){query.CollisionMask =  0b10000000; }
			var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
			if (((StaticBody3D)result["collider"]).Name=="RedPlaneR" || ((StaticBody3D)result["collider"]).Name=="GreenPlaneR" || ((StaticBody3D)result["collider"]).Name=="BluePlaneR"){
				Vector3 p = (Vector3)result["position"]-Gizmos.Position; // Change Of Position
				p= ((A - new Vector3(1,1,1))*new Vector3(-1,-1,-1)*p).Normalized(); // Only Change Position On Axis
				float a = 0;
				if (ispressedR == 0){a=-(Mathf.Atan2(p.Y, p.Z));}
				if (ispressedR == 1){a=-(Mathf.Atan2(p.Z, p.X));}
				if (ispressedR == 2){a=-(Mathf.Atan2(p.X, p.Y));}
				Gizmos.Rotation = A*a; // Apply Change of Position
				((Node3D)collider).Rotation = startRotation;
				((Node3D)collider).Rotate(A, a);;//= startRotation + A*a;
				}
		}
		}
}
}
