using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "SceneDataConfig", menuName = "Configs/SceneDataConfig", order = 4)]
    public class SceneDataConfig : ScriptableObject
    {
        [SerializeField] private Vector3[] playerSpawnPoints;
        [SerializeField] private DoorButtonPair[] doorButtonPairs;

        public DoorButtonPair[] DoorButtonPairs => doorButtonPairs;
        public DoorButtonPair GetDoorButtonPair(int index) =>
            doorButtonPairs[index < doorButtonPairs.Length ? index : doorButtonPairs.Length - 1];

        public Vector3[] PlayerSpawnPoints => playerSpawnPoints;
        public Vector3 GetPlayerSpawnPoint(int index) =>
            playerSpawnPoints[index < playerSpawnPoints.Length ? index : playerSpawnPoints.Length - 1];

        public Vector3 GetRandomPlayerSpawnPoint() => GetPlayerSpawnPoint(Random.Range(0, playerSpawnPoints.Length));
    }

    [Serializable]
    public struct DoorButtonPair
    {
        [SerializeField] private int doorId;
        [SerializeField] private int buttonId;
        [SerializeField] private Vector3 offsetFromStartPosition;
        [SerializeField] private float velocity;

        public int DoorId => doorId;
        public int ButtonId => buttonId;
        public Vector3 OffsetFromStartPosition => offsetFromStartPosition;
        public float Velocity => velocity;
    }
}