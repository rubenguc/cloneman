using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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

    private Vector2 originalColliderSize;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        m_MoveAction = InputSystem.actions.FindAction("Player/Move");
        m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
        m_ShootAction = InputSystem.actions.FindAction("Player/Shoot");

        originalColliderSize = boxCollider.size;
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
        boxCollider.size = originalColliderSize * 0.8f;

        Collider2D ladder = Physics2D.OverlapBox(
         boxCollider.bounds.center,
         boxCollider.bounds.size,
         0f,
         ladderLayer
     );

        if (ladder != null)
        {
            // Buscar el Tilemap asociado a la escalera
            Tilemap tilemap = ladder.GetComponent<Tilemap>();
            if (tilemap == null)
                tilemap = ladder.GetComponentInParent<Tilemap>();

            if (tilemap != null)
            {
                // Convertir la posición del jugador a celda del tilemap
                Vector3 playerWorldPos = transform.position;
                Vector3Int cellPos = tilemap.WorldToCell(playerWorldPos);

                // Obtener el centro de la celda
                Vector3 cellCenterWorld = tilemap.GetCellCenterWorld(cellPos);

                // Mover solo en X, mantener Y y Z
                transform.position = new Vector3(cellCenterWorld.x, playerWorldPos.y, playerWorldPos.z);
            }
        }

        anim.SetBool("climbing", true);
        _isClimbing = true;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
    }

    private void StopClimbing()
    {
        anim.SetBool("climbing", false);
        anim.SetBool("climbing_moving", false);
        boxCollider.size = originalColliderSize;
        _isClimbing = false;
        body.gravityScale = 1f;
    }

    private void HandleLadderMovement(float climbInput)
    {
        
        if (climbInput != 0)
        {
             anim.SetBool("climbing_moving", true);
            body.linearVelocity = new Vector2(0, climbInput * climbSpeed);
        }
        else
        {
             anim.SetBool("climbing_moving", false);
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
            new Vector2(0.2f, boxCollider.bounds.size.y),
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
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.6f, boxCollider.bounds.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxSize,
            0f,
            Vector2.down,
            0.1f,
            groundLayer
        );
        return hit.collider != null;
    }

    private void FlipSprite(float direction)
    {
        if (_isClimbing) return;

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

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;

        Gizmos.color = Color.red;
        Vector2 ladderBoxSize = new Vector2(0.2f, boxCollider.bounds.size.y);
        Vector3 ladderBoxCenter = boxCollider.bounds.center + Vector3.right * Mathf.Sign(transform.localScale.x) * ladderCheckDistance * 0.5f;
        Gizmos.DrawWireCube(ladderBoxCenter, ladderBoxSize);
    }


}