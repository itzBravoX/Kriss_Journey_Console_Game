﻿using lybra;
using System;
using System.Collections.Generic;
using System.Threading;

namespace kriss.Classes
{
    /// <summary>
    /// Extension methods for NodeBase
    /// </summary>
    public static class NodeMethods
    {
        public static void Init(this NodeBase node)
        {
            // start text
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan; //narrator, default color

            string text;
            if (node.IsVisited && node.AltText != null)
                text = node.AltText;
            else
                text = node.Text;
            
            Typist.RenderText(!node.IsVisited, text);
        }

        public static void AdvanceToNext(this NodeBase node, int childId)
        {
            // mark as visited
            DataLayer.SaveProgress(node);

            // if it closes story or section, go back to menu
            if (node.IsClosing)
                DataLayer.DisplayMenu();

            // if it closes chapter load the next chapter, else load next node
            if (node.IsLast)
                DataLayer.StartNextChapter();

            DataLayer.LoadNode(childId);
        }
    }
}
