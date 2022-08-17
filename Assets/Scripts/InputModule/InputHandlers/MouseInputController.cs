using GameEntities;
using Leopotam.EcsLite;
using UnityEngine;
using Utils;

namespace InputModule
{
    public class MouseInputController : ABaseInputController
    {
        private Camera camera;
        private RaycastHit hit;

        public override void Init(IEcsSystems systems)
        {   
            base.Init(systems);
            var cameraPool = world.GetPool<CameraComponent>();

            camera = cameraPool.GetRawDenseItems()[0].Camera;
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