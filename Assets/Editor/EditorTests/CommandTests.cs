using CCE.Commands;
using NUnit.Framework;

namespace CCE.EditorTests
{
    public class CommandTests
    {
        private class MockCommand : ICommand
        {
            public bool Executed { get; private set; }

            public void Execute()
            {
                Executed = true;
            }

            public void Undo()
            {
                Executed = false;
            }
        }

        [Test]
        public void AppendInvokeExecutesCommand()
        {
            var command = new MockCommand();
            CommandSystem.AppendInvoke(command);
            Assert.IsTrue(command.Executed);
        }

        [Test]
        public void UndoUndoesCommand()
        {
            var command = new MockCommand();
            CommandSystem.AppendInvoke(command);
            CommandSystem.Undo();
            Assert.IsFalse(command.Executed);
        }

        [Test]
        public void RedoRedoesCommand()
        {
            var command = new MockCommand();
            CommandSystem.AppendInvoke(command);
            CommandSystem.Undo();
            CommandSystem.Redo();
            Assert.IsTrue(command.Executed);
        }

        [Test]
        public void UndoDoesNotOverwriteRedo()
        {
            var command1 = new MockCommand();
            var command2 = new MockCommand();
            CommandSystem.AppendInvoke(command1);
            CommandSystem.AppendInvoke(command2);
            CommandSystem.Undo();
            CommandSystem.Undo();
            CommandSystem.Redo();
            CommandSystem.Undo();
            CommandSystem.Redo();
            CommandSystem.Redo();
            Assert.IsTrue(command1.Executed);
            Assert.IsTrue(command2.Executed);
        }

        [Test]
        public void AppendOverwritesRedo()
        {
            var command1 = new MockCommand();
            var command2 = new MockCommand();
            var command3 = new MockCommand();
            CommandSystem.AppendInvoke(command1);
            CommandSystem.AppendInvoke(command2);
            CommandSystem.Undo();
            CommandSystem.Undo();
            CommandSystem.Redo();
            CommandSystem.AppendInvoke(command3);
            CommandSystem.Redo();
            Assert.IsTrue(command1.Executed);
            Assert.IsFalse(command2.Executed);
            Assert.IsTrue(command3.Executed);
        }
    }
}