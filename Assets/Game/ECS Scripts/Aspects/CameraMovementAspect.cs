using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public class CameraMovementAspect : ProtoAspectInject
{
    public ProtoPool<CameraTagComponent> CameraTagComponentPool;
}

[ProtoUnityAuthoring]
public struct CameraTagComponent { }
