namespace CCE.Data
{
    public interface IChartChangedListener
    {
        void OnNoteAdded(Note note);
        void OnNoteChanged(Note note);
        void OnNoteRemoved(int noteId);

        void OnTempoAdded(Tempo tempo);
        void OnTempoChanged(Tempo tempo);
        void OnTempoRemoved(Tempo tempo);

        void OnPageDirectionModified(Page page);
    }
}