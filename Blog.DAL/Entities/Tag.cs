using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.DAL.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
