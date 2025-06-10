using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float moveSmoothness = 0.1f;

        [Space(15)]
        [Header("References")]
        public Transform character;
        public Inventory inventory;
        public InputReader inputReader;


        private Rigidbody2D rb;

        private Vector2 targetVelocity = Vector2.zero;
        private Vector2 currentVelocity = Vector2.zero;

        private Vector3 leftFacing = Vector3.one;
        private Vector3 rightFacing = new Vector3(-1f, 1f, 1f);
        private bool controllerEnabled = true;


        /**/


        #region Setup

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }


        public void EnableController(bool _enable)
        {
            if (!_enable) targetVelocity = Vector3.zero;
            controllerEnabled = _enable;
        }

        #endregion


        #region Controller

        private void Update()
        {
            if (controllerEnabled)
            {
                targetVelocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

                if (targetVelocity.x < 0) character.localScale = leftFacing;
                else if (targetVelocity.x > 0) character.localScale = rightFacing;
            }
        }


        private void FixedUpdate()
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, moveSmoothness);
            rb.linearVelocity = currentVelocity;
        }

        #endregion
    }
}