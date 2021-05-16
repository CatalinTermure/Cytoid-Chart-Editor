using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class CommandSystem
{
    static Stack<NoteCommand> commandStack = new Stack<NoteCommand>();
    static Stack<NoteCommand> redoStack = new Stack<NoteCommand>();

    public static void AppendInvoke(NoteCommand command)
    {
        commandStack.Push(command);
        command.Execute();
        redoStack.Clear();
    }

    public static void Undo()
    {
        if (!commandStack.Any()) return;

        var buffer = commandStack.Pop();
        redoStack.Push(buffer);
        buffer.Undo();

        PostCommandUpdate();
    }

    public static void Redo()
    {
        if (!redoStack.Any()) return;

        var buffer = redoStack.Pop();

        commandStack.Push(buffer);

        buffer.Execute();

        PostCommandUpdate();
    }

    static void PostCommandUpdate()
    {
        GameLogic instance = GameLogic.Instance;
        
        instance.CalculateTimings();
        instance.UpdateTime(GameLogic.Instance.CurrentPage.start_time);
    }
}
