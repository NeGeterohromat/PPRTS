using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public class SendToggleRotation : MonoBehaviour
{
    public GameObject Obj;
    [DI] private ICommandAdder adder;
    public void Send()
    {
        adder.Add(new ToggleRotatingCommand(Obj.GetComponent<ProtoUnityAuthoring>().Entity()));
    }
}
