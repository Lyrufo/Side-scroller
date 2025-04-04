using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]

// force � mettre ces composants

public class PlayerCharacter2D : MonoBehaviour
{

    [Tooltip("la 'difficult�' pour faire avancer le perso (sa vitesse d'acc�l�ration)")]
    public float movementAcceleration = 20f;

    [Tooltip("vitesse max/s sur X")]
    public float maxMovementSpeed = 4f;

    [Tooltip("la 'difficult�' pour faire sauter le perso (force appliqu�e)")]
    public float jumpForce = 16f;

    [Tooltip("le layer sur lequel �a agit pour �viter que le chara capte son propre collider ")]
    public LayerMask groundLayer;

    [Tooltip("la taille du 'rayon' raycast si on veut sauter � nouveau sans toucher le sol")]
    public float groundDistance = 1.1f;

    private Rigidbody2D _rigidbody = null; //pour bouger

    private Animator _animator = null; //pour mettre les anim

    private SpriteRenderer _spriteRenderer = null; // pour montrer le chara sur l'�cran
 

    // = null c'est pour etre sur que en gros �a fait ref � rien d�s le d�part


    private void Awake()
    {
        //Void awake pour �tre lanc� avant un quelconque start => ici pour choper les coponents direct et forcer le fait qu'ils soient sur l'objet o� le script est mis
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

    }


    private void Update()
    //� chaque frame on va appeler les 4 fonctions en dessous l�
    {
        UpdateMovement(); //met � jour le sens de l'anim 
        UpdateJump(); //v�rifie si on appuie espace pour sauter
        ClampVelocity(); //capte la vitesse pour limiter la vitesse max
        UpdateGround(); //v�rifie si le joueur est bien au sol

        //ces lignes c'est d�di� � l'aimation et � laquelle jouer selon la vitesse et l'�tat du perso

        // Quand la vitesse en X est n�gative, on inverse l'animation pour regarder � gauche

        if (_rigidbody.velocity.x > 0) //qd va vers la droite
        {
            _spriteRenderer.flipX = false; //ne flip pas
        }
        else if (_rigidbody.velocity.x < 0) //qd va vers la gauche
        {
            _spriteRenderer.flipX = true; // flip
        }

        //mais tout �a dans le cas o� � la base il �tait vers la droite, puisqu'on se base sur la v�locit�


        // Utilise la vitesse du personnage pour choisir l'animation ("idle", "move", etc.)
        _animator.SetFloat("xVelocity", Mathf.Abs(_rigidbody.velocity.x));


        // Si la vitesse en Y est positive, on dit que le personnage saute, sinon il tombe.
        _animator.SetBool("isJumping", _rigidbody.velocity.y > 0);
        _animator.SetBool("isFalling", _rigidbody.velocity.y < 0);
    }


    private void UpdateMovement()
    {
        float xMovement = 0f; //initiatlise le mvt du perso � 0 sur x

        if (Input.GetKey(KeyCode.RightArrow))
        {
            xMovement += 1; //si fleche droite press�e -> +1 sur l'axe X donc avance vers droite
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            xMovement += -1; //si fleche gauche press�e -> -1 sur l'axe X donc avance vers gauche
        }

        if (xMovement == 0) //si perso bouge pas
        {
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y); // on met son mvt sur x � 0 mais on laisse son �tat sur y comme il est (donc s'il saute ou tombe)
        }
        else //se joue ds tous les cas d�s qu'il y a un mvt (lorsque xmovement =/ 0) donc ds tous si une key est press�e
        {
            _rigidbody.velocity += Vector2.right * xMovement * movementAcceleration * Time.deltaTime;
            //ok donc ici, il s'agit d'appliquer la vitesse selon la physique (la direction et le mvt a d�j� �t� d�fini plus haut)
            //velocity c'est un composant de rigidbody qui repr�sente la vitesse (sur x et y) et donc ici on va y ajouter un mvt 
            //vector2 c'est un vecteur 2d auquel on peut donner un x et y entre -1 et 1 (x:y) mais en ajoutant .right on lock le y � 0 et le x � 1
            //xmovement peut stocker 1 ou -1 donc c'est pour inverser ou non le r�sultat en n�gatif si vers la gauche car on a bloqu� x en postitif en ajouter right � vector2
            //movement acceleration c'est la variable qu'on a d�termin� tout en haut (donc on peut direct �crire 20f en th�orie dans la fonction) et ici c'est la vitessse d'acc�l�ration donc
            //Time.deltaTime notre meilleur pote en gros c'est le temps entre 2 frames (ici aussi en th�orie on pourrait une valeur � la place, sauf qu'en mettant la fonction instead, �a s'adapt au nb de fps)

        }

    }


    private bool isGrounded = false; //bool�en pour savoir si je touche le sol ou pas


    private void UpdateGround()
    {
        
        RaycastHit2D hit = Physics2D.CircleCast (transform.position, 0.2f,  Vector2.down, groundDistance, groundLayer);
        //Raycast envoie un rayon pour capter si y'a un collider en partant du bas de l'objet auquel on a assign� le script

        if (hit.collider != null) //si pas null alors a touch� qqc, donc est au sol, donc je peux sauter
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    private void OnDrawGizmos()
    {
        // "update" uniquement vu en mode editor
        // Ici pour voir la taille du Raycast
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundDistance);
        // en gros : tracer une ligne du centre l'objet (transform.position c'est les coordoon�es bas�es sur le centre)  vers la position actuelle + la direction d'un vecteur (ici 3 car a du mal avec 2d) donc vers le bas de la taille grounddistance
    }

    private void UpdateJump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)  //DOWN n�cessaire contrairement au d�pacement car sinon se joue en boucle et is grounded pour �tre s�r d'�tre au sol
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);


            //donc qd espace press� : on prend l'objet, on lui applique une force (pas un mvt parce que c'est juste une impulsion sur y) 
            //vector2.up expliqu� plus haut * la variable force d�finie en haut 
            //forcemode2d.impulse appplique direct en mode impulsion
            isGrounded = false; 
            //on remet � l'�tat faux jusqu'� ce que �a capte le sol et revien en true
            }
            
      
        }
    
    




    private void ClampVelocity() //pour v�rifier qu'on d�passe pas la vitesse max sur x dont on a d�finit la variable tt en haut 
    {
        Vector2 velocity = _rigidbody.velocity; //on chope la valeur de la v�locit� (que le x) actuelle 

        velocity.x = Mathf.Clamp(velocity.x, -maxMovementSpeed, maxMovementSpeed);
        //peu importe cette valeur elle doit �tre �gale � ... = entre les deux valeurs (le max ou son inverse si on va vers la gauche) et si c'est le cas on lui assigne le max

        _rigidbody.velocity = velocity;
        //et pouf on lui applique cette valeur 
    }


}



