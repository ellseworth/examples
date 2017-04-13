using System;
using UnityEngine;

internal class UnitCommonMove
{
	private readonly Transform _transform;

	private float _currSpeed;
	private Ray _standing;

	public bool debug;
	/// <summary>
	/// meters per second
	/// </summary>
	public float moveSpeed;
	/// <summary>
	/// degrease per second
	/// </summary>
	public float rotateSpeed;
	/// <summary>
	/// seconds
	/// </summary>
	public float accelerateTime;
	/// <summary>
	/// degrease
	/// </summary>
	public float stopAngle;

	public Ray Standing
	{
		get { return _standing; }
		set
		{
			if (value.direction == Vector3.zero)
				throw new ArgumentException("Standing.direction");
			value = new Ray(value.origin, value.direction.normalized);
			if (_standing.origin == value.origin && _standing.direction == value.direction) return;
			_standing = value;
		}
	}
	public Vector3 Position
	{
		get { return _standing.origin; }
		set
		{
			if (Position == value) return;
			Standing = new Ray(value, value - _transform.position);
		}
	}
	public Vector3 Direction
	{
		get { return _standing.direction; }
		set { Standing = new Ray(_standing.origin, value); }
	}
	public Vector3 Moving { get { return _transform.forward * _currSpeed; } }

	public UnitCommonMove(Transform transform)
	{
		if (!transform)
			throw new ArgumentNullException("transform");
		_transform = transform;
		SetStandingRigid(new Ray(transform.position, transform.forward));
	}

	private void Update(float deltaTime)
	{
		if (deltaTime < 0)
			throw new ArgumentOutOfRangeException("deltaTime");

		if (_currSpeed == 0 && _transform.position == Position && _transform.forward == Direction)
			return;

		if (debug)
			Debug.DrawRay(Position, Vector3.up * 2, Color.green);

		Vector3 direction;

		//move
		if (Position == _transform.position)
			direction = Direction;
		else
		{
			direction = Position - _transform.position;
			float angle = Vector3.Angle(_transform.forward, direction);
			float distance = direction.magnitude;
			float acceleration = moveSpeed / accelerateTime;
			float accelerationDistance = moveSpeed * accelerateTime / 2;

			float recommendStopAngle = stopAngle;
			if (distance <= accelerationDistance)
				recommendStopAngle = stopAngle * distance / accelerationDistance;

			float frameAcceleration = acceleration * deltaTime;
			float stopTime = _currSpeed / acceleration;
			float stopDistance = _currSpeed * stopTime / 2;
			if (debug) Debug.DrawRay(Position, -direction.normalized * stopDistance, Color.red);
			if (angle > recommendStopAngle)
				_currSpeed = Mathf.Max(_currSpeed - frameAcceleration, 0);
			else if (distance < stopDistance)
			{
				float recommendMoveSpeed = distance * 2 / stopTime;
				_currSpeed = recommendMoveSpeed;
			}
			else _currSpeed = Mathf.Min(_currSpeed + frameAcceleration, moveSpeed);
			Vector3 frameMove =
				_transform.forward * Mathf.Min(_currSpeed * deltaTime, direction.magnitude);
			_transform.position += frameMove;
		}

		//rotation
		Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
		Quaternion frameRotation =
			Quaternion.RotateTowards(_transform.rotation, targetRotation, rotateSpeed * deltaTime);
		_transform.rotation = frameRotation;

		if (Standing.origin == _transform.position && Standing.direction == _transform.forward)
			_currSpeed = 0;
	}

	public void SetStandingRigid(Ray standing)
	{
		_standing = standing;
		_transform.position = standing.origin;
		_transform.forward = standing.direction;
	}
	public void DropPosition()
	{
		float acceleration = moveSpeed / accelerateTime;
		float stopTime = _currSpeed / acceleration;
		float stopDistance = _currSpeed * stopTime / 2;
		Vector3 stopPosition = _transform.position + _transform.forward * stopDistance;
		Standing = new Ray(stopPosition, _transform.forward);
	}
}