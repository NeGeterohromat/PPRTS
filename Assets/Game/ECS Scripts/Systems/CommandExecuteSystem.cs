using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LightTransport;

public class CommandExecuteSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private ICommandPopper _commandQueue;

    public void Init(IProtoSystems systems) 
    {
    }

    public void Run()
    {
        while (_commandQueue.HasElements())
        {
            _commandQueue.Pop().Execute();
        }
    }
}