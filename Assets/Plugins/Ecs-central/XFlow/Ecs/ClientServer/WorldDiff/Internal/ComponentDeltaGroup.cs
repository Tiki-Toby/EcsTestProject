using System.Collections.Generic;
using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.WorldDiff.Internal
{
    public class ComponentDeltaGroup
    {
        public IComponentCollection Component;
        
        public object Data;
        
        public List<int> ChangedEntities;
        public List<int> RemovedFromEntities;

        public override string ToString()
        {
            return $"ComponentDeltaGroup {Component.GetComponentType().FullName}";
        }

        public void ApplyChanges(EcsWorld world)
        {
            Component.ApplyDiff(world, ChangedEntities, Data, RemovedFromEntities);
        }

        public ComponentDeltaGroupSerializable BuildSerializable()
        {
            var copy = new ComponentDeltaGroupSerializable();
            //copy.Id = Component.GetId();
            copy.ChangedEntities = ChangedEntities.ToArray();
            copy.RemovedFromEntities = RemovedFromEntities.ToArray();
            copy.FullComponentName = Component.GetComponentType().FullName;
            copy.Json = Component.ListToJson(Data);
                
            return copy;
        }

        public bool isEmpty()
        {
            if (ChangedEntities.Count != 0)
                return false;
            if (RemovedFromEntities.Count != 0)
                return false;
            return true;
        }
    }
}