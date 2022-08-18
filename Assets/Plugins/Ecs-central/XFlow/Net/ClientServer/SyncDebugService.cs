using System;
using System.IO;
using UnityEngine;

namespace XFlow.Net.ClientServer
{
    public class SyncDebugService
    {
        private readonly string hashDir;
        private readonly bool loggingEnabled;

        public SyncDebugService(string hashDir)
        {
            this.hashDir = hashDir;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            if (Directory.Exists(hashDir) && (hashDir.Contains("temp") || hashDir.Contains("tmp")))
            {
                try
                {
                    var files = Directory.GetFiles(hashDir);
                    Array.ForEach(files, file =>
                    {
                        File.Delete(file);
                    });
                    loggingEnabled = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"tmp folder '{hashDir}' exists but something goes wrong {ex}");
                }
            }
#endif
        }

        public SyncWorldLogger CreateLogger()
        {
            if (loggingEnabled)
                return new SyncWorldLogger(hashDir);
            return null;
        }
    }
}