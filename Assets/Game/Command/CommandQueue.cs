using System.Collections.Generic;
using UnityEngine;

public class CommandQueue : IComandAdder, IComandPopper
{

    private Queue<ICommand> commands = new();

    public void Add(ICommand command) => commands.Enqueue(command);

    public ICommand Pop() => commands.Dequeue();

    public bool HasElements() => commands.Count > 0;
}

public interface IComandAdder
{
    public void Add(ICommand command);
}

public interface IComandPopper
{
    public ICommand Pop();
    public bool HasElements();
}
