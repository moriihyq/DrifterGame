using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // public Animator anim;
    private Animator anim;
    private Rigidbody2D rb;
    private bool isMoving;
    private bool approach;
    public float distanceToPlayer;
    private float life;
    private bool isDead;
    private float originScaleX;
    //private bool direction = true; // true = right, false = left
    private float lastSpeed;
    void Start()
    {
        isMoving = false;
        approach = false;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lastSpeed = 0;
        life = 100f; // Example initial life value
        isDead = false; // Example initial dead state
        originScaleX = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject enemy = this.gameObject;
        GameObject player = GameObject.Find("Player");
        //rb.linearVelocityX = 5;
        if (player != null)
        {
            distanceToPlayer = player.transform.position.x - enemy.transform.position.x;
        }
        else
        {
            distanceToPlayer = 0;
            Debug.Log("Player not found");
        }

        if ((distanceToPlayer < 5 && distanceToPlayer > 1) || (distanceToPlayer > -5 && distanceToPlayer < -1))
        {
            if (enemy.transform.position.x > player.transform.position.x)
            {
                rb.linearVelocityX = -5;
            }
            else
            {
                rb.linearVelocityX = 5;
            }
        }

        AnimatorController();
    }
    private void AnimatorController()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x = distanceToPlayer > 0 ? originScaleX : -originScaleX;
        transform.localScale = currentScale;
        lastSpeed = rb.linearVelocityX;
        isMoving = rb.linearVelocityX != 0;
        anim.SetBool("isMoving", isMoving);
        approach = distanceToPlayer < 1 && distanceToPlayer > -1;
        anim.SetBool("approach", approach);
    }
}
