////////////////////////////////////////////////////////////////////////////
//  CommandLineParser.cs  - Parses Command Line Arguments                 //
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
Provides support for parsing command line into path, patterns and options.
Defines CommandLineParser Class.

Public Interfaces:
==================
parseCommandLine();	   //parses command line arguments to find path, patterns, options.
getPath();             //Gets the path argument from command line parsing
getPatterns();         //Gets the patterns collection from command line parsing
getOptions();          //Gets the options collection from command line parsing
validateCmdLine();     //Validating command line arguments
isOptionPresent();     //checks whether a particular option is passed in command line argument or not

Build Process:
==============
 
Required Files:
---------------
CommandLineParser.cs

Build Command:
--------------
csc /target:exe /define:TEST_COMMANDLINEPARSER CommandLineParser.cs

Maintanence History:
====================
ver 1.0 - 17 Sep 2014
- first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
    //----< CommandLineParser class used for parsing Command Line Arguments >-----------
    public class CommandLineParser
    {
        private string path;
        private List<string> filePatterns = new List<string>();
        private List<string> options = new List<string>();


        //----< parses command line arguments to find path, patterns, options >--------
        public void parseCommandLine(string[] args)
        {
            // first argument as path
            if (args.Length > 0)
                path = args[0];             

            for(int i = 1; i< args.Length; i++)
            {
                //starting with '/' as options  
                if (args[i][0].Equals('/'))
                    options.Add(args[i]);        
                else
                    filePatterns.Add(args[i]);  //remanining are patterns
            }

            //adding default pattern, if no pattern is given
            if (filePatterns.Count == 0)
                filePatterns.Add("*.cs");       
        }


        //----< Gets the path argument after command line parsing >-----------
        public string getPath()
        {
            return path;
        }


        //----< Gets the patterns collection after command line parsing >-----------
        public List<string> getPatterns()
        {
            return filePatterns;
        }


        //----< Gets the options collection after command line parsing >-----------
        public List<string> getOptions()
        {
            return options;
        }


        //----< Validating command line arguments - checks whether given path is valid or not >--------
        public bool validateCmdLine()
        {                
            //whether directory exists or not
            if(!Directory.Exists(path))
                return false;	

	        return true;
        }

        
        //----< checks whether a particular option is passed in command line argument or not >------------
        public bool isOptionPresent(string token)
        {
	        foreach (string option in options)
	        {
		        if (option == token)
			        return true;
	        }
        	return false;
        }


        //----< Test Stub >--------------------------------------------------

        #if(TEST_COMMANDLINEPARSER)

        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrating CommandLineParser");
            Console.Write("\n =================================\n");

            CommandLineParser cmdLineParser = new CommandLineParser();
            cmdLineParser.parseCommandLine(args);            

            string path = cmdLineParser.getPath();
            List<string> patterns = cmdLineParser.getPatterns();
            List<string> options = cmdLineParser.getOptions();
                        
            Console.Write("\n  Parsing Command Line:");
            Console.Write("\n ----------------------");            
	        Console.WriteLine("\n  Path:\t " + path);
	        Console.Write( "  Patterns:\t ");
	        foreach (string pattern in patterns)
		        Console.Write(pattern + "  ");
	        Console.Write("\n  Options:\t ");
	        foreach (string option in options)
		        Console.Write(option + "  ");

            Console.WriteLine();
            //validating command line arguments
            if (cmdLineParser.validateCmdLine())
                Console.WriteLine("\n\n  Command Line arguments are valid.");
            else
                Console.WriteLine("\n\n  Command Line Arguments does not contain 'valid existing Path'.");
            Console.WriteLine("\n");
        }

        #endif
    }
}
