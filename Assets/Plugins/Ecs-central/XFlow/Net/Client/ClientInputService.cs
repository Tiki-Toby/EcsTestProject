using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace XFlow.Net.Client
{
    /**
     * 
     */
    public class ClientInputService: IInputService
    {
        private NetClient client;
        private HGlobalWriter writer = new HGlobalWriter();
        private ComponentsCollection collection;

        public ClientInputService(NetClient client, ComponentsCollection collection)
        {
            this.client = client;
            this.collection = collection;
        }
        
        
        public void Input(EcsWorld inputWorld, int playerID, int tick, IInputComponent inputComponent)
        {
            //GetTick как раз указывает на тик который произойдет сейчас, но еще не начался, +1 тут не нужен
            var nextTick = client.GetWorld().GetTick();
            if (tick != nextTick)
            {
                Debug.LogError($"{tick} != {nextTick}");
            }

            writer.Reset();
            writer.WriteByteArray(P2P.P2P.ADDR_SERVER.Address, false);
            writer.WriteSingleT(0xff);
            writer.WriteSingleT(playerID);
            writer.WriteSingleT(tick);

            var cm = collection.GetComponent(inputComponent.GetType());
            cm.WriteSingleComponentWithId(writer, inputComponent);

            var array = writer.CopyToByteArray();

            client.Socket.Send(array);
        
        }
    }
}