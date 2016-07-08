////////////////////////////////////////////////////////////////////////////
//  XMLWriter.cs  - creating an XML file with code analysis output        //
//                  using System.Xml.Linq.XDocument                       //
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
Provides support for storing types, their functions and relationships with
other types in the given file set in an XML file format.
Defines XMLWriter class

Public Interfaces:
==================
addFileTypesAndFunctions();	   //Writes File Types and Functions details into the XML file
addTypeRelationships();        //Writes File Type Relationships into the XML file
getXML();                      //Returns the output XML XDocument

Build Process:
==============
 
Required Files:
---------------
XMLWriter.cs CodeAnalyzer.cs, RulesAndActions.cs

Build Command:
--------------
csc /target:exe /define:TEST_XMLWRITER XMLWriter.cs CodeAnalyzer.cs, RulesAndActions.cs

Maintanence History:
====================
ver 1.0 - 02 Oct 2014
- first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeAnalysis
{
    //----< XMLWriter class - converts CodeAnalyzer output into xml file format >------------
    public class XMLWriter
    {
        private XDocument xml = new XDocument();    //output xml file
        private XElement root;                      //xml root


        //----< Initializes XDocument with declaration and root element >----------
        public XMLWriter()
        {
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XComment comment = new XComment("Code Analyzer XML Output");
            xml.Add(comment);
            root = new XElement("CodeAnalyzerOutput");
            xml.Add(root);                      
        }


        //----< Returns the output XML XDocument >-------------------
        public XDocument getXML()
        {            
            return xml;
        }


        //----< Writes File Types and Functions details into the XML file >----------------
        public void addFileTypesAndFunctions(List<FileTypesInfo> fileTypesInfo, bool showFuncSizeComp)
        {
            XElement typesAndFunctions = new XElement("TypesAndFunctions");
            foreach (FileTypesInfo file in fileTypesInfo)
            {
                XElement fileTypeAndFunctions = new XElement("FileTypesFunctions", new XAttribute("filename", file.fileName));                
                XElement types = new XElement("Types");
                XElement functions = new XElement("Functions");
                List<Elem> fileTypes = file.typesAndFuncs.FindAll(t => t.type != "function");
                List<Elem> fileFunctions = file.typesAndFuncs.FindAll(t => t.type == "function");

                //adding file types
                foreach (Elem e in fileTypes)
                {
                    XElement type = new XElement("Type", new XAttribute("type", e.type), new XAttribute("name", ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + e.name));
                    types.Add(type);
                }

                //adding file functions
                foreach (Elem e in fileFunctions)
                {
                    XElement function;
                    if (showFuncSizeComp)
                        function = new XElement("Function", new XAttribute("name", ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name), new XAttribute("size", e.end - e.begin + 1), new XAttribute("complexity", e.complexity));
                    else
                        function = new XElement("Function", new XAttribute("name", ((e.typeNamespace != "") ? e.typeNamespace + "." : "") + ((e.typeClassName != "") ? e.typeClassName + "." : "") + e.name));
                    functions.Add(function);
                }
                
                fileTypeAndFunctions.Add(types);
                fileTypeAndFunctions.Add(functions);
                typesAndFunctions.Add(fileTypeAndFunctions);
            }

            root.Add(typesAndFunctions);    //addding Type and Functions to the root element
        }


        //----< Writes File Type Relationships into the XML file >----------------
        public void addTypeRelationships(List<Relationship> relationships)
        {
            XElement typesRels = new XElement("TypeRelationships");
            //finding each relationship to display them in a group
            List<Relationship> inheritanceRel = relationships.FindAll(t => t.relation == "Inheritance");
            List<Relationship> compositionRel = relationships.FindAll(t => t.relation == "Composition");
            List<Relationship> aggregationRel = relationships.FindAll(t => t.relation == "Aggregation");           
            List<Relationship> usingRel = relationships.FindAll(t => t.relation == "Using");

            foreach (Relationship r in inheritanceRel)
            {                
                XElement relation = new XElement("Relationship");
                relation.Add(new XElement("Relation", "Inheritance"));
                relation.Add(new XElement("Type1", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1));
                relation.Add(new XElement("Type2", ((r.type2.Contains(r.type2Namespace + ".")) ? r.type2 : r.type2Namespace + "." + r.type2)));
                typesRels.Add(relation);
            }

            foreach (Relationship r in compositionRel)
            {
                XElement relation = new XElement("Relationship");
                relation.Add(new XElement("Relation", "Composition"));
                relation.Add(new XElement("Type1", ((r.type2.Contains(r.type2Namespace + ".")) ? r.type2 : r.type2Namespace + "." + r.type2)));
                relation.Add(new XElement("Type2", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1));
                typesRels.Add(relation);
            }

            foreach (Relationship r in aggregationRel)
            {
                XElement relation = new XElement("Relationship");
                relation.Add(new XElement("Relation", "Aggregation"));
                relation.Add(new XElement("Type1", ((r.type2.Contains(r.type2Namespace + ".")) ? r.type2 : r.type2Namespace + "." + r.type2)));
                relation.Add(new XElement("Type2", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1));
                typesRels.Add(relation);
            }

            foreach (Relationship r in usingRel)
            {
                XElement relation = new XElement("Relationship");
                relation.Add(new XElement("Relation", "Using"));
                relation.Add(new XElement("Type1", ((r.type1Namespace != "") ? r.type1Namespace + "." : "") + r.type1));
                relation.Add(new XElement("Type2", ((r.type2.Contains(r.type2Namespace + ".")) ? r.type2 : r.type2Namespace + "." + r.type2)));
                typesRels.Add(relation);
            }

            root.Add(typesRels); //addding TypeRelationships to the root element
        }


        //----< Test Stub >--------------------------------------------------

        #if(TEST_XMLWRITER)

        static void Main(string[] args)
        {
            List<Relationship> relations = new List<Relationship>();

            //adding inheritance relation
            Relationship relationship = new Relationship();
            relationship.type1 = "type1";
            relationship.type2 = "type2";
            relationship.relation = "Inheritance";
            relations.Add(relationship);

            //adding aggregation relation
            Relationship relationship2 = new Relationship();
            relationship2.type1 = "type1";
            relationship2.type2 = "type2";
            relationship2.relation = "Aggregation";
            relations.Add(relationship2);

            XMLWriter xmlWriter = new XMLWriter();
            xmlWriter.addFileTypesAndFunctions(new List<FileTypesInfo>(), true);    //writing types and functions into xml
            xmlWriter.addTypeRelationships(relations);                              //writing type relationships into xml
            
            //displaying xml output on console
            Console.Write(xmlWriter.getXML().ToString());
            Console.Write("\n\n\n");
        }
        #endif
    }
}
