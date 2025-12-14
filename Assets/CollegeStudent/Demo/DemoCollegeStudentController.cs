using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClearSky
{
    public class DemoCollegeStudentController : MonoBehaviour
    {
        public float movePower = 10f;
        public float KickBoardMovePower = 15f;
        public float jumpPower = 20f; //Set Gravity Scale in Rigidbody2D Component to 5

        // 冰面滑行效果参数
        [Header("Ice Sliding Settings")]
        public float acceleration = 15f; // 加速度
        public float deceleration = 3f; // 减速度（摩擦力），值越小滑行越远
        public float maxSpeed = 12f; // 最大速度
        [Range(0.5f, 5f)]
        public float slideFriction = 1.5f; // 滑行摩擦力，值越小滑行越远

        [Header("Death & Respawn Settings")]
        [SerializeField] private bool useBoundaryDeath = true; // 是否启用边界死亡
        [SerializeField] private float deathBoundaryMinX = -120f; // 死亡边界X最小值
        [SerializeField] private float deathBoundaryMaxX = 110f;  // 死亡边界X最大值
        [SerializeField] private float deathBoundaryMinY = -30f;  // 死亡边界Y最小值（掉落死亡）
        [SerializeField] private float deathBoundaryMaxY = 50f;   // 死亡边界Y最大值
        [SerializeField] private string enemyName = "Cop Variant"; // 敌人名称

        private Rigidbody2D rb;
        private Animator anim;
        Vector3 movement;
        private int direction = 1;
        bool isJumping = false;
        private bool alive = true;
        private bool isKickboard = false;
        
        // 当前速度（用于滑行效果）
        private Vector2 currentVelocity = Vector2.zero;
        
        // 起始位置（用于重生）
        private Vector3 spawnPosition;


        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            // 记录起始位置
            spawnPosition = transform.position;
        }

        private void Update()
        {
            Restart();
            if (alive)
            {
                Hurt();
                Die();
                Attack();
                Jump();
                KickBoard();
                Run();
                CheckBoundaryDeath();
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            anim.SetBool("isJump", false);
        }
        void KickBoard()
        {
            if (Input.GetKeyDown(KeyCode.Alpha4) && isKickboard)
            {
                isKickboard = false;
                anim.SetBool("isKickBoard", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) && !isKickboard )
            {
                isKickboard = true;
                anim.SetBool("isKickBoard", true);
            }

        }

        void Run()
        {
            if (!isKickboard)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                Vector2 targetVelocity = Vector2.zero;
                anim.SetBool("isRun", false);

                // 根据输入计算目标速度
                if (inputX < 0)
                {
                    direction = -1;
                    targetVelocity = Vector2.left * maxSpeed;
                    transform.localScale = new Vector3(direction, 1, 1);
                    if (!anim.GetBool("isJump"))
                        anim.SetBool("isRun", true);
                }
                else if (inputX > 0)
                {
                    direction = 1;
                    targetVelocity = Vector2.right * maxSpeed;
                    transform.localScale = new Vector3(direction, 1, 1);
                    if (!anim.GetBool("isJump"))
                        anim.SetBool("isRun", true);
                }

                // 冰面滑行效果：平滑过渡到目标速度
                if (inputX != 0)
                {
                    // 有输入时，加速到目标速度
                    currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
                }
                else
                {
                    // 无输入时，应用摩擦力逐渐减速（基于时间的衰减）
                    // 使用MoveTowards确保平滑减速到零
                    currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, slideFriction * Time.deltaTime);
                }
                
                // 滑行时保持跑步动画
                if (currentVelocity.magnitude > 0.5f && !anim.GetBool("isJump"))
                {
                    anim.SetBool("isRun", true);
                }

                // 应用速度到位置
                transform.position += new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;
            }
            else if (isKickboard)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                Vector2 targetVelocity = Vector2.zero;
                
                if (inputX < 0)
                {
                    direction = -1;
                    targetVelocity = Vector2.left * KickBoardMovePower;
                    transform.localScale = new Vector3(direction, 1, 1);
                }
                else if (inputX > 0)
                {
                    direction = 1;
                    targetVelocity = Vector2.right * KickBoardMovePower;
                    transform.localScale = new Vector3(direction, 1, 1);
                }

                // 滑板模式也使用滑行效果，摩擦力更小滑得更远
                if (inputX != 0)
                {
                    currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * 0.8f * Time.deltaTime);
                }
                else
                {
                    // 滑板模式摩擦力更小，滑行距离更远
                    currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, slideFriction * 0.5f * Time.deltaTime);
                }

                transform.position += new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;
            }
        }
        void Jump()
        {
            if ((Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0)
            && !anim.GetBool("isJump"))
            {
                isJumping = true;
                anim.SetBool("isJump", true);
            }
            if (!isJumping)
            {
                return;
            }

            rb.velocity = Vector2.zero;

            Vector2 jumpVelocity = new Vector2(0, jumpPower);
            rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

            isJumping = false;
        }
        void Attack()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                anim.SetTrigger("attack");
            }
        }
        void Hurt()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                anim.SetTrigger("hurt");
                if (direction == 1)
                    rb.AddForce(new Vector2(-5f, 1f), ForceMode2D.Impulse);
                else
                    rb.AddForce(new Vector2(5f, 1f), ForceMode2D.Impulse);
            }
        }
        void Die()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TriggerDeath();
            }
        }

        /// <summary>
        /// 检查角色是否超出边界，超出则死亡
        /// </summary>
        void CheckBoundaryDeath()
        {
            if (!useBoundaryDeath) return;
            
            Vector3 pos = transform.position;
            if (pos.x < deathBoundaryMinX || pos.x > deathBoundaryMaxX ||
                pos.y < deathBoundaryMinY || pos.y > deathBoundaryMaxY)
            {
                TriggerDeath();
            }
        }

        /// <summary>
        /// 碰撞检测 - 与敌人碰撞时死亡
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 使用 StartsWith 检测所有以 enemyName 开头的敌人（如 Cop Variant、Cop Variant(1) 等）
            if (collision.gameObject.name.StartsWith(enemyName) && alive)
            {
                TriggerDeath();
            }
        }

        /// <summary>
        /// 触发死亡并重生
        /// </summary>
        void TriggerDeath()
        {
            if (!alive) return;
            
            alive = false;
            isKickboard = false;
            anim.SetBool("isKickBoard", false);
            
            // 直接重生到起始位置，不播放死亡动画
            Respawn();
        }

        /// <summary>
        /// 重生到起始位置
        /// </summary>
        void Respawn()
        {
            // 传送到起始位置
            transform.position = spawnPosition;
            
            // 重置速度
            currentVelocity = Vector2.zero;
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            
            // 恢复状态
            alive = true;
            isKickboard = false;
            anim.SetBool("isKickBoard", false);
            anim.SetTrigger("idle");
        }
        void Restart()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                isKickboard = false;
                anim.SetBool("isKickBoard", false);
                anim.SetTrigger("idle");
                alive = true;
            }
        }
    }

}