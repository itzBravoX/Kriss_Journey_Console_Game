﻿using kriss.Classes;
using lybra;
using System;
using System.Linq;
using System.Threading;

namespace kriss.Nodes
{
    public class NDialogue : NodeBase
    {
        ConsoleKeyInfo key;
        int selectedRow = 0;

        public NDialogue(NodeBase node) : base(node)
        {
            this.Init();
            RecursiveDialogues();
        }

        void RecursiveDialogues(int lineId = 0, bool isLineFlowing = true)           //lineid iterates over elements of dialogues[] 
        {
            if (lineId == 0)
                this.Init();

            if (IsVisited)
                isLineFlowing = false;
            // else, it depends

            Dialogue currentLine = Dialogues[lineId];                                    //cureent object selected in the iteration

        #region Drawing base element of the Dialog object (speech part)

            if(currentLine.PreComment != null)
            {
                Typist.RenderNonSpeechPart(isLineFlowing, currentLine.PreComment);

                Console.WriteLine();
                Console.WriteLine();
            }

            if (currentLine.Line != null)
                Typist.RenderLine(isLineFlowing, currentLine.Line, (ConsoleColor)currentLine.Actor, currentLine.IsTelepathy);

            if (currentLine.Comment != null)      
                Typist.RenderNonSpeechPart(isLineFlowing, currentLine.Comment);

            if (currentLine.Break || (IsLast && Dialogues.Count == Dialogues.IndexOf(currentLine) + 1))
            {
                Typist.WaitForKey(4);                                 //pause if it's marked as break or if it's last line of chapter
                Console.Clear();
            }
        #endregion

            if (currentLine.ChildId.HasValue)                                       //if it encounters a link, jump to the node
            {
                Typist.WaitForKey(4);
                this.AdvanceToNext(currentLine.ChildId.Value);
            }

            if (currentLine.Replies!= null && currentLine.Replies.Any())            //if there are replies inside, display choice
            {
                for (int i = 0; i < Dialogues[lineId].Replies.Count; i++)           //draw the replies, select them
                {
                    if (i == selectedRow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write("\t");
                    Console.Write((i + 1) + ". " + Dialogues[lineId].Replies[i].Line);

                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine();
                    Console.CursorLeft = Console.WindowLeft;
                }

                while (Console.KeyAvailable)
                    Console.ReadKey(true);
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Clear();

                    if (currentLine.Replies[selectedRow].ChildId.HasValue)                  //on selecion, either 
                        this.AdvanceToNext(currentLine.Replies[selectedRow].ChildId.Value); //navigate to node specified in selected reply
                    else                                                                    //or jump to the next line
                        RecursiveDialogues(Dialogues.FindIndex(l => l.LineName == currentLine.Replies[selectedRow].NextLine));                                   
                }

                if ((key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow) && selectedRow > 0)
                    selectedRow--;
                if ((key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow) && selectedRow < Dialogues[lineId].Replies.Count - 1)
                    selectedRow++;

                do
                {
                    if (lineId <= 0)
                        break;

                    lineId--;
                }
                while (Dialogues[lineId].Break == false);

                Console.Clear();
                RecursiveDialogues(lineId, false);               //redraw the node to allow the selection effect
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(currentLine.NextLine))
                {
                    int nextLineId = Dialogues.FindIndex(l => l.LineName == currentLine.NextLine);
                    RecursiveDialogues(nextLineId, isLineFlowing);
                }
                else 
                    if (Dialogues.Count > lineId + 1)                       
                        RecursiveDialogues(lineId + 1, isLineFlowing);

                this.AdvanceToNext(ChildId);
            }
        }
    }
}
