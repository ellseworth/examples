using UnityEngine;

internal class CameraMove : MonoSingletone
{
	[SerializeField] private Vector2 _heghtClamp = new Vector2(2, 10);
	[SerializeField] private Vector2 _angleClamp = new Vector2(-10, -60);
	[SerializeField] private float _backDistance = 3;
	[SerializeField] private float _verticalMoveSpeed = 1;
	[SerializeField] private float _rotateSpeed = 1;
	[SerializeField] private Camera _camera;

	private Transform _camTransform;
	private float _currAngle;

	protected override void AwakeActions() { }
	protected override void DestroyActions() { }

	private void OnEnable()
	{
		_camera = _camera ?? GetComponent<Camera>() ?? Camera.main;
		if(!_camera)
		{
			Debug.LogError("can't find camera component, disable self", gameObject);
			enabled = false;
			return;
		}
		_camTransform = _camera.transform;
	}
	private void Update ()
	{
		float position = transform.localPosition.y;
		position -= GameInput.CameraMoveDrag.y * _verticalMoveSpeed;
		position = Mathf.Clamp(position, _heghtClamp.x, _heghtClamp.y);

		float positionRate = (position - _heghtClamp.x) / (_heghtClamp.y - _heghtClamp.x);
		float vertRotation = Mathf.Lerp(_angleClamp.x, _angleClamp.y, positionRate);

		float horRotation = transform.eulerAngles.y;
		horRotation += GameInput.CameraMoveDrag.x * _rotateSpeed;

		transform.localPosition = new Vector3(0, position, 0);
		transform.eulerAngles = new Vector3(vertRotation, horRotation, 0);

		_camTransform.rotation = transform.rotation;
		_camTransform.position = transform.position - transform.forward * _backDistance;
	}
}
