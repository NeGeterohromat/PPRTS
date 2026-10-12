using UnityEngine;

public class SendToggleRotation : MonoBehaviour
{
    public GameObject Obj;
    public void Send()
    {
        CommandQueue.OnlyAddInstance.Add(new ToggleRotatingCommand(Obj.GetComponent<EntityID>().ID));
    }
}
