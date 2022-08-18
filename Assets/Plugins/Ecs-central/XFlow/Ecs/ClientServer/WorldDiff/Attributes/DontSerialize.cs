using System;

namespace XFlow.Ecs.ClientServer.WorldDiff.Attributes
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DontSerialize : Attribute
    {
    }
}