using System.Collections.Generic;
using UnityEngine;

public class CommandQueue : IComandAdder
{
    public static CommandQueue FullAddPopInstance { get; private set; } = new();
    public static IComandAdder OnlyAddInstance { get; private set; } = FullAddPopInstance;

    private Queue<ICommand> commands = new();

    public void Add(ICommand command) => commands.Enqueue(command);

    public ICommand Pop() => commands.Dequeue();

    public bool HasElements() => commands.Count > 0;
}

public interface IComandAdder
{
    public void Add(ICommand command);
}
