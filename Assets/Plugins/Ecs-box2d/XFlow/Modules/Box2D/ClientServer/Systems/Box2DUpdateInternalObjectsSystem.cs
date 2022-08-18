using UnityEngine;
using UnityEngine.Profiling;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DUpdateInternalObjectsSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        
        private EcsFilter filter;
        private EcsFilter bodiesFilter;
        
        private EcsPool<Box2DBodyComponent> poolBodyReference;
        private EcsPool<Box2DRigidbodyComponent> poolRigidbody;
        private EcsPool<PositionComponent> poolPosition;
        private EcsPool<Rotation2DComponent> poolRotation;
        private EcsPool<Box2DRigidbodyComponent> poolRigidBody;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            
            
            filter = world
                .Filter<Box2DBodyCreatedComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<Rotation2DComponent>()
                .End();

            bodiesFilter = world.Filter<Box2DRigidbodyComponent>().End();
            
            poolBodyReference = world.GetPool<Box2DBodyComponent>();
            poolRigidbody = world.GetPool<Box2DRigidbodyComponent>();
            poolPosition = world.GetPool<PositionComponent>();
            poolRotation = world.GetPool<Rotation2DComponent>();
            poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
        }
        
        public void Run(EcsSystems systems)
        {
            Profiler.BeginSample("UpdateInternalBox2D");
            
            
            foreach (var entity in bodiesFilter)
            {
                ref var rb = ref poolRigidBody.GetRef(entity);
                if (rb.LinearVelocity.sqrMagnitude < 0.0001f)
                    rb.LinearVelocity = new Vector2(0, 0);
                if (Mathf.Abs(rb.AngularVelocity) < 0.001f && Mathf.Abs(rb.AngularVelocity) > 0)
                    rb.AngularVelocity = 0;
                //var anglularVelocity = po
            }
            
            foreach (var entity in filter)
            {
                var bodyReference = poolBodyReference.Get(entity).BodyReference;
                var positionComponent = poolPosition.Get(entity);
                var rotationComponent = poolRotation.Get(entity);
                var rigidBodyComponent = poolRigidbody.Get(entity);

                
                Box2DApi.SetBodyInfo2(bodyReference,
                    new Vector2(positionComponent.value.x, positionComponent.value.z),
                    rotationComponent.Angle, rigidBodyComponent.LinearVelocity, rigidBodyComponent.AngularVelocity);
                Box2DApi.SetAwake(bodyReference, true);

                Box2DServices.Dump(world, entity);
                /*
                var bodyInfo = new BodyInfo
                {
                    LinearVelocity = rigidBodyComponent.LinearVelocity,
                    AngularVelocity = rigidBodyComponent.AngularVelocity,
                    Angle = rotationComponent.Angle,
                    Awake = true
                };
                bodyInfo.Position.x = positionComponent.value.x;
                bodyInfo.Position.y = positionComponent.value.z;
                Box2DApi.SetBodyInfo(bodyReference, bodyInfo);
                */
            }
            
            Profiler.EndSample();
        }

    }
}