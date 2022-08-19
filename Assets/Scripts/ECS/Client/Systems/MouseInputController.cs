using UnityEngine;
using XFlow.EcsLite;

namespace ECS.Client
{
    public class MouseInputController : ABaseInputController
    {
        private Camera camera;
        private RaycastHit hit;

        public override void Init(EcsSystems systems)
        {   
            base.Init(systems);
            
            var cameraEntity = world.GetUnique<MainCameraComponent>().cameraEntity;
            camera = world.GetPool<CameraComponent>().GetRef(cameraEntity).Camera;
            
            if (camera == null)
            {
                Debug.LogError("Camera didn't initialize in component pool");
                camera = Camera.main;
                world.AddUnique<CameraComponent>().Camera = camera;
            }
        }
        
        protected override bool IsTouchedSomeTargetPoint()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag.Equals("WalkArea"))
                    {
                        return true;
                    }
                }
                
                return false;
            }

            return false;
        }

        protected override Vector3 GetTouchedPoint()
        {
            return hit.point;
        }

        protected override Vector3 GetInputMoveDirection()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            return new Vector3(moveX, 0, moveZ);
        }
    }
}