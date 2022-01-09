using CaseStudy.Helpers;

namespace CaseStudy.Controllers
{
    public class OrderHelper
    {
        public string email { get; set; }
        public OrderSelectionHelper[] selections { get; set; }
    }
}