using System.Collections.Generic;
using UnityEngine;

public class CommandQueue : ICommandAdder, ICommandPopper
{

    private Queue<ICommand> commands = new();

    public void Add(ICommand command) => commands.Enqueue(command);

    public ICommand Pop() => commands.Dequeue();

    public bool HasElements() => commands.Count > 0;
}

public interface ICommandAdder
{
    public void Add(ICommand command);
}

public interface ICommandPopper
{
    public ICommand Pop();
    public bool HasElements();
}
