using UnityEngine;

namespace Controllers {
    public class CameraController : MonoBehaviour {
        private float yaw = 0.0f;
        private float pitch = 30.0f;

        public float NormalSpeed = 100;
        public GameObject Menu;


        void Start() {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Menu.SetActive(!Menu.activeSelf);
            }

            if (Input.GetKeyDown("space"))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
            }
        }

        void FixedUpdate()
        {
            //(WASD, EQ and Mouse)
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

            float speed = NormalSpeed;
            if (Input.GetKey(KeyCode.C))
            {
                speed /= 10;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= 5;
            }

            transform.Translate(new Vector3(moveHorizontal * speed * Time.deltaTime, moveUp * speed * Time.deltaTime,
                moveVertical * speed * Time.deltaTime));

            yaw += 2 * Input.GetAxis("Mouse X");
            pitch -= 2 * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

}
