namespace CCE.Commands
{
    public interface ICommand
    {
        public void Undo();
        public void Execute();
    }
}