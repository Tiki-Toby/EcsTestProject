using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Utils;

namespace XFlow.Modules.Box2D.ClientServer
{
    public static class Box2DServices
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif


#if CLIENT
        public static void PrintClassOffsets<T>()
        {
            string str = String.Empty;
            str += "Offsets:\n";
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                str += $"{fieldInfo.Name} = {Marshal.OffsetOf<T>(fieldInfo.Name)}\n";
            }
            Debug.Log(str);

            // Debug.Log($"{Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("type")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("userData")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("bodyA")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("bodyB")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("collideConnected")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("localAnchorA")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("localAnchorB")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("length")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("minLength")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("maxLength")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("stiffness")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("damping")} ");
        }
#endif

        // Clone current physicsWorld, delete old, change all old body references to the new ones, return cloned world
        public static void ReplicateBox2D(EcsWorld src, EcsWorld dest, EcsSystems systems)
        {
            Profiler.BeginSample("Box2DServices.ReplicateBox2D");
            ReplicateBox2D0(src, dest);
            
            //т.к box2d мир реплицирован - тоесть создан новый,
            //то нужно переназначить contactsListener на свой инстанс внутри системы
            var contactsSystem = systems.GetSystem<Box2DCreateContactsSystem>();
            contactsSystem?.SetListener(dest);
            
            Profiler.EndSample();
        }


        public static void DestroyWorld(EcsWorld world)
        {
            var b2 = world.GetUnique<Box2DWorldComponent>().WorldReference;
             Box2DApi.DestroyWorld(b2);

            __ClearWorld(world);
        }

        public static void __ClearWorld(EcsWorld world)
        {
            world.DelUnique<Box2DWorldComponent>();
            
            var pool = world.GetPool<Box2DBodyComponent>();
            var poolBodyCreated = world.GetPool<Box2DBodyCreatedComponent>();
            
            pool.GetEntities(tempEntities);

            var count = tempEntities.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = tempEntities[i];
                pool.Del(entity);
                poolBodyCreated.Del(entity);
            }
        }

        //todo! multithreading?
        
        private static List<int> tempEntities = new List<int>(); 
        
        public static void ReplicateBox2D0(EcsWorld src, EcsWorld dest)
        {
            if (!src.HasUnique<Box2DWorldComponent>())
            {
                throw new Exception("cant replicate from src world");
            }
            if (dest.HasUnique<Box2DWorldComponent>())
            {
                throw new Exception("can't replicate into dest world");
            }
            
            src.GetPool<Box2DBodyComponent>().GetEntities(tempEntities);
            var srcPool = src.GetPool<Box2DBodyComponent>();

            var srcCount = tempEntities.Count;
            IntPtr[] arrayOfReferences = new IntPtr[srcCount];
            IntPtr[] copyOfReferences = new IntPtr[srcCount];
            
            for (int i = 0; i < srcCount; i++)
            {
                arrayOfReferences[i] = srcPool.Get(tempEntities[i]).BodyReference;
                copyOfReferences[i] = arrayOfReferences[i];
            }

            var srcWorld = src.GetUnique<Box2DWorldComponent>().WorldReference;
            
            Profiler.BeginSample("Box2DApi.CloneWorld");
            var newDestWorld = Box2DApi.CloneWorld(arrayOfReferences, srcCount, srcWorld);
            Profiler.EndSample();

            
            var destPool = dest.GetPool<Box2DBodyComponent>();
            var destPoolBodyCreated = dest.GetPool<Box2DBodyCreatedComponent>();
            
            
            dest.AddUnique<Box2DWorldComponent>().WorldReference = newDestWorld;

            for (int i = 0; i < srcCount; i++)
            {
                var entity = tempEntities[i];
                var reference = arrayOfReferences[i];
                destPool.GetOrCreateRef(entity).BodyReference = reference;
                destPoolBodyCreated.GetOrCreateRef(entity);
            }
        }

        public static void Dump(EcsWorld world, int entity)
        {
            /*
            if (entity != 3)
                return;
            var bodyReference = entity.EntityGet<Box2DBodyComponent>(world).BodyReference;
            world.Log($"dump entity {entity}");
            world.Log($"  bodyInfo.LinearVelocity {Box2DApi.GetBodyInfo(bodyReference).LinearVelocity}");
            world.Log($"  component.LinearVelocity {entity.EntityGet<Box2DRigidbodyComponent>(world).LinearVelocity}");
            world.Log($"  component.position {entity.EntityGet<PositionComponent>(world).value}");
            */
        }

        public static void DestroyBodyNow(EcsWorld world, int entity)
        {
            var pool = world.GetPool<Box2DBodyComponent>();
            if (pool.TryGet(entity, out Box2DBodyComponent component))
            {
                Box2DApi.DestroyBody2(component.BodyReference);
                pool.Del(entity);
            }

            entity.EntityDel<Box2DBodyCreatedComponent>(world);
            entity.EntityDel<Box2DRigidbodyDefinitionComponent>(world);
            entity.EntityDel<Box2DRigidbodyComponent>(world);
        }
        
        public static IntPtr CreateBodyNow(EcsWorld world, int entity, List<Vector2> shapeVertices = null)
        {
            
            if (entity.EntityHas<Box2DBodyComponent>(world))
            {
                throw new Exception("Box2DBodyComponent already exists");
            }


            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;
            var poolRigidbodyDefinition = world.GetPool<Box2DRigidbodyDefinitionComponent>();
            var poolBodyReference = world.GetPool<Box2DBodyComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<Rotation2DComponent>();
            var poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
            var poolBodyCreated = world.GetPool<Box2DBodyCreatedComponent>();
            
            var positionComponent = poolPositionComponent.Get(entity);
            var bodyAngle = poolRotationComponent.Get(entity).Angle;
            
            var def = poolRigidbodyDefinition.Get(entity);
            
            world.Log($"CreateBodyNow {entity} {def.Bullet}");
            
            var bodyReference = Box2DApi.CreateBody(
                physicsWorld,
                def.BodyType,
                new Vector2(positionComponent.value.x, positionComponent.value.z),
                bodyAngle,
                entity,
                def.IsFreezeRotation);
            
            Box2DApi.SetBullet(bodyReference, def.Bullet);
            Box2DApi.SetSleepingAllowed(bodyReference, def.SleepingAllowed);


            poolBodyCreated.Add(entity);
                
                
            var polygonCollider = entity.EntityGetNullable<Box2DPolygonColliderComponent>(world);
            if (polygonCollider.HasValue)
            {
                var vertices = polygonCollider.Value.Vertices;
                var anchors = polygonCollider.Value.Anchors;

                int index = 0;
                if (shapeVertices == null)
                    shapeVertices = new List<Vector2>();
                    
                foreach (var anchor in anchors)
                {
                    shapeVertices.Clear();
                    for (int offset = index; index <= anchor + offset; index++)
                        shapeVertices.Add(vertices[index]);
                    var shape = Box2DApi.CreatePolygonShape(shapeVertices.ToArray(), shapeVertices.Count);
                    AddFixtureToBody(bodyReference, shape, def);
                }
            }
            else
            {
                var shape = CreateSimpleShape(world, entity);
                AddFixtureToBody(bodyReference, shape, def);
            }
 
            Box2DApi.SetLinearDamping(bodyReference, def.LinearDamping);
            Box2DApi.SetAngularDamping(bodyReference, def.AngularDamping);
                
            poolBodyReference.Add(entity).BodyReference = bodyReference;
            poolRigidBody.GetOrCreateRef(entity).BodyType = def.BodyType;
            
            
            return bodyReference;
        }
        
        private static IntPtr CreateSimpleShape(EcsWorld world, int entity)
        {
            var boxCollider = entity.EntityGetNullable<Box2DBoxColliderComponent>(world);
            if (boxCollider.HasValue)
                return Box2DApi.CreateBoxShape(boxCollider.Value.Size / 2f);

            var circleCollider = entity.EntityGetNullable<Box2DCircleColliderComponent>(world);
            
            if (circleCollider.HasValue)
                return Box2DApi.CreateCircleShape(circleCollider.Value.Radius);
            
            var chainCollider = entity.EntityGetNullable<Box2DChainColliderComponent>(world);
            
            if (chainCollider.HasValue)
            {
                var vertices = chainCollider.Value.Points;
                return Box2DApi.CreateChainShape(vertices, vertices.Length);
            }

            return default;
        }
        
        private static void AddFixtureToBody(IntPtr bodyReference, IntPtr shape, Box2DRigidbodyDefinitionComponent box2DRigidbodyDefinitionComponent)
        {
            B2Filter filter;
            filter.CategoryBits = box2DRigidbodyDefinitionComponent.CategoryBits;
            filter.MaskBits = box2DRigidbodyDefinitionComponent.MaskBits;
            filter.GroupIndex = box2DRigidbodyDefinitionComponent.GroupIndex;
            
            Box2DApi.AddFixtureToBody(bodyReference, shape,
                box2DRigidbodyDefinitionComponent.Density,
                box2DRigidbodyDefinitionComponent.Friction,
                box2DRigidbodyDefinitionComponent.Restitution,
                box2DRigidbodyDefinitionComponent.RestitutionThreshold,
                box2DRigidbodyDefinitionComponent.IsTrigger,
                filter);
        }

        public static void WriteFromBodyToComponents(EcsWorld world, int entity)
        {
            /*
            ref var positionComponent = ref poolPosition.GetRef(entity);
            positionComponent.value.x = bodyInfo.Position.x;
            positionComponent.value.z = bodyInfo.Position.y;

            rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
            rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;

            poolRotation.GetRef(entity).Angle = bodyInfo.Angle;
            */
        }
        
        public static IntPtr GetBodyRefFromEntity(this EcsWorld world, int entity)
        {
            IntPtr bodyRef = IntPtr.Zero;

            var ecsPool = world.GetPool<Box2DBodyComponent>();

            if (ecsPool.Has(entity))
            {
                bodyRef = ecsPool.Get(entity).BodyReference;
            }
                        
            return bodyRef;
        }
        
        [DllImport(DllName)]
        public static extern int GetEntityFromBody(IntPtr body);

        [DllImport(DllName)]
        public static extern IntPtr GetWorldFromBody(IntPtr body);
    }
}