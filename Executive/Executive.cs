////////////////////////////////////////////////////////////////////////////
//  Executive.cs  - The first package that gets called.                   //
//                  Oversees the control flow in the entire application   //
//                                                                        //
//  ver 1.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     HP Split 13 *2 PC, Microsoft Windows 8, Build 9200      //
//  Application:  CSE681 Pr2, Code Analysis Project                       //
//  Source Author:       Neethu Haneesha Bingi,                           //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  nbingi@syr.edu  
//  Editor:       Kejian Wang
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
ver 2.0 - 20 Nov 2014
- delete some of the display and save the relationTable and packageTable for myself
ver 1.0 - 26 Sep 2014
- first release
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace CodeAnalysis
{
    //----< Executive class - controls flow of the application >--------------
    public class Executive
    {
        //XML output fileName to store code analysis results
        private string fileName = "../../../XMLOutputFiles/CodeAnalyzerXMLOutput_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".xml";
        public XDocument typeRelation = new XDocument(); ///// for typeRelation
        public XDocument packageRelation = new XDocument();///// for packageRelation

        //----< starts Code Analysis and displays results >--------------
        public void startCodeAnalysisAndDisplay(List<string> files, CommandLineParser clp)
        {            
            CodeAnalyzer ca = new CodeAnalyzer(files, clp.isOptionPresent("/R"));
            ca.doCodeAnalysis();

            bool showTypeRels = clp.isOptionPresent("/R");
            bool writeXMLOutput = clp.isOptionPresent("/X");

            if (writeXMLOutput)
            {
                XMLWriter outputXMLWriter = new XMLWriter();
                outputXMLWriter.addFileTypesAndFunctions(ca.fileTypes, !clp.isOptionPresent("/R"));

                if (showTypeRels)
                    outputXMLWriter.addTypeRelationships(Repository.getInstance().relationships);
                
                //saving xml output in an xml file
                outputXMLWriter.getXML().Save(fileName);
                typeRelation = outputXMLWriter.getXML();
            }

            Display.displayStr("\n\n");
        }
        //Start Code Analysis for Package analysis
        public void startCodeAnalysisAndDisplay(List<string> files, CommandLineParser clp, List<Elem> l)
        {
            CodeAnalyzer ca = new CodeAnalyzer(files, clp.isOptionPresent("/R"));
            ca.doCodeAnalysisForPackages(l);

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
                packageRelation = outputXMLWriter.getXML();

                Display.displayStr("\n\n\n\n");
                Display.displayTitle("Code Analyzer XML Output:", '*');
                Display.displayStr("\n\n  Code Analyzer xml format output is written in file: \n  " + Path.GetFullPath(fileName));
            }

            Display.displayStr("\n\n");
        }

        //----< Displays summary statistics for all the functions in the given files >---------
        public void displayFunctionsSummary()
        {
            Display.displayStr("\n\n");
            Display.displayTitle("Functions summary - all files:", '*'); 
            
            int totalLines = Repository.getInstance().locations.FindAll(t=> t.type == "function").Sum(t => t.end - t.begin + 1);
            int totalComplexity = Repository.getInstance().locations.FindAll(t => t.type == "function").Sum(t => t.complexity);
            int maxFunLines = Repository.getInstance().locations.FindAll(t => t.type == "function").Max(t => t.end - t.begin + 1);
            int maxFunComplexity = Repository.getInstance().locations.FindAll(t => t.type == "function").Max(t => t.complexity);            

            
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

        //----< Test Stub >--------------------------------------------------
        #if(TEST_EXECUTIVE)

        static void Main(string[] args)
        {            
	        Display.displayTitle("Code Analyzer:");
            Executive executive = new Executive();

            CommandLineParser clp = new CommandLineParser();
            clp.parseCommandLine(args);
            string path = clp.getPath();
            List<string> patterns = clp.getPatterns();
            List<string> options = clp.getOptions();

            
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
        }
        #endif
    }
}
