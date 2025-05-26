using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 13f;
    public LayerMask groundLayer;
    public LayerMask ladderLayer;


    [Header("Physics Settings")]
    public float gravity = -17f;

    [Header("Ladder Settings")]
    public float climbSpeed = 4f;
    public float ladderCheckDistance = 0.1f;

    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private float lastMoveDirection = 1f;
    private bool _isClimbing = false;

    // Inputs
    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_ShootAction;

    private Animator anim;
    private float moveInputX;
    public bool isJumpingAttack { get; set; } = false;
    public bool isRunningAttack { get; set; } = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        m_MoveAction = InputSystem.actions.FindAction("Player/Move");
        m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
        m_ShootAction = InputSystem.actions.FindAction("Player/Shoot");
    }

    private void Update()
    {
        moveInputX = m_MoveAction.ReadValue<Vector2>().x;
        float moveDirection = Mathf.Sign(moveInputX);
        float verticalVelocity = body.linearVelocity.y;

        Vector2 moveValue = m_MoveAction.ReadValue<Vector2>();
        float climbInput = moveValue.y;

        bool isNearLadder = IsNearLadder();
        bool isOnGround = isGrounded();

        anim.SetBool("run", moveInputX != 0);
        anim.SetBool("jump_attack", isJumpingAttack);
        anim.SetBool("grounded", isOnGround);
        anim.SetBool("run_attack", isRunningAttack);

        if (isOnGround) isJumpingAttack = false;

        if (_isClimbing && (isOnGround || !isNearLadder)) StopClimbing();

        // Verificar si debe comenzar a escalar
        if (isNearLadder && Mathf.Abs(moveInputX) < 0.1f && climbInput > 0.1f) StartClimbing();


        if (_isClimbing)
        {
            HandleLadderMovement(climbInput);
        }
        else
        {
            HandleRegularMovement(moveInputX, verticalVelocity, moveDirection);
        }

        // Salto solo si está en el suelo
        if (m_JumpAction.WasPressedThisFrame() && isOnGround) Jump();
        if (m_JumpAction.WasReleasedThisFrame() && body.linearVelocity.y > 0) body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocityY / 2);

        FlipSprite(moveInputX);
    }

    private void Jump()
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
        isRunningAttack = false;
        isJumpingAttack = false;
    }

    private void StartClimbing()
    {
        _isClimbing = true;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
    }

    private void StopClimbing()
    {
        _isClimbing = false;
        body.gravityScale = 1f;
    }

    private void HandleLadderMovement(float climbInput)
    {
        if (climbInput != 0)
        {
            // Subir o bajar por la escalera
            body.linearVelocity = new Vector2(0, climbInput * climbSpeed);
        }
        else
        {
            // Mantener posición: no se mueve vertical u horizontalmente
            body.linearVelocity = new Vector2(0, 0);
        }
    }

    private void HandleRegularMovement(float moveInputX, float verticalVelocity, float moveDirection)
    {
        float appliedGravity = gravity;
        if (verticalVelocity < 0) appliedGravity *= 3f;

        if (!isGrounded()) verticalVelocity += appliedGravity * Time.deltaTime;

        float horizontalVelocity = moveInputX * moveSpeed;
        if (isTouchingWall(moveDirection) && !isGrounded()) horizontalVelocity = 0;

        body.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private bool IsNearLadder()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            new Vector2(0.1f, boxCollider.bounds.size.y),
            0f,
            Vector2.right * Mathf.Sign(transform.localScale.x),
            ladderCheckDistance,
            ladderLayer
        );
        return hit.collider != null;
    }

    private bool isTouchingWall(float _moveDirection)
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.right * _moveDirection,
            0.1f,
            groundLayer
        );
        return hit.collider != null;
    }

    private bool isGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            0.1f,
            groundLayer
        );
        return hit.collider != null;
    }

    private void FlipSprite(float direction)
    {
        if (direction != 0 && Mathf.Sign(direction) != Mathf.Sign(transform.localScale.x))
        {
            lastMoveDirection = direction;
            transform.localScale = new Vector3(
                transform.localScale.x * -1,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }


}