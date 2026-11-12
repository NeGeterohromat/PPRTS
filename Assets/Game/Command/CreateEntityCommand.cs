using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateEntityCommand : ICommand
{
    private GameObject Prefab;
    private Vector3 position;
    private int senderId;
    public CreateEntityCommand(GameObject Prefab, Vector3 position, int senderId) 
    {
        this.Prefab = Prefab;
        this.senderId = senderId;
        this.position = position;
    }

    public void Execute()
    {
        var obj = GameObject.Instantiate(Prefab, position, Quaternion.identity);

        var comps = obj.GetComponent<ProtoUnityAuthoring>().Components;
        var ownerComp = comps.Where(c=>c.GetType()==typeof(OwnerComponent)).First();
        var transformComp = comps.Where(c => c.GetType() == typeof(TransformDataComponent)).First();
        comps.Remove(ownerComp);
        comps.Remove(transformComp);

        var owner = new OwnerComponent() { OwnerID = senderId };
        var transform = new TransformDataComponent() { Position = position, Rotation = Quaternion.identity };

        comps.Add(owner);
        comps.Add(transform);
    }
}
