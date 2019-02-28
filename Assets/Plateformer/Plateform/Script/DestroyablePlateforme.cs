using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class DestroyablePlateforme : MonoBehaviour
{
    [SerializeField]
    private bool respawn = false;
    [SerializeField]
    private float speedAnimation = 1f;
    private Animator animator;


    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = speedAnimation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (respawn)
            animator.Play("fadeRespawn",-1,0);
        else
            animator.Play("fade",-1,0);
    }
}
