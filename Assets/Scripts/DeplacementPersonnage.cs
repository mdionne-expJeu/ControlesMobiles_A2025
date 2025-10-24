
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DeplacementPersonnage : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;         // un Empty sous les pieds
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Coupe l'ascension si on relâche la touche (jump 'à la Mario').")]
    [SerializeField] private bool variableJump = true;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Animation (facultatif)")]
    [SerializeField] private Animator animator;             // laisser vide si pas d'Animator
    [SerializeField] VariableJoystick variableJoystick;
   
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>(); // tente de l'attraper si présent
        
    }

    void Update()
    {
        // Ancien Input System
        moveInput = variableJoystick.Horizontal;          // -1, 0, 1

        // Flip visuel selon la direction
        if (moveInput != 0f)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x == 0 ? 1 : scale.x);
            transform.localScale = scale;
        }

        // Animation (facultatif)
        if (animator)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }

    public void DemandeSaut()
    {
        jumpRequested = true;
    }

    void FixedUpdate()
    {
        // Sol ?
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
       Debug.Log("Au Sol = " + isGrounded);
        // Déplacement horizontal par vélocité
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Saut (uniquement si au sol)
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator) animator.SetTrigger("jump");
        }
        jumpRequested = false;

        // Saut variable : si on relâche la touche de saut pendant la montée, on coupe l'ascension
        if (variableJump && rb.linearVelocity.y > 0f && Input.GetButtonUp("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    // Gizmo pour voir le groundCheck dans l'éditeur
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
