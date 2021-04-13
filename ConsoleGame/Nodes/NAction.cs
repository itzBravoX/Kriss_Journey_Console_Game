﻿using kriss.Classes;
using lybra;
using System;
using System.Collections.Generic;

namespace kriss.Nodes
{
    public class NAction : NodeBase
    {
        lybra.Action act = null;
        internal readonly List<ConsoleKeyInfo> keysPressed = new List<ConsoleKeyInfo>();
        internal string BottomMessage = string.Empty;
        ConsoleColor BottomMessageColor = ConsoleColor.DarkCyan;

        public NAction(NodeBase node) : base(node)
        {
            this.Init();
            PrepareForAction(true);
        }

        internal void PrepareForAction(bool isFirstTimeDisplayed)
        {
            ///go to bottom line and prepare prompt
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 2;
            Console.CursorLeft = Console.WindowLeft + Console.WindowWidth - 1;

            Console.ForegroundColor = ConsoleColor.Gray;
            if (!isFirstTimeDisplayed)
            {
                Console.CursorTop -= 1;
                Console.WriteLine(" You can't or won't do that. Try again.");
            }

            Console.Write(" \\> ");
            
            //if redrawing after backspacing, rewrite stack
            if (keysPressed.Count > 0)
                for (int i = 0; i < keysPressed.Count; i++)
                    Console.Write(keysPressed[i].KeyChar.ToString());

            do
            {
                ConsoleKeyInfo input = Console.ReadKey();
                switch (input.Key)
                {
                    case ConsoleKey.Tab:                 //if player presses tabs looking for help
                        TabPressed();
                        break;

                    case ConsoleKey.Backspace:          //to erase
                        BackSpacePressed(keysPressed);
                        break;

                    case ConsoleKey.Enter:
                        EnterPressed(keysPressed);
                        break;

                    default:
                        keysPressed.Add(input);          //normal keys are registered
                        break;
                }
            } 
            while (true);
        }

