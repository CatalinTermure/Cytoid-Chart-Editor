using System.Collections.Generic;
using System.Linq;
using CCE.Game;

namespace CCE.Commands
{
    internal static class CommandSystem
    {
        private static readonly Stack<NoteCommand> _commandStack = new Stack<NoteCommand>();
        private static readonly Stack<NoteCommand> _redoStack = new Stack<NoteCommand>();

        public static void AppendInvoke(NoteCommand command)
        {
            _commandStack.Push(command);
            command.Execute();
            _redoStack.Clear();
        }

        public static void Undo()
        {
            if (!_commandStack.Any()) return;

            var buffer = _commandStack.Pop();
            _redoStack.Push(buffer);
            buffer.Undo();

            PostCommandUpdate();
        }

        public static void Redo()
        {
            if (!_redoStack.Any()) return;

            var buffer = _redoStack.Pop();

            _commandStack.Push(buffer);

            buffer.Execute();

            PostCommandUpdate();
        }

        static void PostCommandUpdate()
        {
            GameLogic.ForceUpdate();
        }
    }
}