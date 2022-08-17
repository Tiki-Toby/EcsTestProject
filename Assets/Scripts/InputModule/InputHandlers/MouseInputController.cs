using UnityEngine;

namespace InputModule
{
    public class MouseInputController : ABaseInputController
    {
        protected override bool IsTouchedSomeTargetPoint()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // create ray cast 
                return true;
            }

            return false;
        }

        protected override Vector3 GetTouchedPoint()
        {
            return Input.mousePosition;
        }

        protected override Vector3 GetInputMoveDirection()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            return new Vector3(moveX, 0, moveZ);
        }
    }
}