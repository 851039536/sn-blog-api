﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Snblog.Enties.Models
{
    public partial class SnNavigation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Describe { get; set; }
        public string Img { get; set; }
        public string Url { get; set; }
        public int TypeId { get; set; }
        public int UserId { get; set; }
        public DateTime? TimeCreate { get; set; }
        public DateTime? TimeModified { get; set; }

        public virtual SnNavigationType Type { get; set; }
        public virtual User User { get; set; }
    }
}
