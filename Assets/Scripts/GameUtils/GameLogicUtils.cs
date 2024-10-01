using System.Collections.Generic;
using CCE.Data;

namespace CCE.GameUtils
{
    public class GameLogicUtils
    {
        /// <summary>
        ///     Gets the page that contains <paramref name="time" />.
        /// </summary>
        /// <param name="pages"> Sorted list of pages in which to search. </param>
        /// <param name="time"> Time to snap to the page, in seconds. </param>
        /// <returns> Index of the page containing <paramref name="time" /> </returns>
        public static int SnapTimeToPage(List<Page> pages, double time)
        {
            int pageIndex = 0, remainingPages = pages.Count;

            // Finds the first page whose start time is greater than the given time.
            while (remainingPages > 0)
            {
                var step = remainingPages / 2;
                var candidate = pageIndex + step;
                if (pages[candidate].ActualStartTime <= time)
                {
                    pageIndex = candidate + 1;
                    remainingPages -= step + 1;
                }
                else
                {
                    remainingPages = step;
                }
            }

            return pageIndex - 1;
        }
    }
}