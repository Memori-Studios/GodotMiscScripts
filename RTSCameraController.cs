using Godot;
using System;

public partial class RTSCameraController : Node3D
{
	[Export] private float acceleration = 25f;
	[Export] private float moveSpeed = 20f;
	[Export] private float mouseSpeed = 10f;

	private Vector3 velocity = Vector3.Zero;
	private Vector2 lookAngle = Vector2.Zero;
	
    // [Header("Camera Speeds")]
    [Export] private float cameraRotationSpeed = 0.2f;
    [Export] private float cameraYMovementSpeed = 10f, cameraPanSpeed = 1.5f;

    // [Header("Max Camera Movement")]
    [Export] private float minCameraZoom = 5f, maxCameraZoom = 25f, maxCameraHeight = 10f, minCameraHeight = 0f, cameraLerpSpeed=15, cameraZoomSpeed = 15f, staticRotationSpeed = 2f;
	private float X;
    private float Y;
    public float unlockedCameraZoom = 10f;
    [Export] public Camera3D mainCamera;
  	[Export]  public Node3D cameraTarget;
	[Export(PropertyHint.Range, "-90,0,1")] float minCamPitch = -50f;
	[Export(PropertyHint.Range, "0,90,1")] float maxCamPitch = 30f;
	Vector3 camRot = new ();

	public override void _Process(double delta) {
		
		HandleCameraRotation();
        HandleCameraZoom((float)delta);
        HandleCameraMovement((float)delta);

		Position = Position.Lerp(cameraTarget.Position, (float)delta*cameraLerpSpeed);
		Quaternion = Quaternion.Slerp(cameraTarget.Quaternion, (float)delta*cameraLerpSpeed);
	}
	public override void _Input(InputEvent @event)
	{
		if(Input.IsActionJustReleased("free_cam")){
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if(Input.IsActionJustPressed("free_cam")){
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			camRot.Y -= mouseMotion.Relative.X * cameraRotationSpeed;
			camRot.X -= mouseMotion.Relative.Y * cameraRotationSpeed;
		}
	}
	public Vector3 UpdateDirection(){
		Vector3 direction = new Vector3();
		Vector3 cameraForward = -mainCamera.GlobalTransform.Basis.Z;
		cameraForward.Y = 0;

		if(Input.IsActionPressed("move_forward")){
			direction += cameraForward;
		}
		if(Input.IsActionPressed("move_backward")){
			direction -= cameraForward;
		}
		if(Input.IsActionPressed("move_left")){
			direction += cameraForward.Cross(Vector3.Down);
		}
		if(Input.IsActionPressed("move_right")){
			direction += cameraForward.Cross(Vector3.Up);
		}

		if(direction==Vector3.Zero){
			velocity = Vector3.Zero;
		}
		return direction.Normalized();
	}
    private void HandleCameraZoom(float delta)
    {
        if(Input.IsActionJustReleased("zoom_in") && unlockedCameraZoom < maxCameraZoom) {
            unlockedCameraZoom += delta*50;
        }
        if(Input.IsActionJustReleased("zoom_out") && unlockedCameraZoom > minCameraZoom) {
            unlockedCameraZoom -= delta*50;
        }
        
        Vector3 cameraUnlockedPosition = new (0f,0f,unlockedCameraZoom);
        mainCamera.Position = mainCamera.Position.Lerp(cameraUnlockedPosition, delta*cameraZoomSpeed);
    }
    private void HandleCameraRotation() {

		if(Input.IsActionPressed("rotate_right")){
			camRot.Y += staticRotationSpeed;
		}
		if(Input.IsActionPressed("rotate_left")){
			camRot.Y -= staticRotationSpeed;
		}

		if(!Input.IsActionPressed("free_cam") && !Input.IsActionPressed("rotate_left") && !Input.IsActionPressed("rotate_right")){
            return;
		} 

		camRot.X = Mathf.Clamp(camRot.X, minCamPitch, maxCamPitch);
		cameraTarget.Quaternion = Quaternion.FromEuler( new (Mathf.DegToRad(camRot.X), Mathf.DegToRad(camRot.Y), 0));
    }
    private void HandleCameraMovement(float delta)
    {
		Vector3 direction = UpdateDirection();
		if(direction==Vector3.Zero){
			velocity = Vector3.Zero;
			return;
		}

		velocity += direction * acceleration * delta;
		if(velocity.Length()>moveSpeed){
			velocity = velocity.Normalized() * moveSpeed;
		}

		cameraTarget.GlobalTranslate(velocity * delta);	
    }
}
