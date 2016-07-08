////////////////////////////////////////////////////////////////////////////
//  Executive.cs  - The first package that gets called.                   //
//                  Oversees the control flow in the entire application   //
//                                                                        //
//  ver 1.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     HP Split 13 *2 PC, Microsoft Windows 8, Build 9200      //
//  Application:  CSE681 Pr2, Code Analysis Project                       //
//  Author:       Neethu Haneesha Bingi,                                  //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  nbingi@syr.edu                                          //
////////////////////////////////////////////////////////////////////////////
/*
Package Operations:
===================
The purpose of this module is oversee the program flow. This is the
entry point to the application. All the calls to the subsequent modules
will be routed from here.

Public Interfaces:
==================
None

Build Process:
==============
 
Required Files:
---------------
CodeAnalyzer.cs, CommandLineParser.cs, FileManager.cs, RulesAndActions.cs, Display.cs, XMLWriter.cs 

Build Command:
--------------
csc /target:exe CodeAnalyzer.cs CommandLineParser.cs FileManager.cs RulesAndActions.cs Display.cs XMLWriter.cs

Maintanence History:
====================
ver 1.0 - 26 Sep 2014
- first release
*/


/* 
**************READ ME**************
Code Analyzer Project
======================

Command Line Arguments:
=======================
Three divisions of command line arguments: path, pattern, option.

•	First argument is always considered as path. If the path is not valid (an existing directory), the program will display a message and will exit the program.
•	Any argument which is starting with '/' is considered as option. Options are   case-sensitive.
•	Remaining arguments other than above two are considered as patterns. If no pattern is entered, “*.cs” is taken as default pattern.

Options (case-sensitive) applicable for this project:
•	/S – searches recursively for files to be analyzed in the entire directory
•	/R – shows the relationships between all types defined in the file set.
•	/X – writes the output into a file in XML format, apart from displaying on console. 


XML Output file:
==============
The output xml file which gets generated when /X option is given is stored in the folder “XMLOutputFiles” of the project directory.
The name of the xml file will be:
CodeAnalyzerXMLOutput_yyyy-MM-dd-HHmmss.xml
(‘yyyy-MM-dd-HHmmss’ represents code analyzer execution date and time)


Sample command line arguments:
=============================
-	. /S /X /R *.cs	    path = “.”  Pattern={/S, /X, /R}  Option={.cs}
-	./TestCode /S /X	path = “./TestCode”  Pattern={/S, /X}  Option={.cs}
-	../TestCode 		path = “../TestCode”  Pattern={}  Option={.cs}

-	/S /X ./TestCode	path = “/S”  Pattern={/X}  Option={.cs, ./TestCode} 
Invalid Path (first argument must be always a valid existing path)
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
    //----< Executive class - controls flow of the application >--------------
    class Executive
    {
        //XML output fileName to store code analysis results
        private string fileName = "../../../XMLOutputFiles/CodeAnalyzerXMLOutput_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".xml";


        //----< starts Code Analysis and displays results >--------------
        private void startCodeAnalysisAndDisplay(List<string> files, CommandLineParser clp)
        {            
            CodeAnalyzer ca = new CodeAnalyzer(files, clp.isOptionPresent("/R"));
            ca.doCodeAnalysis();

            bool showTypeRels = clp.isOptionPresent("/R");
            bool writeXMLOutput = clp.isOptionPresent("/X");

            //Displaying types and function details on console
            foreach (FileTypesInfo file in ca.fileTypes)
                Display.displayFileTypesAndFunctions(file.fileName, file.typesAndFuncs, !showTypeRels);

            //Displaying type relationships on console if required
            if (showTypeRels)
                Display.displayTypeRelationships(Repository.getInstance().relationships);                

            //Writing xml output if required
            if (writeXMLOutput)
            {
                XMLWriter outputXMLWriter = new XMLWriter();
                outputXMLWriter.addFileTypesAndFunctions(ca.fileTypes, !clp.isOptionPresent("/R"));

                if (showTypeRels)
                    outputXMLWriter.addTypeRelationships(Repository.getInstance().relationships);
                
                //saving xml output in an xml file
                outputXMLWriter.getXML().Save(fileName);

                Display.displayStr("\n\n\n\n");
                Display.displayTitle("Code Analyzer XML Output:", '*');
                Display.displayStr("\n\n  Code Analyzer xml format output is written in file: \n  " + Path.GetFullPath(fileName));
            }

            Display.displayStr("\n\n");
        }


        //----< Displays summary statistics for all the functions in the given files >---------
        private void displayFunctionsSummary()
        {
            Display.displayStr("\n\n");
            Display.displayTitle("Functions summary - all files:", '*'); 
            
            int totalLines = Repository.getInstance().locations.FindAll(t=> t.type == "function").Sum(t => t.end - t.begin + 1);
            int totalComplexity = Repository.getInstance().locations.FindAll(t => t.type == "function").Sum(t => t.complexity);
            int maxFunLines = Repository.getInstance().locations.FindAll(t => t.type == "function").Max(t => t.end - t.begin + 1);
            int maxFunComplexity = Repository.getInstance().locations.FindAll(t => t.type == "function").Max(t => t.complexity);            

            Display.displayStr("\n\n  All functions, Lines count: \t " + totalLines);
            Display.displayStr("\n  All functions, Complexity: \t " + totalComplexity);

            Display.displayStr("\n  Highest function Lines: \t " + maxFunLines);
            Display.displayStr("\n  Highest function Complexity: \t " + maxFunComplexity + "\n");
            
            Display.displayTitle("Functions with lines greater than 50:", '-');
            List<Elem> moreLineFunctions = Repository.getInstance().locations.FindAll(t => t.type == "function" && (t.end - t.begin + 1) > 50);
            if (moreLineFunctions.Count == 0)
                Display.displayStr("\n  None");
            foreach(Elem e in moreLineFunctions)
                Display.displayStr("\n  " + ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name + ":\t " + (e.end - e.begin + 1));

            Display.displayStr("\n");
            Display.displayTitle("Functions with complexity greater than 10:", '-');
            List<Elem> moreComplexFunctions = Repository.getInstance().locations.FindAll(t => t.type == "function" && (t.complexity) > 10);
            if (moreComplexFunctions.Count == 0)
                Display.displayStr("\n  None");
            foreach (Elem e in moreComplexFunctions)
                Display.displayStr("\n  " + ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name + ":\t " + (e.complexity));
            Display.displayStr("\n\n");
        }


        //----< First entry point to the application which contains user inputs >--------------
        static void Main(string[] args)
        {            
	        Display.displayTitle("Code Analyzer:");
            Executive executive = new Executive();

	        //Parsing and Displaying Command Line arguments
            Display.displayNewLine();
            Display.displayCommandLine(args);
            CommandLineParser clp = new CommandLineParser();
            clp.parseCommandLine(args);
            string path = clp.getPath();
            List<string> patterns = clp.getPatterns();
            List<string> options = clp.getOptions();

            Display.displayNewLine();
            Display.displayTitle("Parsed Command Line Arguments:", '-');
            Display.displayCmdLineParse(path, patterns, options);
            
	        if (!clp.validateCmdLine())     //checking path argument validity
	        {
		        Display.displayStr("\n  Command Line Arguments does not contain 'Valid Existing Path'.");
		        Display.displayStr("\n  Exiting Program.\n\n\n\n");
		        return;
	        }

            try
            {
                FileManager fm = new FileManager();
                foreach (string pattern in patterns)
                    fm.addPattern(pattern);
                if (clp.isOptionPresent("/S"))
                    fm.setRecurse(true);
                fm.findFiles(path);
                List<string> files = fm.getFiles();
                Display.displayFileList(files);

                if (files.Count == 0)           //No files                                
                    return;                              

                executive.startCodeAnalysisAndDisplay(files, clp);
                executive.displayFunctionsSummary();
            }
            catch (Exception e)
            {   
                Display.displayStr("\n\n    " + e.Message);   
            }            
            Display.displayStr("\n\n\n");
        }
    }
}
