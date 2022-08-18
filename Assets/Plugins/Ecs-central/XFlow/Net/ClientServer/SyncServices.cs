using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components.Input;

namespace XFlow.Net.ClientServer
{
    public static class SyncServices
    {
        public static void FilterInputs(EcsWorld inputWorld, int tick)
        {
            var filter = inputWorld.Filter<InputTickComponent>().End();
            var pool = inputWorld.GetPool<InputTickComponent>();
            foreach (var entity in filter)
            {
                if (pool.Get(entity).Tick < tick)
                    inputWorld.DelEntity(entity);
            }
        }
        
        public static void Tick(EcsSystems systems, EcsWorld inputWorld, EcsWorld world, string debug="")
        {
            //обновляем мир 1 раз
            
            var currentTick = world.GetTick();
            var sl = world.GetSyncLogger();
            sl?.BeginTick(world, currentTick);
            
            var strStateDebug = "";
            /*
            if (writeHashes)
            {
                strStateDebug += WorldDumpUtils.DumpWorld(Components, inputWorld);
                strStateDebug += "\n\n\n";
            }*/
            
            
            //тик мира
            systems.ChangeDefaultWorld(world);
            systems.Run();

            sl?.EndTick(world, world.GetTick());

            currentTick = world.GetTick();

            //отладочный код, который умеет писать в файлы hash мира и его diff
            //var empty = WorldUtils.CreateWorld("empty", Pool);
            //var dif = WorldUtils.BuildDiff(Pool, empty, world, true);
            
            
            
            world.Log($"<- tick {currentTick}\n");

            /*
            if (writeHashes)
            {
                //var str = JsonUtility.ToJson(dif, true);
                var strWorldDebug = WorldDumpUtils.DumpWorld(Components, world);
                var hash = Utils.CreateMD5(strWorldDebug);

                //SyncLog.WriteLine($"hash: {hash}\n");

                var str = strStateDebug + "\n>>>>>>\n" +  strWorldDebug;
                var tick = currentTick.ToString("D4");

                //var tt = DateTime.UtcNow.Ticks % 10000000;
                
                using (var file = new StreamWriter($"{hashDir}/{hash}-{tick}-{world.GetDebugName()}-{debug}.txt"))
                {
                    file.Write(str);
                }
                
                using (var file = new StreamWriter($"{hashDir}/{tick}-{world.GetDebugName()}-{hash}-{debug}.txt"))
                {
                    file.Write(str);
                }
            }*/
        }
    }
}