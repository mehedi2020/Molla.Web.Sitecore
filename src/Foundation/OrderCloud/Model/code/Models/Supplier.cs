using System;
using System.Collections.Generic;

namespace Molla.Foundation.OrderCloud.Models.Models
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
        public string LogoUrl { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
    }

    public class Supplier
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
        public bool AllBuyersCanOrder { get; set; }
        public Xp xp { get; set; }
    }
    public class SupplierListResponse
    {
        public Meta Meta { get; set; }
        public List<Supplier> Items { get; set; }
    }

}