using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : AbstractEntity
{
    [SerializeField] 
    private CameraFollow _cameraFollow;

    [SerializeField] 
    private AudioSource _footstepSoundEmitter;

    public float MoveSpeed = 5;
    private Vector3 _moveVector = Vector3.zero;
    private Quaternion _addedRotation;

    public float SlowdownSpeed = 6;
    public float MaxHorizontalMoveSpeed = 10;
    public float MaxVerticalMoveSpeed = 30;
    public bool IsGrounded { get; private set; } = false;

    [HideInInspector]
    public bool UserMoveEnabled;

    private float _footstepTimer;
    private const float _footstepTimerMax = 0.6f;


    protected override void Start() {
        base.Start();

        _addedRotation = MainRB.rotation;

        _footstepSoundEmitter.volume = Game.VolumeScale; // Probably going to be changed later
        _footstepTimer = _footstepTimerMax;

        GameManager.Player = this; // Probably going to be changed later
    }

    private void Update() {
        Move();
    }

    private void FixedUpdate() {
        CheckGrounded();
        UpdateGravity();
        MoveUpdate();
    }


    protected override IEnumerator StartAlive() {
        UserMoveEnabled = true;

        yield return null;
    }


    private void Move() {
        if (!UserMoveEnabled)
            return;


        Vector2 v = Game.Input.Player.Move.ReadValue<Vector2>();

        if (v.x != 0 || v.y != 0) {
            if(_footstepTimer >= _footstepTimerMax) {
                //_footstepSoundEmitter.PlayOneShot(_footstepSoundEmitter.clip);
                _footstepTimer = 0;
            }

            _footstepTimer += _deltaTime;

            // Get the forward and right directions based on the current camera view
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            Vector3 camRight = new Vector3(-camForward.z, 0, camForward.x);

            // Construct the direction the player is moving in
            _moveVector = (camForward * v.y + camRight * -v.x).normalized;

            // Rotate the player model to face the new direction with a really fast interpolation
            Quaternion newDir = Quaternion.LookRotation(_moveVector, -Vector3.Cross(camForward, camRight));
            _addedRotation = Quaternion.Lerp(_addedRotation, newDir, Time.deltaTime * 20);
        }
    }

    private void MoveUpdate() {
        // Apply rotation
        MainModel.rotation = _addedRotation;


        // Apply inertia force if not moving
        bool sprinting = Game.Input.Player.Sprint.ReadValue<float>() == 1;

        Vector3 flatCurVelocity = MainRB.velocity;
        flatCurVelocity.y = 0;

        if (_moveVector.x == 0 && _moveVector.z == 0) {
            Vector3 oppositeFlatNorm = (-flatCurVelocity).normalized;
            float sprintBonus = sprinting || oppositeFlatNorm.magnitude > MaxHorizontalMoveSpeed ? 2 : 1;
            MainRB.AddForce(oppositeFlatNorm * SlowdownSpeed * sprintBonus);

            Vector3 newFlat = MainRB.velocity;
            newFlat.y = 0;
            if(newFlat.normalized == oppositeFlatNorm || newFlat.magnitude < 1f) {
                MainRB.velocity = Vector3.Lerp(MainRB.velocity, new Vector3(0, MainRB.velocity.y, 0), Time.deltaTime * 20);
            }
        }
        

        // Apply movement speed
        _moveVector *= MoveSpeed;

        // Create the vector the player moves at; halves the speed of the player's movement when focusing
        _moveVector *= 2.5f * (sprinting ? 3 : 2);
        float usedMaxCap = sprinting ? MaxHorizontalMoveSpeed * 1.75f : MaxHorizontalMoveSpeed;

        // Reduce speed in the air
        if (!IsGrounded) {
            _moveVector *= 0.3f; //subbject to change
        }

        // Add force in the x and z directions
        Vector3 flatMoveVec = new Vector3(_moveVector.x, 0, _moveVector.z);

        MainRB.AddForce(flatMoveVec, ForceMode.Acceleration);

        float mag = MainRB.velocity.magnitude;
        if (mag > usedMaxCap) {
            // Reusing magnitude to speed up normalization
            MainRB.velocity = MainRB.velocity / mag * usedMaxCap;
        }

        // Reset _moveVector
        _moveVector = Vector3.zero;
    }

    private void UpdateGravity() {
        // Note: I'm pretty sure this will have problems when going too fast upwards instead of downwards. It needs reennovation
        if (!IsGrounded && Mathf.Abs(MainRB.velocity.y) < MaxVerticalMoveSpeed) {
            MainRB.AddForce(new Vector3(0, Game.Manager.PlayerGravity, 0), ForceMode.Acceleration);
        }
    }

    private void CheckGrounded() {
        Vector3 rayOrigin = MainCollider.bounds.center;

        Vector3 gravityDir = new Vector3(0, Game.Manager.PlayerGravity, 0).normalized;

        int mask = LayerMask.GetMask("Default");

        bool hit = Physics.SphereCast(rayOrigin, MainCollider.bounds.extents.x, gravityDir, out _, MainCollider.bounds.extents.y, mask);
        if(hit != IsGrounded) { IsGrounded = hit; }
    }
}
