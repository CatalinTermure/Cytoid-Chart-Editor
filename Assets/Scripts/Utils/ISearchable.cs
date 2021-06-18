namespace CCE.Utils
{
    /// <summary>
    /// Describes a data type that can be searched with <see cref="SearchableList{TQueryStruct,TSearchResult}"/>.
    /// </summary>
    /// <typeparam name="TResult"> The type of the result that will be returned after the search. </typeparam>
    public interface ISearchable<TResult>
    {
        public string Description { get; set; }
        public TResult Result { get; set; }
    }
}