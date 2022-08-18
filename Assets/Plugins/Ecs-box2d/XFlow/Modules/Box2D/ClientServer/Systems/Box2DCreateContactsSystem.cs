using System;
using System.Runtime.InteropServices;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Utils;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DCreateContactsSystem : IEcsInitSystem, IEcsDestroySystem
    {
        private Box2DApi.CollisionCallback _cbkBeginContactDelegate;
        private Box2DApi.CollisionCallback _cbkEndContactDelegate;
        private Box2DApi.CollisionCallback _cbkPostSolveDelegate;
        private Box2DApi.CollisionCallback _cbkPreSolveDelegate;

        private EcsWorld world;
        private GCHandle gc;
        private IntPtr contactsListenerInstance;

        public Box2DCreateContactsSystem()
        {
            _cbkBeginContactDelegate = BeginContactCallback;
            _cbkEndContactDelegate = EndContactCallback;
            _cbkPostSolveDelegate = PreSolveCallback;
            _cbkPreSolveDelegate = PostSolveCallback;
            
            
            contactsListenerInstance = Box2DApi.CreateContactListener();
            gc = GCHandle.Alloc(this);
           
            Box2DApi.SetContactCallbacks2( contactsListenerInstance, GCHandle.ToIntPtr(gc), 
                _cbkBeginContactDelegate, _cbkEndContactDelegate,
                _cbkPreSolveDelegate, _cbkPostSolveDelegate);
        }
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            SetListener(world);
        }

        public void SetListener(EcsWorld world_)
        {
            if (!world_.HasUnique<Box2DWorldComponent>())
                return;//init will be later
            
            var b2World = world_.GetUnique<Box2DWorldComponent>().WorldReference;
            Box2DApi.SetContactListener(b2World, contactsListenerInstance);
        }
        
        public void Destroy(EcsSystems systems)
        {
            Box2DApi.DestroyContactListener(contactsListenerInstance);
            gc.Free();
        }

#if ENABLE_IL2CPP        
        [AOT.MonoPInvokeCallback(typeof(Box2DApi.CollisionCallback))]
#endif
        private static void BeginContactCallback(CollisionCallbackData callbackData)
        {
            var instance = GCHandle.FromIntPtr(callbackData.UserPtr).Target as Box2DCreateContactsSystem;
            var world = instance.world;
            var newEntity = world.NewEntity();
            newEntity.EntityAddComponent<Box2DBeginContactComponent>(world).Data.CopyFrom(world, callbackData);
        }

#if ENABLE_IL2CPP
        [AOT.MonoPInvokeCallback(typeof(Box2DApi.CollisionCallback))]
#endif
        private static void EndContactCallback(CollisionCallbackData callbackData)
        {
            var instance = GCHandle.FromIntPtr(callbackData.UserPtr).Target as Box2DCreateContactsSystem;
            var world = instance.world;
            var newEntity = world.NewEntity();
            newEntity.EntityAddComponent<Box2DEndContactComponent>(world).Data.CopyFrom(world, callbackData);
        }

#if ENABLE_IL2CPP
        [AOT.MonoPInvokeCallback(typeof(Box2DApi.CollisionCallback))]
#endif
        private static void PreSolveCallback(CollisionCallbackData callbackData)
        {
            var instance = GCHandle.FromIntPtr(callbackData.UserPtr).Target as Box2DCreateContactsSystem;
            var world = instance.world;
            var newEntity = world.NewEntity();
            newEntity.EntityAddComponent<Box2DPreSolveComponent>(world).Data.CopyFrom(world, callbackData);
        }

#if ENABLE_IL2CPP
        [AOT.MonoPInvokeCallback(typeof(Box2DApi.CollisionCallback))]
#endif
        private static void PostSolveCallback(CollisionCallbackData callbackData)
        {
            var instance = GCHandle.FromIntPtr(callbackData.UserPtr).Target as Box2DCreateContactsSystem;
            var world = instance.world;
            var newEntity = world.NewEntity();
            newEntity.EntityAddComponent<Box2DPostSolveComponent>(world).Data.CopyFrom(world, callbackData);
        }

    }
}