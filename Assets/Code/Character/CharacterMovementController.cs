using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
    private const string SPEED_PARAM = "Speed";
    private static readonly int SPEED_HASH = Animator.StringToHash(SPEED_PARAM);

    [SerializeField] private Animator characterAnimator = null;
    [SerializeField] private float movementSpeed = 2.0f;
    [SerializeField, Min(1f)] private float rotationSpeed = 10f;

    private Coroutine movementCoroutine = null;

    public bool IsMoving => movementCoroutine != null;

    private void OnDisable()
    {
        stopMovement(); // Ensure movement stops when the object is disabled and references are cleared
    }

    /// <summary> Moves the object along the specified path, invoking a callback upon completion. </summary>
    /// <remarks>If the object is already moving, this method does nothing. Ensure the path is valid before calling this method.</remarks>
    public void MoveAlongPath(List<PathNode> _path, System.Action _onMovementComplete = null)
    {
        if (!IsMoving
            && _path != null
            && _path.Count != 0)
        {
            movementCoroutine = StartCoroutine(handleMovement(_path, _onMovementComplete));
        }
    }

    /// <summary> Moves the character along a specified path, updating its position and rotation at each step. </summary>
    private IEnumerator handleMovement(List<PathNode> _path, System.Action _onMovementComplete)
    {
        if (characterAnimator != null)
        {
            characterAnimator.SetFloat(SPEED_HASH, movementSpeed);
        }

        int _currentIndex = 0;
        Vector3 _targetPosition = _path[_currentIndex].Tile.WorldPosition;

        while (_currentIndex < _path.Count)
        {
            float _step = movementSpeed * Time.deltaTime;
            Vector3 _direction = (_targetPosition - transform.position);
            float _distance = _direction.magnitude;

            // If the character can reach or overshoot the target position in this step
            if (_distance <= _step)
            {
                transform.position = _targetPosition;
                _currentIndex++;

                // If there are more nodes in the path, set the next target position
                if (_currentIndex < _path.Count)
                {
                    _step -= _distance;
                    _targetPosition = _path[_currentIndex].Tile.WorldPosition;
                    _direction = _targetPosition - transform.position;

                    if (_direction != Vector3.zero)
                    {
                        moveCharacter(Mathf.Min(_step, _direction.magnitude), _direction);
                    }

                    continue;
                }
                else
                {
                    transform.position = _targetPosition;
                    break;
                }
            }
            else if (_direction != Vector3.zero)
            {
                moveCharacter(_step, _direction);
            }

            yield return null;
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetFloat(SPEED_HASH, 0.0f);
        }

        movementCoroutine = null;
        _onMovementComplete?.Invoke();
    }

    /// <summary> Moves the character a specified distance in the current direction and updates its rotation. </summary>
    private void moveCharacter(float _distance, Vector3 _direction)
    {
        transform.position += _direction.normalized * _distance;
        Quaternion _targetRotation = Quaternion.LookRotation(_direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary> Stops any ongoing movement and clears the movement coroutine reference. </summary>
    private void stopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }
}
