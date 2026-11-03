using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDSRetriever
{
    class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string LastRevisionDate { get; set; }
        public Item() { }
        public Item(string id, string name, string title, string lastRevisionDate)
        {
            Id = id;
            Name = name;
            Title = title;
            LastRevisionDate = lastRevisionDate;
        }
    }

    public class ImageItem
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public string CreationDateTime { get; set; }
        public string CdsVersionId { get; set; }
        public ImageItem() { }
    }
}
