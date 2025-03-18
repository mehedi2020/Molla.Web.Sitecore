using System.Collections.Generic;
namespace Molla.Foundation.OrderCloud.Models
{
    public class Meta
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<int> ItemRange { get; set; }
        public object NextPageKey { get; set; }
    }

    public class Xp
    {
        public string ImagePath { get; set; }
        public string DisplayHomePage { get; set; }
    }

    public class Category
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ListOrder { get; set; }
        public bool Active { get; set; }
        public string ParentID { get; set; }
        public int ChildCount { get; set; }
        public Xp xp { get; set; }
    }

    public class CategoryResponse
    {
        public Meta Meta { get; set; }
        public List<Category> Items { get; set; } // Renamed from Items to Categories
    }
}