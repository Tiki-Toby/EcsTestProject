using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class DoorButtonTrigger : MonoBehaviour
    {
        private EcsWorld world;
        private int buttonId;

        public void Init(EcsWorld world, int buttonId)
        {
            this.world = world;
            this.buttonId = buttonId;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player"))
            {
                var doorButtonPool = world.GetPool<DoorButtonComponent>();
                EcsFilter filter = world.Filter<DoorButtonComponent>().End();
                
                int triggeredButtonEntity = -1;
                foreach (int buttonEntity in filter)
                {
                    if (doorButtonPool.Get(buttonEntity).buttonId == buttonId)
                    {
                        triggeredButtonEntity = buttonEntity;
                        break;
                    }
                }
                
                if(triggeredButtonEntity > -1)
                    world.GetPool<TriggerEnterTag>().Add(triggeredButtonEntity);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag.Equals("Player"))
            {
                var doorButtonPool = world.GetPool<DoorButtonComponent>();
                EcsFilter filter = world.Filter<DoorButtonComponent>().Inc<TriggerEnterTag>().End();
                
                int outTriggeredButtonEntity = -1;
                foreach (int buttonEntity in filter)
                {
                    if (doorButtonPool.Get(buttonEntity).buttonId == buttonId)
                    {
                        outTriggeredButtonEntity = buttonEntity;
                        break;
                    }
                }

                if(outTriggeredButtonEntity > -1)
                    world.GetPool<TriggerExitTag>().Add(outTriggeredButtonEntity);
            }
        }
    }
}