using UnityEngine;
#if ENABLE_INPUT_SYSTEM
// Requires installing the InputSystem Package from the Package Manager: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Installation.html
using UnityEngine.InputSystem;
#endif

public class PlayerInputController:MonoBehaviour
{
	public PlayerInputData Current;
	public Vector2 RightStickMultiplier = new Vector2(3, -1.5f);

	private void Start()
	{ Current = new PlayerInputData(); }

	private void Update()
	{
		// Retrieve our current WASD or Arrow Key input.
		// Using GetAxisRaw removes any kind of gravity or filtering being applied to the input
		// Ensuring that we are getting either -1, 0 or 1.
		#if ENABLE_INPUT_SYSTEM
		Vector3 moveInput = new Vector3(Keyboard.current.dKey.ReadValue() - Keyboard.current.aKey.ReadValue(), 0,
			Keyboard.current.wKey.ReadValue() - Keyboard.current.sKey.ReadValue());
		Vector2 mouseInput = new Vector2(Mouse.current.delta.ReadValue().x, Mouse.current.delta.ReadValue().y);
		bool jumpInput = Keyboard.current.spaceKey.isPressed;
		#else
		Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		bool jumpInput = Input.GetButtonDown("Jump");
		#endif

		Current = new PlayerInputData() {
			MoveInput = moveInput,
			MouseInput = mouseInput,
			JumpInput = jumpInput
		};
	}
}

public struct PlayerInputData
{
	public Vector3 MoveInput;
	public Vector2 MouseInput;
	public bool JumpInput;
}