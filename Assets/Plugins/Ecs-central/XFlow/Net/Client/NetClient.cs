using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.Client.Systems;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Grid.Components;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.Client.Socket;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using XFlow.Net.ClientServer.Protocol;
using XFlow.Utils;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace XFlow.Net.Client
{
    public class NetClient
    {
        public Action<EcsWorld> InitWorldAction;
        public Action ConnectedAction;


        public EcsWorld MainWorld { get; }
        public EcsWorld InputWorld { get; }
        public EcsWorld ServerWorld { get; private set; }
        
        public EcsWorld copyServerWorld { get; private set; }

        

        public bool Connected { get; private set; }

        private SyncDebugService syncDebug;
        private float lastUpdateTime { get; set; }
        private EcsSystems clientSystems;
        private EcsSystems serverSystems;
        private EcsSystems copyServerSystems;

        private WorldDiff dif2;
        private ComponentsCollection components;
        
        private readonly int playerID;

        private Stats stats = new Stats();
        
        public UnitySocket Socket { get; private set; }

        private float stepMult = 1f;
        private float stepOffset = 0.001f;

        private String serverUrl;

        private IEcsSystemsFactory systemsFactory;

        private HGlobalWriter writer = new HGlobalWriter();

        public NetClient(
            EcsWorld world, 
            [Inject (Id = "input")]EcsWorld inputWorld,
            [Inject (Id = "tmpHashesPath")]String tmpHashesPath,
            [Inject (Id = "serverUrl")]String serverUrl,
            ComponentsCollection pool, 
            IEcsSystemsFactory systemsFactory)
        {
            Application.targetFrameRate = 60;

            this.serverUrl = serverUrl;

            this.InputWorld = inputWorld;
            
            this.systemsFactory = systemsFactory;

            components = pool;
            syncDebug = new SyncDebugService(tmpHashesPath);
            WorldLoggerExt.logger = syncDebug.CreateLogger();


            //???????????????????? ?????????????????? id ???????????? ?? ?????????????? ?????? ?????????? ?????????????????????????? ????????????
            playerID = Random.Range(1000, 9999);

            MainWorld = world;
            /*
             * ?????? ?????????? ???????? GEN ???? ?????????????? ?? entity ?????????????????? ???? ???????????????????? ?? ?? ?? ???????????? ???????????????????? ???????? ????????????????,
             * ?? ???? ???????????????? ?????????? ????-???? ?????????????????????? Gen ?? ??????????????????
             */
            MainWorld.SetDefaultGen(15000);
            MainWorld.AddUnique<MainPlayerIdComponent>().value = playerID;
            
            stats.delaysHistory.Add(0);
            stats.delaysHistory.Add(0);
            stats.delaysHistory.Add(0);
        }


        /**
         * ?????????? ?????????????????? ???? ???????????? ???????? ?????? ?? ?????????????????? ?????????????????? ?? ??????, ???????????? ????????????????????????????????????
         */
        public void Start(string sendInitialWorld)
        {
            //???????????????????????? ?????? ?????? ?????????????????????? ?? ??????????????
            //???????????? ???????????????? ???? ?????????? ?????????? ?????????? ?? ???????????? ???????????????????? asyncmain
            var hello = new Packet {hello = new Hello {Text = "hello"}, playerID = playerID, hasHello = true};

            hello.hello.InitialWorld = sendInitialWorld;
            
            var url = $"{serverUrl}/{playerID}";
            var connection = new WebSocketConnection(hello, url, P2P.P2P.ADDR_SERVER);
            connection.OnConnected += () =>
            {
                Socket = connection.ExtractSocket();
                AsyncMain(connection.Response);
            };
            connection.Start();
        }
        
        public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                return;
            foreach (var item in source) 
                action(item);
        }

        private void Sim(int delay)
        {
            Profiler.BeginSample("NetClientSim");
            //Debug.Log($"world\n{LeoDebug.e2s(serverWorld)}");

            Profiler.BeginSample("SimServerWorld");
            //?????????????? ???????????????????????????? ???????????????????? ???????? ???? ????????????
            SyncServices.FilterInputs(InputWorld, ServerWorld.GetTick() - 10);

            stats.lastClientTick = MainWorld.GetTick();
            stats.lastReceivedServerTick = ServerWorld.GetTick();
            //?????????????????????? ?? ?????????????? ?????????????????? ??????


            //if (leo.GetCurrentTick(serverWorld) < leo.GetCurrentTick(currentWorld))
            
            
            if (delay != -999)
            {
                var prevDelay1 = stats.delaysHistory[stats.delaysHistory.Count - 1];
                var prevDelay2 = stats.delaysHistory[stats.delaysHistory.Count - 2];
                var prevDelay3 = stats.delaysHistory[stats.delaysHistory.Count - 2];

                if (delay > 0)
                {
                    //???????? ?????????????????? ????????????
                    if (delay > 1)
                    {
                        //delay=1 ???? ?????????????? ???? ??????, ?????? ????
                        stats.oppLags++;
                    }

                    if (delay > 50)
                        stepOffset = 0.5f;
                    else
                        stepOffset = 0.001f * delay;
                    
                    
                    if (delay == 1)
                    {
                        stepOffset = 0;
                    }
                }

                if (delay < 0 )
                {
                    //?????????? ?????????????? ?????????????? ???? ??????????????
                    stepOffset = 0.001f * delay;

                    if (delay == -1 && prevDelay1 == 0 && prevDelay2 == 0 && prevDelay3 == 0)
                    {
                        //?????????????????
                        stepOffset /= 2;
                    }

                    stats.lags++;
                }

                if (delay == 0)
                {
                    //?????? ?????????? ???????? ?????????????? ??????????????, ???????? delay ?????????????????? ???????? > 0  ?? ???? 1 ?????????????? ???????? ?????? ???? -1
                    stepOffset = -0.0005f;
                }
                
                
                stats.delaysHistory.Add(delay);
                if (stats.delaysHistory.Count > Stats.HISTORY_LEN)
                    stats.delaysHistory.RemoveAt(0);
            }

            //stepOffset = 0;
            stepMult = 1;

            var iterations = 0;
            
            
           
            Profiler.BeginSample("PrepareSimWorld");

            var gridComponent = copyServerWorld.GetUnique<GridComponent>();
            
            //???????????? box2d ???????? ?????????????? ????????????, ?????????? ?????? ???????????????????? copyServerWorld.CopyFrom
            //?? ???????????????????? ????????????
            Box2DServices.DestroyWorld(copyServerWorld);
            
            
            if (copyServerWorld.HasUnique<Box2DWorldComponent>())
            {
                
                //var oldBox2dWorld = copyServerWorld.GetUnique<Box2DWorldComponent>().WorldReference;
                //Box2DApi.DestroyWorld(oldBox2dWorld);
                
            }
            
            copyServerWorld.CopyFrom(ServerWorld);
            copyServerWorld.SetDebugName($"cp{ServerWorld.GetTick()}");
            Box2DServices.__ClearWorld(copyServerWorld);
            
            Box2DServices.ReplicateBox2D(ServerWorld, copyServerWorld, copyServerSystems);
            //Debug.Log($"repl {copyServerWorld.GetUnique<Box2DWorldComponent>().WorldReference}");
            

            //?? ServerWorld ???????? Grid ??????????????, ???????????? ?????? ?????????????????????? ?????? ????????????????, ??????????????????????
            copyServerWorld.AddUnique(gridComponent);

            Profiler.EndSample();

            var serverTick = copyServerWorld.GetTick();
            var clientTick = MainWorld.GetTick();
            
            string debug = $"{ServerWorld.GetUnique<TickComponent>().Value.Value}";
            
            //Debug.Log($"pr srv:{Leo.GetCurrentTick(ServerWorld).Value} client: {Leo.GetCurrentTick(MainWorld).Value}");
            
            stats.simTicksTotal = 0;
            while (copyServerWorld.GetTick() < MainWorld.GetTick())
            {
                Profiler.BeginSample("SimTick");
                SyncServices.Tick(copyServerSystems, InputWorld, copyServerWorld, debug);
                Profiler.EndSample();
                
                stats.simTicksTotal++;
                if (iterations > 500)
                {
                    Debug.LogWarning(
                        $"too much iterations {serverTick} -> {clientTick}, {clientTick - serverTick}");
                    break;
                }

                iterations++;
            }
            Profiler.EndSample();
            

            Profiler.BeginSample("Apply Main Dif");
            
            dif2 = WorldDiff.BuildDiff(components, MainWorld, copyServerWorld, dif2);
            
            //if (copyServerWorld.GetTick() != MainWorld.GetTick())
            //    Debug.LogError($"ticks not equal {copyServerWorld.GetTick()} != {MainWorld.GetTick()}");
            
            dif2.ApplyChanges(MainWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("replicate2");
            Box2DServices.DestroyWorld(MainWorld);
            Box2DServices.ReplicateBox2D(copyServerWorld, MainWorld, clientSystems);
            Profiler.EndSample();
            
            Box2DDebugViewSystem.ReplaceBox2D(MainWorld);
            Profiler.EndSample();
        }

        private async UniTask<string> AsyncMain(Packet packet)
        {
            try
            {
                return await AsyncMain0(packet);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return "";
        }
        
        private async UniTask<string> AsyncMain0(Packet packet)
        {
            components.RemapOrder(packet.hello.Components);
            
            while (!packet.hasWelcomeFromServer)
            {
                var msg = await Socket.AsyncWaitMessage();
                packet = P2P.P2P.ParseResponse<Packet>(msg.buffer);
            }
                
            if (!Application.isPlaying)
                throw new Exception("async next step for stopped application");

            //???????????????? ?????????????????? ???????? ?? ??????????????
            var data = Convert.FromBase64String(packet.WorldUpdate.difStr);
            var dif0 = WorldDiff.FromByteArray(components, data);

            InitWorldAction?.Invoke(MainWorld);

            clientSystems = new EcsSystems(MainWorld);
            clientSystems.AddWorld(InputWorld, "input");

            clientSystems.Add(systemsFactory.CreateSyncDebugSystem(true));
            systemsFactory.AddNewSystems(clientSystems, new IEcsSystemsFactory.Settings{AddClientSystems = true, AddServerSystems = false});
            clientSystems.Add(systemsFactory.CreateSyncDebugSystem(false));
            clientSystems.Add(new TickSystem());

            dif0.ApplyChanges(MainWorld);

            clientSystems.Init();


            ServerWorld = WorldUtils.CopyWorld(components, MainWorld);
            ServerWorld.SetDebugName("rsrv");

            serverSystems = new EcsSystems(ServerWorld);
            serverSystems.Add(systemsFactory.CreateSyncDebugSystem(true));
            serverSystems.AddWorld(InputWorld, "input");
            //serverSystems.Add(Box2DModule.CreateMainSystems(Config.POSITION_ITERATIONS, Config.VELOCITY_ITERATIONS));
            serverSystems.Add(new Box2DInitSystem());
            serverSystems.Add(new Box2DCreateBodiesSystem());
            serverSystems.Add(new Box2DUpdateInternalObjectsSystem());
            //serverSystems.Add(new Box2DWriteBodiesToComponentsSystem());
            serverSystems.Add(systemsFactory.CreateSyncDebugSystem(false));
            //serverSystems.Add(new TickSystem());

            //?????????? ???????? ?????? ?????? ???????????? ??????-???? ???? ?????????????? ???????????? ?????????? ?????????? ???????????????? ??????????????
            serverSystems.Add(new DeleteOutdatedInputEntitiesSystem());

            //SystemsAndComponents.AddSystems(Leo.Pool, serverSystems, false, false);
            serverSystems.Init();

            //Debug.Log($"world\n{LeoDebug.e2s(MainWorld)}");


            copyServerWorld = new EcsWorld("csrv");
            copyServerSystems = new EcsSystems(copyServerWorld);
            copyServerSystems.AddWorld(InputWorld, "input");
            
            copyServerSystems.Add(systemsFactory.CreateSyncDebugSystem(true));
            systemsFactory.AddNewSystems(copyServerSystems,
                new IEcsSystemsFactory.Settings{AddClientSystems = false, AddServerSystems = false});
            copyServerSystems.Add(systemsFactory.CreateSyncDebugSystem(false));
            copyServerSystems.Add(new TickSystem());


            copyServerWorld.CopyFrom(ServerWorld);
            
            Box2DServices.__ClearWorld(copyServerWorld);
            Box2DServices.ReplicateBox2D(ServerWorld, copyServerWorld, copyServerSystems);


            copyServerSystems.Init();



            Connected = true;
            Debug.Log("client started");

            ConnectedAction?.Invoke();

            lastUpdateTime = Time.realtimeSinceStartup;
            
           // for (int i = 0; i < 300; ++i)
            //    Leo.Tick(clientSystems, InputWorld, MainWorld, Leo.Inputs.ToArray(), Config.SyncDataLogging);
            
            try
            {
                while (true)
                {
                    if (Socket == null)
                        break;//closed connection
                    int delay = 0;
                    bool updated = false;
                    while (true)
                    {
                        var msg = Socket.PopMessage();
                        if (msg == null)
                        {
                            break;
                        }
                        
                        Profiler.BeginSample("Packet received");

                        updated = true;
                        packet = P2P.P2P.ParseResponse<Packet>(msg.buffer);

                        if (!Application.isPlaying) throw new Exception("async next step for stopped application");

                        //stats.diffSize = msg.buffer.Length;

                        //if (Input.GetKey(KeyCode.A))
                        //     break;

                        Profiler.BeginSample("Packet Diff Decode");
                        var byteArrayDifCompressed = Convert.FromBase64String(packet.WorldUpdate.difBinary);
                        stats.diffSize = byteArrayDifCompressed.Length;
                        var byteArrayDif = P2P.P2P.Decompress(byteArrayDifCompressed);
                        var dif = WorldDiff.FromByteArray(components, byteArrayDif);
                        Profiler.EndSample();

                        /*
                         * ???????? ???????????? ???????????? ?????? entity ?? ???????????? ???? id, ???? ???? ???????? ?????????????? ???????????? ?????? ?????????????????? dif
                         * ?????????? ?????????? ???????????????????? ??????, ?????? ?????????????????? ???????????? ??????????-???? ?????????? view component 
                         */
                        ForEach(dif.CreatedEntities, entity =>
                        {
                            if (!MainWorld.IsEntityAliveInternal(entity))
                                return;

                            if (entity.EntityHasComponent<TransformComponent>(MainWorld))
                            {
                                var go = entity.EntityGetComponent<TransformComponent>(MainWorld).Transform.gameObject;
                                Object.Destroy(go);
                            }

                            MainWorld.DelEntity(entity);
                        });


                        delay = packet.WorldUpdate.delay;

                        if (dif.CreatedEntities.Count > 0)
                        {
                            
                        }

                        //?????????????????? diff ?? ???????????????? ???????? ?????????????????????? ???? ??????????????
                        dif.ApplyChanges(ServerWorld);
                        ServerWorld.GetSyncLogger()?.BeginTick(ServerWorld, ServerWorld.GetTick()-1);
                        serverSystems.Run();
                        ServerWorld.GetSyncLogger()?.EndTick(ServerWorld, ServerWorld.GetTick());
                        Profiler.EndSample();
                    }

                    if (updated)
                        Sim(delay);
                    //else

                    await UniTask.WaitForEndOfFrame();
                    //WorldMono.log.write($"got update from server, {leo.getTime(world)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            await UniTask.WaitForEndOfFrame();
            return "";
        }


        public EcsWorld GetWorld()
        {
            return MainWorld;
        }

        public int GetPlayerID()
        {
            return playerID;
        }

        public void Update()
        {
            if (!Connected)
                return;

            //?????????????????? ???????? ?????? ?? ?????????????????? ????????????

            var deltaTime = MainWorld.GetDeltaSeconds();
            deltaTime *= stepMult;
            deltaTime += stepOffset;

            Tick(deltaTime, () =>
            {
                //???????? ?????????? ???????????? ???? ?????????????????? ???? ??????????????, ???? ?? ?????????????????? ???????? ?????? ?????? ????????????
                //if (Leo.GetCurrentTick(MainWorld).Value > stats.lastReceivedServerTick + 200)
                //    return;
                    
                //leo.ApplyUserInput(world);
                SendPing(MainWorld.GetTick());
                SyncServices.Tick(clientSystems, InputWorld, MainWorld, "");
                
            });

            //stepMult = 1;
            //stepOffset = 0;
        }

        private void SendPing(int currentTick)
        {
            if (currentTick % 5 != 0)
                return;
            
            writer.Reset();
            writer.WriteByteArray(P2P.P2P.ADDR_SERVER.Address, false);
            writer.WriteInt32(0xff);
            writer.WriteInt32(playerID);
            writer.WriteInt32(currentTick);
            writer.WriteInt32(components.GetComponent(typeof(PingComponent)).GetId());
                
            Socket.Send(writer.CopyToByteArray());
        }


        public void OnDestroy()
        {
            Socket?.Close();
            Socket = null;
        }

        public void Tick(float deltaTime, Action action)
        {
            if (deltaTime < 0)
            {
                deltaTime = 0.001f;
            }
            
            Profiler.BeginSample("Main Tick");
            //???????????? ???????? ???? ?????????????????????? ???????? ?????? ?? ?????????????? ?????? ???????????????? tickrate
            var iteration = 0;
            while (true)
            {
                var tm = Time.unscaledTime;
                if (lastUpdateTime + deltaTime > tm)
                    break;

                if (iteration > 200)
                {
                    Debug.LogWarning("too much iterations");
                    break;
                }

                lastUpdateTime += deltaTime;
                action();
                iteration++;
            }
            Profiler.EndSample();
        }

        public void OnGUI()
        {
            if (!Connected)
                return;

            
            //GUILayout.Label(GetDebugString());
        }

        private StringBuilder sb = new StringBuilder(512);
        public string GetDebugString()
        {
            sb.Clear();
#if UNITY_EDITOR
            
            sb.AppendLine($"main entities {MainWorld.GetAllEntitiesCount()}");
            //sb.AppendLine($"input entities {InputWorld.GetAllEntitiesCount()}");
            sb.AppendLine($"playerID {playerID}");
            //sb.AppendLine($"future {futureTicks}");
            sb.AppendLine($"lags {stats.lags} vs {stats.oppLags}");
            sb.AppendLine($"stepOffset {stepOffset}");
            //sb.AppendLine($"stepMult {stepMult}");
            sb.AppendLine($"currentWorldTick {MainWorld.GetTick()}");
            sb.AppendLine($"serverWorldTick {ServerWorld.GetTick()}");

            //sb.AppendLine($"lastReceivedServerTick {stats.lastReceivedServerTick}");
            sb.AppendLine($"lastClientTick {stats.lastClientTick}");
            sb.AppendLine($"delta {stats.lastClientTick - stats.lastReceivedServerTick}");
            //sb.AppendLine($"simTicksTotal {stats.simTicksTotal}");


            //sb.Append("history: ");
            foreach (var i in stats.delaysHistory)
            {
                sb.Append($"{i.ToString()}".PadLeft(4));
            }

            sb.AppendLine();

            sb.AppendLine($"diffSize {stats.diffSize}");
            
            var size = MainWorld.GetAllocMemorySizeInBytes() / 1024;
                
            sb.AppendLine($"world size {size} kb");
#else
            sb.AppendLine($"diffSize {stats.diffSize}");
#endif
            
            return sb.ToString();
        }
    }
}