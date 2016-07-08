////////////////////////////////////////////////////////////////////////////
//  Display.cs  - Handles the console display part of Project             //
//  ver 1.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     HP Split 13 *2 PC, Microsoft Windows 8, Build 9200      //
//  Application:  CSE681 Pr2, Code Analysis Project                       //
//  Source Author:       Neethu Haneesha Bingi,                           //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  nbingi@syr.edu                                          //
////////////////////////////////////////////////////////////////////////////
/*
Package Operations:
===================
Provides support for displaying String, Command Line arguments,
Titles with underline strings, New lines, displays file types and functions,
displays type relationships, displays file list.
Defines Display Class.

Public Interfaces:
==================
displayStr();			        //displays given string
displayTitle();			        //displays the text as a heading with underline char
displayCmdLineParse();	        //displays parsed command line arguments
displayNewLine();		        //displays new line
displayCommandLine();           //displays user entered command line aguments on console
displayFileList();              //Displays file list which are code analyzed
displayFileTypesAndFunctions(); //Displays types and functions details of code analysis
displayTypeRelationships();     //Displays relationships between given files

Build Process:
==============
 
Required Files:
---------------
Display.cs, RulesAndActions.cs

Build Command:
--------------
csc /target:exe /define:TEST_DISPLAY Display.cs RulesAndActions.cs

Maintanence History:
====================
ver 1.0 - 4 Oct 2014
- first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    //----< Display class -- handles display part of the project >-------------------
    public class Display
    {

        //----< Displays a string on console >------------------------------------
        public static void displayStr(string str)
        {
            Console.Write(str);
        }


        //----< Displays a new empty line on console >------------------------
        public static void displayNewLine()
        {
            Console.WriteLine();
        }


        //----< Displays title string with underline character passed on console>---------------------
        public static void displayTitle(string title, char underline = '=')
        {
            Console.Write("\n  " + title + "\n ");
	        string underlines = new string(underline, title.Length + 2);
            Console.Write(underlines);	        
        }


        //----< displays command line aguments on console >-------------------------
        public static void displayCommandLine(string[] args)
        {
            displayTitle("Command line arguments:", '-');
            Console.Write("\n  ");

        	if (args.Length == 0)
		        Console.Write("No command Line arguments passed.");
	        for (int i = 0; i < args.Length; i++)
		        Console.Write("\"" + args[i] + "\"  ");
	        Console.WriteLine();
        }


        //----< Displays the command line arguments after parsing >------------------------
        public static void displayCmdLineParse(string path, List<string> patterns, List<string> options)
        {
            Console.WriteLine("\n  Path:\t\t" + path);
            Console.Write("  Patterns:\t");
            foreach (string pattern in patterns)
                Console.Write(pattern + "  ");
            Console.Write("\n  Options:\t");
            foreach (string option in options)
                Console.Write(option + "  ");
            Console.WriteLine("\n");
        }


        //----< Displays file list for code analyzer processing >------------------------
        public static void displayFileList(List<string> fileList)
        {
            Display.displayStr("\n\n");
            Display.displayTitle("File List for Code Analyzer Processing: ", '*');
            for(int i = 0; i < fileList.Count; i++)            
                Display.displayStr("\n  - " + fileList[i]);    
        
            if(fileList.Count == 0)
                Display.displayStr("\n  No files matched with the given arguments for doing code analysis. \n  Exiting Program.\n\n\n\n");
        }


        //----< Displays types and functions details of code analysis >------------------------
        public static void displayFileTypesAndFunctions(string fileName, List<Elem> typesAndFuncs, bool showFuncSizeComp)
        {
            Display.displayStr("\n\n\n\n");
            Display.displayTitle("Processing File: " + fileName, '*');
            List<Elem> fileTypes = typesAndFuncs.FindAll(t => t.type != "function");
            List<Elem> fileFunctions = typesAndFuncs.FindAll(t => t.type == "function");

            Display.displayStr("\n");
            Display.displayTitle("Types defined:", '=');            
            if (fileTypes.Count == 0)
                Display.displayStr("\n  Above file does not have any `Types' defined.");
            else
            {
                Console.Write("\n  {0,-10}\t{1}", "Type", "Name(namespace.type)");
                Console.Write("\n  {0,-10}\t{1}", "----", "----");
                foreach (Elem e in fileTypes)
                    Console.Write("\n  {0,-10}\t{1}", e.type, ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + e.name);
            }

            Display.displayStr("\n");
            Display.displayTitle("Functions defined:", '=');
            if (fileFunctions.Count == 0)
                Display.displayStr("\n  Above file does not have any `Functions'.");
            else if (fileFunctions.Count > 0 && showFuncSizeComp)
            {
                Console.Write("\n  {0,-50}   {1,-6}   {2}", "Name(namespace.type.function)", "Size", "Complexity");
                Console.Write("\n  {0,-50}   {1,-6}   {2}", "----", "----", "----------");
                foreach (Elem e in fileFunctions)
                    Console.Write("\n  {0,-50}    {1,-6}   {2}", ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name, e.end - e.begin + 1, e.complexity);          
            }
            else if (fileFunctions.Count > 0 && !showFuncSizeComp)
            {
                //without size and complexity
                Console.Write("\n  {0}", "Name(namespace.type.function)");
                Console.Write("\n  {0}", "----");
                foreach (Elem e in fileFunctions)
                    Console.Write("\n  {0}", ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name);                         
            }
        }


        //----< Displays relationships between given files >------------------
        public static void displayTypeRelationships(List<Relationship> relationships)
        {
            Display.displayStr("\n\n\n\n");
            Display.displayTitle("Relationships between types in the given file set:", '*');

            if (relationships.Count == 0)
            {
                Display.displayStr("\n  No 'Relationships' between types in the given files.");
                return;
            }

            List<Relationship> inheritanceRel = relationships.FindAll(t => t.relation == "Inheritance");
            List<Relationship> compositionRel = relationships.FindAll(t => t.relation == "Composition");
            List<Relationship> aggregationRel = relationships.FindAll(t => t.relation == "Aggregation");            
            List<Relationship> usingRel = relationships.FindAll(t => t.relation == "Using");

            if (inheritanceRel.Count > 0)
                displayInheritanceRels(inheritanceRel);

            if (compositionRel.Count > 0)
                displayCompositionRels(compositionRel);

            if (aggregationRel.Count > 0)
                displayAggregationRels(aggregationRel);

            if (usingRel.Count > 0)
                displayUsingRels(usingRel);
        }


        //----< Displays Inheritance relationships details between given files >----------------
        private static void displayInheritanceRels(List<Relationship> relationships)
        {
            Display.displayStr("\n\n");
            Display.displayTitle("`Inheritance' relationships:", '=');
            String type2 = "";

            Console.Write("\n  {0,-35}   {1,-6}   {2}", "Type1-Name", "      ", "Type2-Name");
            Console.Write("\n  {0,-35}   {1,-6}   {2}", "----------", "      ", "----------");
            foreach (Relationship r in relationships)
            {
                if (r.type2.Contains(r.type2Namespace + "."))
                    type2 = r.type2;
                else
                    type2 = r.type2Namespace + "." + r.type2;

                Console.Write("\n  {0,-35}   {1,-6}   {2}", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1, "\"is-a\"", type2);
            }
        }


        //----< Displays Composition relationships details between given files >----------------
        private static void displayCompositionRels(List<Relationship> relationships)
        {
            Display.displayStr("\n\n\n");
            Display.displayTitle("`Composition' relationships:", '=');
            String type2 = "";

            Console.Write("\n  {0,-35}   {1,-9}   {2}", "Type1-Name", "         ", "Type2-Name");
            Console.Write("\n  {0,-35}   {1,-9}   {2}", "----------", "         ", "----------");
            foreach (Relationship r in relationships)
            {
                if (r.type2.Contains(r.type2Namespace + "."))
                    type2 = r.type2;
                else
                    type2 = r.type2Namespace + "." + r.type2;

                Console.Write("\n  {0,-35}   {1,-9}   {2}", type2, "\"part-of\"", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1);
            }
        }


        //----< Displays Aggregation relationships details between given files >----------------
        private static void displayAggregationRels(List<Relationship> relationships)
        {
            Display.displayStr("\n\n\n");
            Display.displayTitle("`Aggregation' relationships:", '=');
            String type2 = "";

            Console.Write("\n  {0,-35}   {1,-9}   {2}", "Type1-Name", "         ", "Type2-Name");
            Console.Write("\n  {0,-35}   {1,-9}   {2}", "----------", "         ", "----------");
            foreach (Relationship r in relationships)
            {
                if (r.type2.Contains(r.type2Namespace + "."))
                    type2 = r.type2;
                else
                    type2 = r.type2Namespace + "." + r.type2;

                Console.Write("\n  {0,-35}   {1,-9}   {2}", type2, "\"part-of\"", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1);
            }
        }


        //----< Displays Using relationships details between given files >----------------
        private static void displayUsingRels(List<Relationship> relationships)
        {
            Display.displayStr("\n\n\n");
            Display.displayTitle("`Using' relationships:", '=');
            String type2 = "";

            Console.Write("\n  {0,-35}   {1,-6}   {2}", "Type1-Name", "      ", "Type2-Name");
            Console.Write("\n  {0,-35}   {1,-6}   {2}", "----------", "      ", "----------");
            foreach (Relationship r in relationships)
            {
                if (r.type2.Contains(r.type2Namespace + "."))
                    type2 = r.type2;
                else
                    type2 = r.type2Namespace + "." + r.type2;

                Console.Write("\n  {0,-35}   {1,-6}   {2}", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1, "\"uses\"", type2);
            }
        }


        //----< Test Stub >--------------------------------------------------

        #if(TEST_DISPLAY)

        static void Main(string[] args)
        {
            //Testing all methods
            //Testing displayTitle()
	        Display.displayTitle("Testing Display Package");
	        Console.WriteLine("\n  displayTitle tested");

            //Testing displayStr()
            string testDisplayStr = "\n  Testing displayStr():";
            	string testStr = "\n  String String String";
	        Display.displayStr(testDisplayStr);
	        Display.displayStr(testStr);

	        //Testing displayNewLine()
	        Display.displayStr("\n\n  Testing displayNewLine():");
	        Display.displayNewLine();

            //Testing displayCommandLine()
            Display.displayStr("\n  Testing displayCommandLine():");
            Display.displayCommandLine(args);

            //Testing displayCommandLine()
            Display.displayStr("\n  Testing displayCmdLineParse():");            
            Display.displayCmdLineParse("./", new List<string>(), new List<string>());

            //Testing displayTypeRelationships()
            List<Relationship> relations = new List<Relationship>();
            Relationship relationship = new Relationship();
            relationship.type1 = "type1";
            relationship.type2 = "type2";
            relationship.relation = "Inheritance";
            relations.Add(relationship);
            Relationship relationship2 = new Relationship();
            relationship2.type1 = "type1";
            relationship2.type2 = "type2";
            relationship2.relation = "Aggregation";
            relations.Add(relationship2);
            Display.displayTypeRelationships(relations);

            Display.displayNewLine();
            Display.displayNewLine();
            Display.displayNewLine();
        }
        #endif
    }
}
