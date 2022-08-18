using System;

namespace XFlow.Ecs.ClientServer.WorldDiff.Internal
{
    [Serializable]
    public class ComponentDeltaGroupSerializable
    {
        //public int Id;
        
        public string FullComponentName;//replace with hash
        public string Json;
        
        public int[] ChangedEntities;
        public int[] RemovedFromEntities;

        public override string ToString()
        {
            return $"ComponentDeltaGroupSerializable {FullComponentName}";
        }
    }
}