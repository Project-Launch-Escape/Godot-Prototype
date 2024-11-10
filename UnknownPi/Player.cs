using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 7f;
	private float _mX = 0;
	private float _mY = 0;
	private SpotLight3D _flashlight;
	private int c = 1;
	
	public override void _Input(InputEvent @event)
	{

		 if (@event is InputEventMouseMotion mouseEvent)
		{
			RotateObjectLocal(Vector3.Up, -mouseEvent.Velocity[0]/(Mathf.Pi*2000));
		}
	}
		

	public override void _PhysicsProcess(double delta)
	{
		
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta *3;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
	
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		
		MoveAndSlide();
		
		_flashlight = GetNode<SpotLight3D>("Flashlight");
		//var _node = GetNode<MeshInstance3D>("untitled/Cube_001");
		if (Input.IsActionJustPressed("flashlight"))
		{

			//GD.Print($"{GetNode<MeshInstance3D>("untitled/Cube_001")}");
			if (_flashlight.IsVisibleInTree()){_flashlight.Hide();GetNode<MeshInstance3D>("untitled/Cube_001").Hide();}
			else{_flashlight.Show();GetNode<MeshInstance3D>("untitled/Cube_001").Show();}	
		}
		if (Input.IsActionJustPressed("cameraswich"))
		{
			c++;
			if(c>1){c=-1;}
			if (c==1){
			GetNode<Camera3D>("Camera3D").Position = new Vector3(0,2.637f,2.344f);
			GetNode<Camera3D>("Camera3D").Rotation = new Vector3(-29.4f*(Mathf.Pi*2/360),0,0);}
			if (c==0){
			GetNode<Camera3D>("Camera3D").Position = new Vector3(0,1,-0.5f);
			GetNode<Camera3D>("Camera3D").Rotation = new Vector3(0,0,0);}
			if (c==-1){
			GetNode<Camera3D>("Camera3D").Position = new Vector3(0,2.637f,-2.344f);
			GetNode<Camera3D>("Camera3D").Rotation = new Vector3(-29.4f*(Mathf.Pi*2/360),Mathf.Pi,0);}
		}
	}
}
