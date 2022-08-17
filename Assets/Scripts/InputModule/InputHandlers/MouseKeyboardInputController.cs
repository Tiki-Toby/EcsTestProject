using UnityEngine;

namespace InputModule
{
    public class MouseKeyboardInputController : AInputController
    {
        public MouseKeyboardInputController(float zoomThreshold) : base(zoomThreshold)
        {
        }

        protected override float GetZoomDelta()
        {
            return Input.mouseScrollDelta.x;
        }

        protected override Vector3 GetFirstSingleTouchPosition()
        {
            return Input.mousePosition;
        }

        protected override Vector3 GetInputMoveDirection()
        {
            float moveZ = Input.GetAxis("Horizontal");
            float moveX = Input.GetAxis("Vertical");
            
            /*
            if (Input.GetKey(KeyCode.W))
                moveZ = 1;
            else if (Input.GetKey(KeyCode.S))
                moveZ = -1;
            else
                moveZ = 0;
            
            if (Input.GetKey(KeyCode.D))
                moveX = 1;
            else if (Input.GetKey(KeyCode.A))
                moveX = -1;
            else
                moveX = 0;
            */

            return new Vector3(moveX, 0, moveZ);
        }
    }
}