        #region Special keys pressed
        internal virtual void TabPressed() 
        {
            RedrawNode();

            List<string> helpObjects = new List<string>();                              //if this gets populated, show object help not verbs

            string[] words = ExtractWords();

            string matchingVerb = string.Empty;

            if (!string.IsNullOrWhiteSpace(words[0]))
            {
                foreach (var action in Actions)
                {
                    var verb = action.Verbs.Find(v => v.Term.Equals(words[0]));         //look into each action's verbs to see if there is our typed word
                    if (verb != null)
                    {
                        if (action.Objects.Count > 0)
                        {
                            foreach (var objContainer in action.Objects)                 //when the action is found, iterate through every object term
                                helpObjects.Add(objContainer.Objs[0].Term);
                        }
                        else
                            helpObjects.Add("Just do it.");

                        break;
                    }
                }
            }

            Console.CursorTop = Console.WindowHeight - 4;
            Console.CursorLeft = Console.WindowLeft;

            Console.ForegroundColor = ConsoleColor.DarkGray;

            if (helpObjects.Count > 0)
            {
                Console.WriteLine("Possible objects for the action typed: ");

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                foreach (var term in helpObjects)
                     Console.Write(term + " ");
            }
            else
            {
                Console.WriteLine("Possible actions here: ");

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                foreach (var action in Actions)
                    Console.Write(action.Verbs[0].Term + " ");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.CursorTop = Console.WindowHeight - 1;
            Console.CursorLeft = 0;

            if (DataLayer.CurrentChapter.Id == 2)
                if (Id == 2) //first action node. this if clause is to mock player just the first time they use help
                {
                    Console.CursorTop -= 1;
                    Console.WriteLine("\\> you pressed tab for help. noob.");
                }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\\>");
            Console.CursorLeft += 1;

            if (keysPressed.Count > 0)
                for (int i = 0; i < keysPressed.Count; i++)
                    Console.Write(keysPressed[i].KeyChar.ToString());
        }
        void BackSpacePressed(List<ConsoleKeyInfo> keysPressed)
        {
            if (keysPressed.Count > 0)
            {
                keysPressed.RemoveAt(keysPressed.Count - 1);

                RedrawNode(true);
                PrepareForAction(true);
            }
        }
        internal string[] ExtractWords() 
        {
            //reconstruct
            string typed = string.Empty;

            for (int i = 0; i < keysPressed.Count; i++)
                typed += keysPressed[i].KeyChar.ToString().ToLower();

            char[] delimiterChars = { ' ', ',', '.', ':', '\t', '!', '\r' };

            return typed.Split(delimiterChars);
        }
        internal virtual void EnterPressed(List<ConsoleKeyInfo> keysPressed)
        {
            if (keysPressed.Count > 0)
            {
                act = null;

                string[] words = ExtractWords();
               
                keysPressed.Clear();                                            //clear the stack after giving command

                string matchingVerb = string.Empty;

                foreach (string word in words)                                  //is there one word matching one action?
                {
                    foreach (var action in Actions)
                    {
                        var verb = action.Verbs.Find(v => v.Term.Equals(word));
                        if (verb != null)
                        {
                            act = action;
                            matchingVerb = word;                                //store the typed verb which triggered the action
                            break;
                        }
                    }
                }

                if (act != null)                                                //if there's an action available...
                {
                    if (act.Objects.Count == 0)                                 //...and is objectless...
                    {
                        if (!NodeMethods.Evaluate(act.Condition))                           //if for some reason Kriss can't do it, say it...
                            CustomRefusal(act.Condition.Refusal);
                        else
                        {
                            if (act.Effect != null)                             //in case the action has an Effect (inventory)
                                NodeMethods.StoreItem(act.Effect);

                            DisplaySuccess(act.Answer, act.ChildId);            //...just do it
                        }
                    }
                    else
                    {                                                           //...otherwise, examine Objects 
                        for (int i = 0; i < act.Objects.Count; i++)             
                        {
                            lybra.Object o = act.Objects[i];

                            foreach (string word in words)                      //is there a matching object available? just hand me the first you find please
                            {
                                foreach (var obj in o.Objs)
                                {
                                    if (obj.Term == word)                       //the action is right, and there is a acceptable object specified
                                    {
                                        if (!NodeMethods.Evaluate(o.Condition))             //if for some reason Kriss can't do it, say it...
                                            CustomRefusal(o.Condition.Refusal);
                                        else                                    //...otherwise, do it
                                        {
                                            if (o.Effect != null)               //in case the obj has an Effect (inventory)
                                                NodeMethods.StoreItem(o.Effect);

                                            DisplaySuccess(o.Answer, o.ChildId);
                                        }
                                    }
                                }
                            }
                        }
                        if (act.Answer != null)
                            DisplaySuccess(act.Answer, act.ChildId);
                        else
                            CustomRefusal(act.GetOpinion(matchingVerb));        //the action is right, but no required object is specified
                    }
                }
                else                                                            //if there's no action available, redraw node and display standard refuse
                {
                    RedrawNode();
                    BottomMessage = string.Empty;
                    PrepareForAction(false);
                }
            }
            else
            {
                RedrawNode();
                BottomMessage = string.Empty;
                PrepareForAction(true);
            }
        }
        #endregion

        void CustomRefusal(string refusal)
        {
            RedrawNode();

            Console.CursorTop = Console.WindowHeight - 4;
            Console.CursorLeft = Console.WindowLeft;

            refusal = "<<" + refusal + ">>";

            BottomMessage = refusal;
            BottomMessageColor = ConsoleColor.Cyan;

            NodeMethods.TextFlow(true, refusal, ConsoleColor.Cyan);
            Console.WriteLine();
            Console.WriteLine();

            PrepareForAction(true); //display prompt without standard refuse
        }
        void DisplaySuccess(string answer, int? childId = null) 
        {
            if (answer != null)
            {
                RedrawNode();

                BottomMessage = answer;
                BottomMessageColor = ConsoleColor.DarkYellow;

                Console.CursorTop = MeasureMessage(answer);
                Console.CursorLeft = Console.WindowLeft;

                NodeMethods.TextFlow(true, answer, ConsoleColor.DarkYellow);
                Console.WriteLine();
                Console.WriteLine();

                if (childId.HasValue)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("Press any key...");
                    Console.ReadKey(true);

                    this.AdvanceToNext(childId.Value);
                }
            }
            if (childId.HasValue)
               this.AdvanceToNext(childId.Value);

            //if everything fails:
            PrepareForAction(true); //display prompt without standard refuse
        }
        internal void RedrawNode(bool isDeleting = false)
        {
            Console.Clear();
            NodeMethods.TextFlow(false, Text);

            if (isDeleting)
                if (!string.IsNullOrWhiteSpace(BottomMessage))                      //decide if there is a Bottom Message and of which type
                {
                    switch (BottomMessageColor)
                    {
                        case ConsoleColor.DarkYellow:
                            Console.CursorTop = MeasureMessage(BottomMessage);
                            Console.CursorLeft = Console.WindowLeft;
                            break;

                        case ConsoleColor.Cyan:
                            Console.CursorTop = Console.WindowHeight - 4;
                            Console.CursorLeft = Console.WindowLeft;
                            break;
                    }
                    NodeMethods.TextFlow(false, BottomMessage, BottomMessageColor);
                }
        }

        internal int MeasureMessage(string answer)
        {
            //measure the lenght and the newlines in the answer to determine how up to go to start writing
            var newLines = System.Text.RegularExpressions.Regex.Matches(answer, "\\n").Count;
            var rows = answer.Length / Console.WindowWidth;

            return Math.Min(Console.WindowHeight - (rows + newLines), Console.WindowHeight - 5) - 2;
        }
    }
}
