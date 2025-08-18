using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterRatingModel
    {
        public FilterRatingModel()
        {
            Ratings = new List<RatingModel>();
        }
        public string ProductRatingIds { get; set; }
        public IList<RatingModel> Ratings { get; set; }
    }
}
