using System;
using System.Collections.Generic;
using XFlow.EcsLite;
using XFlow.Utils;

namespace XFlow.Ecs.ClientServer.WorldDiff.Internal
{
    public interface IComponentCollection
    {
        Type GetComponentType();
        int GetId();
        void SetId(int id);
        
        ComponentDeltaGroup CompareEntities(IEcsPool poolFrom, IEcsPool poolTo, 
            List<int> compareEntities, List<int> createdEntities);
        ComponentDeltaGroup AddAllEntities(IEcsPool pool, List<int> entities);

        void CopyAllEntities(EcsWorld worldFrom, EcsWorld worldTo, List<int> entities);

        void ApplyDiff(EcsWorld world, List<int> entities, object componentsData, List<int> removedEntities);

        void Write(HGlobalWriter writer, ComponentDeltaGroup data);
        
        /**writes type of component too*/
        void WriteSingleComponentWithId(HGlobalWriter writer, object data);
        ComponentDeltaGroup ReadComponents(HGlobalReader reader);
        object ReadSingleComponent(HGlobalReader reader);
        object ListToArray(object lst);
        ComponentDeltaGroup GetOrCreateNewGroup();
        string ListToJson(object lst);
        void AddFromJson(string json, ComponentDeltaGroup group);
        
        void PutGroupBackToPool(ComponentDeltaGroup group);
    }

}