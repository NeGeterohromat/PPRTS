using Leopotam.EcsProto.QoL;
using UnityEngine;

public class SendToggleRotation : MonoBehaviour
{
    public GameObject Obj;
    [DI] private IComandAdder adder;
    public void Send()
    {
        adder.Add(new ToggleRotatingCommand(Obj.GetComponent<EntityID>().ID));
    }
}
