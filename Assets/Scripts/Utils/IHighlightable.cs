namespace CCE.Utils
{
    public interface IHighlightable
    {
        bool Highlighted { get; }
        void Highlight();
    }
}