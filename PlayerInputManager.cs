using Godot;
using System;


public partial class PlayerInputManager : Node3D
{
	int rayLength = 1000;
	Vector2 mousePosition = new Vector2();
	[Export] public Node3D cursor;
	[Export] public Vector3 cursorTarget;
	[Export] public float cursorLerpSpeed = 0.1f;
	PackedScene building = (PackedScene)ResourceLoader.Load("res://Data/Blocks/water.tscn");
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Visible) {
			Camera3D mainCamera = GetNode<Camera3D>("Camera3D");
			var mousePosition = GetViewport().GetMousePosition();
			var from = mainCamera.ProjectRayOrigin(mousePosition);
			var to = from + mainCamera.ProjectRayNormal(mousePosition) * rayLength;
			var spaceState = GetWorld3D().DirectSpaceState;
			var rayQuery = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 1);
			rayQuery.CollideWithAreas = true;

			var result = spaceState.IntersectRay(rayQuery);

			if (result.Count > 0) {
				Node3D colliderNode = (Node3D)result["collider"];
				if (colliderNode != null) {
					cursorTarget = colliderNode.GlobalTransform.Origin + new Vector3(0,0.25f,0);
				}
			}
		}

		if (Input.IsActionJustPressed("mouse_click")) {
			var newBuilding = (Node3D)building.Instantiate();	
			newBuilding.GlobalTransform = new Transform3D(Basis.Identity, cursorTarget);
        }
	}

	public override void _Process(double delta) {
		MoveCursor((float)delta);
	}
	private void MoveCursor(float delta){
		cursor.Position = cursor.Position.Lerp(cursorTarget, delta*cursorLerpSpeed);
	}
}
