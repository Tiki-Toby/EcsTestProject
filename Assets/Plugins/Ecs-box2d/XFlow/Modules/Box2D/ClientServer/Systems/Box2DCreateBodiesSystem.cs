using System.Collections.Generic;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Joints;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DCreateBodiesSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        private List<Vector2> shapeVertices = new List<Vector2>();
        
        private EcsFilter filterNewBodies;
        private EcsFilter filterJoints;
        private EcsPool<JointTestComponent> poolJoint;
        private EcsPool<Box2DJointCreatedComponent> poolJointCreated;
        private EcsPool<Box2DBodyComponent> poolBodyReference;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            
            filterNewBodies = world
                .Filter<Box2DRigidbodyDefinitionComponent>()
                .Inc<Rotation2DComponent>()
                .Exc<Box2DBodyCreatedComponent>()
                .End();
            
            filterJoints = world.Filter<JointTestComponent>().Exc<Box2DJointCreatedComponent>().End();
            
            poolJoint = world.GetPool<JointTestComponent>();
            poolJointCreated = world.GetPool<Box2DJointCreatedComponent>();
            poolBodyReference = world.GetPool<Box2DBodyComponent>();
        }

        public void Run(EcsSystems systems)
        {
            if (!world.HasUnique<Box2DWorldComponent>())
                return;

            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;


            /*
            var poolRigidbodyDefinition = world.GetPool<Box2DRigidbodyDefinitionComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<Rotation2DComponent>();
            var poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
            var poolBodyCreated = world.GetPool<Box2DBodyCreatedComponent>();
            */

            foreach (var entity in filterNewBodies)
            {
                //todo, optimize reuse Pools
                Box2DServices.CreateBodyNow(world, entity, shapeVertices);
            }
            
            foreach (var entity in filterJoints)
            {
                var joint = poolJoint.Get(entity);
                Box2DApi.b2DistanceJointDef def = Box2DApi.b2DistanceJointDef.Null;
                def.baseClass.bodyA = poolBodyReference.Get(entity).BodyReference;
                def.baseClass.bodyB = poolBodyReference.Get(joint.Entity).BodyReference;
                def.baseClass.collideConnected = false;
                def.minLength = 3;
                def.maxLength = 3;
                
                Box2DApi.CreateJoint(physicsWorld, def);
                poolJointCreated.Add(entity);
            }
        }
    }
}