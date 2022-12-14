using System;
using System.IO;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;

namespace XFlow.Net.ClientServer
{
    public class SyncWorldLogger : IWorldLogger
    {
        private string folder;
        struct LoggerState
        {
            public int tick;
            public bool inTick;
        }

        public SyncWorldLogger(string tmpFolder)
        {
            folder = tmpFolder;
        }
        public void Log(EcsWorld world, string str)
        {
            var state = GetState(world);
            var offset = state.inTick ? "    " : "";
            var txt = $"{offset}{str}";
            LogRaw(world, txt);
        }
        
        private void LogRaw(EcsWorld world, string str)
        {
#if true
            var worldName = world.GetDebugName();

            try
            {
                lock (world)
                {
                    using (var file = File.AppendText($"{folder}/{worldName}.log"))
                    {
                        file.WriteLine(str);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            
            //var tx = $"{worldName}:{world.GetTick()}) {str}";
#endif
        }

        ref LoggerState GetState(EcsWorld world)
        {
            return ref world.GetOrCreateUniqueRef<LoggerState>();
        }
        
        public void BeginTick(EcsWorld world, int tick)
        {
            GetState(world).tick = tick;
            GetState(world).inTick = true;
            //var b2 = world.GetUnique<Box2DWorldComponent>().WorldReference.ToInt64();
            LogRaw(world, $"tick {tick} [");//b2:0x{b2:X8}"); 
        }
        
        public void EndTick(EcsWorld world, int tick)
        {
            GetState(world).inTick = false;
            LogRaw(world, $"] {tick}\n");
        }
    }
}