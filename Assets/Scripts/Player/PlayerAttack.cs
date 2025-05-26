using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireballs;
    [SerializeField] private AudioClip fireballSound;

    private Animator anim;
    private PlayerController playerController;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame && cooldownTimer > attackCooldown && Time.timeScale > 0) Attack();
        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        // SoundManager. instance.PlaySound(fireballSound);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            anim.SetBool("jump_attack", true);
            playerController.isJumpingAttack = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            anim.SetBool("run_attack", true);
            playerController.isRunningAttack = true;
        }
        else
        {
            anim.SetTrigger("attack");
        }
        // return;
        cooldownTimer = 0;

        fireballs[FindFireball()].transform.position = firePoint.position;
        fireballs[FindFireball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }

    private  void endRunningAttack()
    {
        anim.SetBool("run_attack", false);
        playerController.isRunningAttack = false;
    }
    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}