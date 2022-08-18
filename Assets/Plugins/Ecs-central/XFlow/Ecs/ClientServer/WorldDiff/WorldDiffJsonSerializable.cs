using System;
using XFlow.Ecs.ClientServer.WorldDiff.Internal;

namespace XFlow.Ecs.ClientServer.WorldDiff
{
    [Serializable]
    public struct WorldDiffJsonSerializable
    {
        public int[] RemovedEntities;
        public int[] CreatedEntities;
        public short[] CreatedEntitiesGen;
        public ComponentDeltaGroupSerializable[] Groups;
    }
}