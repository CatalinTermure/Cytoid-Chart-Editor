
namespace CCE.Rendering
{
    public interface IChartToScreenCoordinatesConverter
    {
        float ClickNoteSize { get; }
        float FlickNoteSize { get; }
        float DragHeadNoteSize { get; }
        float CDragHeadNoteSize { get; }
        float DragChildNoteSize { get; }
        float HoldNoteSize { get; }
        float LongHoldNoteSize { get; }
        float ScreenSize { get; }

        float ScreenXFromChartX(double chartX);
        public float ScreenYFromChartY(double chartY);
    }
}