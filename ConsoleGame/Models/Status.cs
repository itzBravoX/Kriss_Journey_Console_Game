﻿using System.Collections.Generic;

namespace kriss.Models
{
    public class Status                             // represent the current status and the saved game
    {
        public int LastChapter { get; set; }        // to save progress
        public List<Item> Inventory { get; set; }
    }
}
