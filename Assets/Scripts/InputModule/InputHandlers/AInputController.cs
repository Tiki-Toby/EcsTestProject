using Leopotam.EcsLite;
using UnityEngine;
using Utils;

namespace InputModule
{
    public abstract class AInputController : IEcsRunSystem, IEcsInitSystem
    {
        public enum ZoomType { None, ZoomOut, ZoomIn }

        protected float _zoomThreshold;

        //replace to component
        public ZoomType CurrentZoomType { get; private set; }
        public float ZoomDelta { get; private set; }
        public Vector3 FirstSingleTouchPosition { get; private set; }
        public Vector3 InputMoveDirection { get; private set; }
        
        public AInputController(float zoomThreshold)
        {
            _zoomThreshold = zoomThreshold;
        }
        
        protected abstract float GetZoomDelta();
        protected abstract Vector3 GetFirstSingleTouchPosition();
        protected abstract Vector3 GetInputMoveDirection();
        
        private ZoomType DefineAndGetZoomType(float zoomDelta)
        {
            if (zoomDelta > 0)
                return ZoomType.ZoomOut;
            else if (zoomDelta < 0)
                return ZoomType.ZoomIn;
            else
                return ZoomType.None;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            int inputEntity = world.NewEntity();

            //world.GetPool<InputDataComponent>().Add(inputEntity);
        }

        public void Run(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();

            //ref var inputData = ref world.GetUnique<InputDataComponent>();

            //inputData.ZoomDelta = GetZoomDelta();
            //inputData.CurrentZoomType = DefineAndGetZoomType(inputData.ZoomDelta);
            //inputData.FirstSingleTouchPosition = GetFirstSingleTouchPosition();
            //inputData.InputMoveDirection = GetInputMoveDirection();
        }
    }
}