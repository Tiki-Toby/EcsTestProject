using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Utils;

namespace XFlow.Net.ClientServer
{
    public static class BaseServices
    {
        public static int GetFreeUnitEntity(EcsWorld world)
        {
            var filter = world.Filter<PlayerComponent>().End();
            foreach (var entity in filter)
            {
                var playerComponent = entity.EntityGetComponent<PlayerComponent>(world);
                if (playerComponent.id == -1)
                    return entity;
            }

            return -1;
        }

        public static int GetUnitEntityByPlayerId(EcsWorld world, int playerID)
        {
            var poolPlayer = world.GetPool<PlayerComponent>();

            var filter = world.Filter<PlayerComponent>().End();
            foreach (var entity in filter)
                if (poolPlayer.Get(entity).id == playerID)
                    return entity;

            return -1;
        }

        public static void JoinPlayer(EcsWorld world, int playerID)
        {
            //Debug.Log($"joinPlayer {playerID}");
            var entity = world.NewEntity();
            entity.EntityAddComponent<InputJoinPlayerComponent>(world) = new InputJoinPlayerComponent
                {leave = false, playerID = playerID};
            entity.EntityAddComponent<InputComponent>(world);
        }

        public static void LeavePlayer(EcsWorld world, int playerID)
        {
            //Debug.Log($"leavePlayer {playerID}");
            var entity = world.NewEntity();
            entity.EntityAddComponent<InputJoinPlayerComponent>(world) = new InputJoinPlayerComponent
                {leave = true, playerID = playerID};
            entity.EntityAddComponent<InputComponent>(world);
        }
    }
}