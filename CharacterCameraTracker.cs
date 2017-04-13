using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraTracker : MonoBehaviour
{
	private struct CalculateLerpResult<T>
	{
		public T from, to;
		public float rate;
	}

	[SerializeField] private float _pivotHeight = 1.2f;
	[SerializeField] private List<float> _degreases = new List<float> { -10 };
	[SerializeField] private List<Vector3> _offsets = new List<Vector3> { new Vector3(0, 0.5f, -2) };
	[SerializeField] private float _moveExtrapolateMaxDistance = 2;
	[SerializeField] private float _targetExtrapolateMaxSpeed = 8;
	[SerializeField] private float _rotationSencivity = 1;
	[SerializeField] private float _elevationSencivity = 1;
	[SerializeField] [Range(0, 1)] private float _startCameraHeightRate = 0.5f;

	private Vector3 _lastTargetPosition;

	private Transform _target;
	public Transform Target
	{
		get { return _target; }
		set
		{
			if (_target == value) return;
			_target = value;
			if (value) _lastTargetPosition = value.position;
			enabled = value;
		}
	}

	private Vector2 _orientation;
	private Vector2 Orientation
	{
		get { return _orientation; }
		set
		{
			float x = Mathf.Repeat(value.x, 1);
			float y = Mathf.Clamp01(value.y);
			value = new Vector2(x, y);
			_orientation = value;
		}
	}

	private void OnEnable()
	{
		InputCatch.CameraRotateDrag += OnCameraRotate;
		InputCatch.CameraElevationDrag += OnCameraElevate;
	}
	private void OnDisable()
	{
		InputCatch.CameraRotateDrag -= OnCameraRotate;
		InputCatch.CameraElevationDrag -= OnCameraElevate;
	}
	private void Start()
	{
		_orientation.y = _startCameraHeightRate;
	}
	private void LateUpdate()
	{
		if (!_target)
		{
			enabled = false;
			return;
		}

		//pivot
		Vector3 targetMove = (_target.position - _lastTargetPosition) / Time.deltaTime;
		targetMove.y = 0;
		_lastTargetPosition = _target.position;
		float extrapolateDistance =
			_moveExtrapolateMaxDistance * Mathf.Min(1, targetMove.magnitude / _targetExtrapolateMaxSpeed);
		Vector3 pivot =
			_target.position + new Vector3(0, _pivotHeight, 0) +
			targetMove.normalized * extrapolateDistance;

		//camera pos
		Vector3 cameraOffsetFromPivot;
		if (_offsets.Count == 0) cameraOffsetFromPivot = Vector3.zero;
		else if (_offsets.Count == 1) cameraOffsetFromPivot = _offsets[0];
		else
		{
			CalculateLerpResult<Vector3> lerpParams = CalculateFromRate(_offsets, _orientation.y);
			cameraOffsetFromPivot =
				Vector3.Lerp(lerpParams.from, lerpParams.to, lerpParams.rate);
		}

		Quaternion cameraYRotation = Quaternion.Euler(0, 360 * _orientation.x, 0);
		Vector3 cameraOffsetYRotated = cameraYRotation * cameraOffsetFromPivot;

		Vector3 cameraPosition = pivot + cameraOffsetYRotated;

		float xDegree;
		if (_degreases.Count == 0) xDegree = -15;
		else if (_degreases.Count == 1) xDegree = _degreases[0];
		else
		{
			CalculateLerpResult<float> lerpParams = CalculateFromRate(_degreases, _orientation.y);
			xDegree = Mathf.Lerp(lerpParams.from, lerpParams.to, lerpParams.rate);
		}
		Quaternion cameraRotation =  cameraYRotation * Quaternion.Euler(xDegree, 0, 0);

		transform.position = cameraPosition;
		transform.rotation = cameraRotation;
	}

	private CalculateLerpResult<T> CalculateFromRate<T>(List<T> list, float rate)
	{
		rate = Mathf.Clamp01(rate);

		CalculateLerpResult<T> result = new CalculateLerpResult<T>();
		if (rate == 0)
		{
			result.from = list[0];
			result.to = list[1];
			result.rate = 0;
		}
		else if (rate == 1)
		{
			result.from = list[list.Count - 2];
			result.to = list[list.Count - 1];
			result.rate = 1;
		}
		else
		{
			float rateInListLength = rate * (list.Count - 1);
			int index1 = Mathf.FloorToInt(rateInListLength);
			int index2 = Mathf.CeilToInt(rateInListLength);

			result.from = list[index1];
			result.to = list[index2];
			result.rate = rateInListLength - index1;
		}

		return result;
	}
	private void OnCameraRotate(float delta)
	{
		float newRotation = Orientation.x - delta * _rotationSencivity;
		Orientation = new Vector2(newRotation, Orientation.y);
	}
	private void OnCameraElevate(float delta)
	{
		float newElevation = Orientation.y + delta * _elevationSencivity;
		Orientation = new Vector2(Orientation.x, newElevation);
	}
}
