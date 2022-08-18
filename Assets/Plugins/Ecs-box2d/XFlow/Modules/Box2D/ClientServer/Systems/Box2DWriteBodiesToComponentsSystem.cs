using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DWriteBodiesToComponentsSystem :  IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        private EcsFilter filter;
        //private EcsFilter filterCheckRefs;
        
        private EcsPool<Box2DBodyComponent> poolBodyRef;
        private EcsPool<PositionComponent> poolPosition;
        private EcsPool<Box2DRigidbodyComponent> poolRigidBody;
        private EcsPool<Rotation2DComponent> poolRotation;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world
                .Filter<Box2DBodyComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<Rotation2DComponent>()
                .End();

            //filterCheckRefs = world.Filter<Box2DBodyComponent>().End();
            
            poolBodyRef = world.GetPool<Box2DBodyComponent>();
            poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
            poolPosition = world.GetPool<PositionComponent>();
            poolRotation = world.GetPool<Rotation2DComponent>();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                var bodyReference = poolBodyRef.Get(entity).BodyReference;
                var bodyInfo = Box2DApi.GetBodyInfo(bodyReference);
                
                ref var rigidBodyComponent = ref poolRigidBody.GetRef(entity);
                if (rigidBodyComponent.BodyType == BodyType.Dynamic)
                {
                    ref var positionComponent = ref poolPosition.GetRef(entity);
                    positionComponent.value.x = bodyInfo.Position.x;
                    positionComponent.value.z = bodyInfo.Position.y;
                    
                    rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
                    rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;
                    
                    poolRotation.GetRef(entity).Angle = bodyInfo.Angle;
                }
                
                Box2DServices.Dump(world, entity);
            }

            /*
            var worldRef = world.GetUnique<Box2DWorldComponent>().WorldReference;

            foreach (var entity in filterCheckRefs)
            {
                var reference = poolBodyRef.Get(entity).BodyReference;
                var b2world = Box2DApi.GetWorldFromBody(reference);
                if (b2world != worldRef)
                    throw new Exception($"{entity} from other Box2dWorld");
            }*/
        }

    }
}