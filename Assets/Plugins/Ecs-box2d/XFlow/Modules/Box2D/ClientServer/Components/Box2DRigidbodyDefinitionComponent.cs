using System;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;

namespace XFlow.Modules.Box2D.ClientServer.Components
{
    [System.Serializable]
    public struct Box2DRigidbodyDefinitionComponent : IEcsAutoReset<Box2DRigidbodyDefinitionComponent>
    {
        public BodyType BodyType;
        public float Density;
        public float Friction;
        public float Restitution;
        public float RestitutionThreshold;
        public float LinearDamping;
        public float AngularDamping;
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
        public bool IsTrigger;
        public bool IsFreezeRotation;
		public bool Bullet;
        public bool SleepingAllowed;
        
        public void AutoReset(ref Box2DRigidbodyDefinitionComponent c)
        {
            c.BodyType = BodyType.Dynamic;
            c.Density = 1f;
            c.RestitutionThreshold = 0.5f;
            c.LinearDamping = 1.0f;
            c.AngularDamping = 1.0f;
            c.Friction = 0f;
            c.Restitution = 0f;
            c.CategoryBits = 0x0001;
            c.MaskBits = 0x0001;
            c.GroupIndex = 0;
            c.IsTrigger = false;
            c.IsFreezeRotation = false;
			c.Bullet = false;
            c.SleepingAllowed = true;
        }
    }
}
