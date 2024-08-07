namespace CCE.Utils
{
    public interface IHighlightable
    {
        bool Highlighted { get; set; }
        void Highlight();
    }
}