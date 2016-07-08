////////////////////////////////////////////////////////////////////////////
//  CodeAnalyzer.cs  - Analyzes code files.                               //
//                     Given a set of files. Finds all the types defined, //
//                     their functions and relationships with other types.//
//                     Finds functions size and complexity                //
//					   Type relationships include inheritance,            //
//                     composition, aggregation and using                 //
//  ver 1.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     HP Split 13 *2 PC, Microsoft Windows 8, Build 9200      //
//  Application:  CSE681 Pr2, Code Analysis Project                       //
//  Source Author:Neethu Haneesha Bingi,                                  //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  nbingi@syr.edu                                          //
//  Editor:       Kejian Wang                                             //
//                Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  Kwang100@syr.edu                                        //
////////////////////////////////////////////////////////////////////////////
/*
Package Operations:
===================
Provides support for findind types, their functions and relationships with
other types in the given file set.
Type relationships include inheritance, composition, aggregation and using.
Defines CodeAnalyzer class and FileTypesInfo class.

Public Interfaces:
==================
CodeAnalyzer.doCodeAnalysis();	         //does code analysis in the given files.
                                         //Finds types and thier functions. Finds function size and complexity.
                                         //Finds relationships between types which include inheritance,
                                            composition, aggregation and using.

Build Process:
==============
 
Required Files:
---------------
CodeAnalyzer.cs, Parser.cs, RulesAndActions.cs, Display.cs

Build Command:
--------------
csc /target:exe /define:TEST_CODEANALYZER CodeAnalyzer.cs Parser.cs RulesAndActions.cs Display.cs

Maintanence History:
====================
ver 2.0 - 20 Nov 2014
- Edited by Kejian Wang, add some functions about dependency analysis.
ver 1.0 - 26 Sep 2014
- first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    //----< FileTypesInfo class used to store file - types and functions details >----------------
    public class FileTypesInfo
    {
        //----< constructor - contains types and funtions for each file >-------
        public FileTypesInfo(string fileName, List<Elem> typesAndFuncs)
        {
            this.fileName = fileName;
            this.typesAndFuncs = typesAndFuncs;
        }
        public string fileName
        {
            get; set;
        }
        public List<Elem> typesAndFuncs
        {
            get; set;
        }
    }


    //----< CodeAnalyzer class used to do Code Analysis for the given file set >----------------
    public class CodeAnalyzer
    {
        private List<string> files;     //to store input files
        private bool isRelationshipsReq;
        CSsemi.CSemiExp semi;
        BuildCodeAnalyzer builder;
        public List<FileTypesInfo> fileTypes    //to store code analysis output
        { 
            get; set; 
        }


        //----< Constructor -- stores the files need to be analysed >----
        public CodeAnalyzer(List<string> files, bool isRelationshipsReq)
        {            
            this.files = files;
            this.isRelationshipsReq = isRelationshipsReq;
            fileTypes = new List<FileTypesInfo>();
            semi = new CSsemi.CSemiExp();
            semi.returnNewLines = false;
            builder = new BuildCodeAnalyzer(semi);
        }


        //----< Does Code Analysis for the given files. Find types and functions. Find type relationships if required >----
        public void doCodeAnalysis()
        {
            findTypesAndFuncs();
            if(isRelationshipsReq)
                findTypeRelationships();            
        }

        //Do Code Analysis for all packages
        public void doCodeAnalysisForPackages(List<Elem> l)
        {
            if (isRelationshipsReq)
                findTypeRelationships(l);
        }


        //----< Find types and their function details >----------------
        private void findTypesAndFuncs()
        {
            Parser parser = builder.build();
            int typesTableCount = 0;

            foreach (string file in files)
            {
                try
                {
                    if (!semi.open(file as string))
                        Display.displayStr("\n\n  Error: Can't open " + file + "\n");                                          

                    //parsing each file
                    while (semi.getSemi())
                        parser.parse(semi);

                    semi.close();

                    //storing each file information
                    Repository rep = Repository.getInstance();
                    fileTypes.Add(new FileTypesInfo(file, rep.locations.GetRange(typesTableCount, rep.locations.Count - typesTableCount)));                    
                }
                catch (Exception e)
                {                    
                    Display.displayStr("\n\n  Error while parsing file " + file + ": " + e.Message + "\n");
                }
                finally
                {
                    typesTableCount = Repository.getInstance().locations.Count;
                }
            }         
        }


        //----< Find relationships between types in the given files >----------------
        private void findTypeRelationships()
        {
            Parser parser = builder.buildRelsParser();

            foreach (object file in files)
            {
                try
                {
                    if (!semi.open(file as string))
                        Display.displayStr("\n\n  Error: Can't open " + file + "\n");

                    //parsing
                    while (semi.getSemi())
                        parser.parse(semi);

                    semi.close();
                }
                catch (Exception e)
                {
                    Display.displayStr("\n\n  Error while parsing file " + file + ": " + e.Message + "\n");
                }
            }
        }

        //Find Relatations between all packages
        private void findTypeRelationships(List<Elem> l)
        {
            Parser parser = builder.buildRelsParser(l);

            foreach (object file in files)
            {
                try
                {
                    if (!semi.open(file as string))
                        Display.displayStr("\n\n  Error: Can't open " + file + "\n");

                    //parsing
                    while (semi.getSemi())
                        parser.parse(semi);

                    semi.close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("It's me !!");
                    Display.displayStr("\n\n  Error while parsing file " + file + ": " + e.Message + "\n");
                }
            }
        }


        //----< Test Stub >--------------------------------------------------

        #if(TEST_CODEANALYZER)

        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrating CodeAnalyzer");
            Console.Write("\n =============================\n");

            List<string> files = new List<string>();
            files.Add("../../CodeAnalyzer.cs");

            CodeAnalyzer ca = new CodeAnalyzer(files, false);
            ca.doCodeAnalysis();              

            foreach (FileTypesInfo file in ca.fileTypes)
            {
                List<Elem> typesAndFuncs = file.typesAndFuncs;
                List<Elem> fileTypes = typesAndFuncs.FindAll(t => t.type != "function");
                List<Elem> fileFunctions = typesAndFuncs.FindAll(t => t.type == "function");
                
                Display.displayTitle("Processing File: " + file.fileName, '*');

                Display.displayStr("\n");
                Display.displayTitle("Types defined:", '=');          
                Console.Write("\n  {0,-10}\t{1}", "Type", "Name");
                Console.Write("\n  {0,-10}\t{1}", "----", "----");
                foreach (Elem e in fileTypes)
                    Console.Write("\n  {0,-10}\t{1}", e.type, e.typeNamespace + "." + e.name);

                Display.displayStr("\n");
                Display.displayTitle("Functions defined:", '=');
                Console.Write("\n  {0,-40}\t{1,-6}\t{2}", "Name", "Size", "Complexity");
                Console.Write("\n  {0,-40}\t{1,-6}\t{2}", "----", "----", "----------");
                foreach (Elem e in fileFunctions)
                    Console.Write("\n  {0,-40}\t {1,-6}\t  {2}", e.typeNamespace + "." + e.name, e.end - e.begin + 1, e.complexity);                           
            }

            Display.displayStr("\n\n\n");
        }
        #endif
    } //CodeAnalyzer class
}
