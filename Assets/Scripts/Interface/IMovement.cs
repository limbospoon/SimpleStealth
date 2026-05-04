using UnityEngine;

public interface IMovement
{
    void Move();
    void Look();

    void Crouch();
    void UnCrouch();

    void Lean(float direction);

    void StopLean();
}
