using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float groundCheckDistance = 0.5f;

    [Header("Physics Settings")]
    public float gravity = -15f;




    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private float lastMoveDirection = 1f;
    // INputs
    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_ShootAction;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        m_MoveAction = InputSystem.actions.FindAction("Player/Move");
        m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
        m_ShootAction = InputSystem.actions.FindAction("Player/Shoot");
    }

    private void Update()
    {
        float moveInputX = m_MoveAction.ReadValue<Vector2>().x;
        float moveDirection = Mathf.Sign(moveInputX);
        float verticalVelocity = body.linearVelocity.y;



        float appliedGravity = gravity;
        if (verticalVelocity < 0)
            appliedGravity *= 2f;


        if (!isGrounded())
            verticalVelocity += appliedGravity * Time.deltaTime;

        float horizontalVelocity = moveInputX * moveSpeed;
        if (isTouchingWall(moveDirection) && !isGrounded())
            horizontalVelocity = 0;

        FlipSprite(moveInputX);


        body.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);

        if (m_JumpAction.WasPressedThisFrame() && isGrounded())
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);

    }

    private bool isTouchingWall(float _moveDirection)
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.right * _moveDirection, 0.1f, groundLayer);
        return hit.collider != null;
    }


    private bool isGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }


    private void FlipSprite(float direction)
    {
        print("a" + Mathf.Sign(direction) + "b" + Mathf.Sign(transform.localScale.x));
        if (direction != 0 && Mathf.Sign(direction) != Mathf.Sign(transform.localScale.x))
        {
            lastMoveDirection = direction;

            print("Flipping sprite" + lastMoveDirection);

            transform.localScale = new Vector3(
                transform.localScale.x * -1,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    // Debug Visual
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

}
