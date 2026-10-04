using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DeplacementPersonnageV2 : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float vitesseDeplacement = 6f;

    [Header("Saut")]
    [SerializeField] private float forceSaut = 12f;
    [SerializeField] private Transform verificationSol;         // Un objet vide (Empty) sous les pieds
    [SerializeField] private float rayonVerificationSol = 0.15f;
    [SerializeField] private LayerMask coucheSol;
    [Tooltip("Coupe l'ascension si on relâche la touche (saut 'à la Mario').")]
    [SerializeField] private bool sautVariable = true;
    [SerializeField] private float multiplicateurCoupSaut = 0.5f;

    [Header("Animation (facultatif)")]
    [SerializeField] private Animator animateur;                // Laisser vide si pas d'Animator
    [SerializeField] private VariableJoystick joystickVariable;

    private Rigidbody2D rb;
    private float entreeMouvement;
    private bool estAuSol;
    private bool sautDemande;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animateur) animateur = GetComponent<Animator>(); // Tente de l'attraper si présent
    }

    void Update()
    {
        // Récupération des entrées du joystick
        entreeMouvement = joystickVariable.Horizontal;          // -1, 0, 1

        // Retournement visuel (Flip) selon la direction du mouvement
        if (entreeMouvement != 0f)
        {
            var echelle = transform.localScale;
            echelle.x = Mathf.Sign(entreeMouvement) * Mathf.Abs(echelle.x == 0 ? 1 : echelle.x);
            transform.localScale = echelle;
        }

        // Mise à jour des paramètres de l'animation (facultatif)
        if (animateur)
        {
            animateur.SetFloat("Speed", Mathf.Abs(entreeMouvement));
            animateur.SetBool("isGrounded", estAuSol);
            animateur.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }


    /// Déclenche la demande de saut (appelé généralement par un bouton UI).
    public void DemandeSaut()
    {
        sautDemande = true;
    }


    /// Gère le saut variable : si on relâche la touche pendant la montée, on réduit l'ascension.
    public void ArreterSaut()
    {
        if (sautVariable && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * multiplicateurCoupSaut);
        }
    }

    void FixedUpdate()
    {
        // Vérification de la présence au sol via un cercle de collision
        estAuSol = Physics2D.OverlapCircle(verificationSol.position, rayonVerificationSol, coucheSol);

        // Application du déplacement horizontal par vélocité physique
        rb.linearVelocity = new Vector2(entreeMouvement * vitesseDeplacement, rb.linearVelocity.y);

        // Exécution du saut (uniquement si demandé et si le personnage est au sol)
        if (sautDemande && estAuSol)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forceSaut);
            if (animateur) animateur.SetTrigger("jump");
        }
        sautDemande = false;
    }

    // Dessine un gizmo dans l'éditeur pour visualiser la zone de vérification du sol
    void OnDrawGizmosSelected()
    {
        if (verificationSol == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(verificationSol.position, rayonVerificationSol);
    }
}