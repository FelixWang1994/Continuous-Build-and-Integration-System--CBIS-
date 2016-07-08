////////////////////////////////////////////////////////////////////////////
//  server2.cs  - the core of server2                                     //
//                                                                        //
//                                                                        //
//  ver 4.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     Mac Pro, Microsoft Windows 7                            //
//  Application:  CSE681 Pr4, Dependency Analysis Project                 //
//  Author:       Kejian Wang,                                            //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  Kwang100@syr.edu                                        //
////////////////////////////////////////////////////////////////////////////
/*
Package Operations:
===================
The purpose of this module is to implement server 2

Public Interfaces:
==================
none

Build Process:
==============
 
Required Files:
---------------
CodeAnalyzer.cs, CommandLineParser.cs, FileManager.cs, RulesAndActions.cs, Display.cs, XMLWriter.cs denpendencyInfo.cs, Message.cs

Build Command:
--------------
csc /target:exe CodeAnalyzer.cs, CommandLineParser.cs, FileManager.cs, RulesAndActions.cs, Display.cs, XMLWriter.cs denpendencyInfo.cs, Message.cs

Maintanence History:
====================
ver 4.0 - 21 Nov 2014
- deal with packages dependency
ver 3.0 - 19 Nov 2014
- deal with projects relationships
ver 2.0 - 18 Nov 2014
- using WCF to communicate with server 2 and clients
ver 1.0 - 17 Nov 2014
- first release
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CodeAnalysis;
using Project4;
using System.Xml.Linq;
using System.Xml;
using dependency;

namespace Server2
{
    class Server2
    {
        WCF.Receiver recvr;
        WCF.Sender sndr;
        Executive ex = new Executive();
        FileManager fm = new FileManager();
        XDocument relationXML = new XDocument();
        XDocument GlobalXML = new XDocument();
        XDocument packageXML = new XDocument();
        List<Elem> localTable = new List<Elem>();
        List<Elem> GlobalTable = new List<Elem>();
        List<dependencyInfo> GlobalPackageTable = new List<dependencyInfo>();
        Message rcvdMsg = new Message();
        Message sendMsg = new Message();
        string[] args;

        Thread rcvThrd = null;
        delegate void NewMessage(string msg);
        //----< receive thread processing >------------------------------

        void ThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty
                rcvdMsg = recvr.GetMessage();
                if (rcvdMsg.type.Equals("Get Projects"))     //Client requests for projects names
                {
                    Console.Write("\n  {0}", "received Get Projects");
                    sendProjectsNames();
                }
                else if (rcvdMsg.type.Equals("Chosen Project Analysis"))
                {
                    Console.Write("\n {0}", "received Projects Analysis");
                    sendProjectsAnalysis();
                }
                else if (rcvdMsg.type.Equals("From Server"))
                {
                    Console.Write("\n {0}", "received from server1");
                    mergeXML(rcvdMsg.content);
                }
                else if (rcvdMsg.type.Equals("All Packages Analysis"))
                {
                    Console.Write("\n {0}", "received All Packages Analysis");
                    sendAllPackagesAnalysis();
                }
                else if (rcvdMsg.type.Equals("Chosen Packages Analysis"))
                {
                    Console.Write("\n {0}", "Chosen Packages Analysis");
                    sendChosenPackagesAnalysis();
                }
                else if (rcvdMsg.type.Equals("Projects Analysis"))
                {
                    Console.Write("\n {0}", "Projects Analysis");
                    sendAllTypeRelation();
                }
            }
        }

        //Start Listen
        void listen()
        {
            string endpoint = "http://localhost:4001/ICommunicator";
            try
            {
                recvr = new WCF.Receiver();
                recvr.CreateRecvChannel(endpoint);

                // create receive thread which calls rcvBlockingQ.deQ() (see ThreadProc above)
                rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
                rcvThrd.IsBackground = true;
                rcvThrd.Start();
                Console.Write("\n  Server started");
                rcvThrd.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //handle with args
        private string[] handleArgs(string a)
        {
            List<string> temp = args.ToList();
            if (a == "")          //not required for sub dir
            {
                if (args.Contains("/S"))
                {
                    temp.Remove("/S");
                }
            }
            else                  //required for sub dir
            {
                if (!args.Contains("/S"))
                {
                    temp.Add("/S");
                }
            }
            args = temp.ToArray();
            return args;
        }

        //Sending projects name
        private void sendProjectsNames()
        {
            handleArgs(rcvdMsg.isSub);
            callCodeAnalzer();
            string remoteAddress;
            string port = rcvdMsg.port;
            remoteAddress = "http://localhost:" + port + "/ICommunicator";
            sndr = new WCF.Sender(remoteAddress);
            List<String> pn = new List<string>();
            pn = fm.getFiles();
            foreach (string s in pn)
            {
                Message tempMsg = new Message();
                tempMsg.content = s;
                tempMsg.type = "project names";
                sndr.PostMessage(tempMsg);
            }
        }

        //Sending project analysis
        private void sendProjectsAnalysis()
        {
            string remoteAddress;
            string port = rcvdMsg.port;
            remoteAddress = "http://localhost:" + port + "/ICommunicator";
            sndr = new WCF.Sender(remoteAddress);
            CommandLineParser tempClp = new CommandLineParser();
            string[] argument = { "", "/X", "/R" };
            tempClp.parseCommandLine(argument);
            string path = tempClp.getPath();
            List<string> patterns = tempClp.getPatterns();
            List<string> options = tempClp.getOptions();
            List<string> files = new List<string>();
            files.Add(rcvdMsg.content);
            ex.startCodeAnalysisAndDisplay(files, tempClp);
            ex.displayFunctionsSummary();
            relationXML = ex.typeRelation;
            Message tempMsg = new Message();
            tempMsg.content = relationXML.ToString();
            tempMsg.type = "chosen project analysis";
            sndr.PostMessage(tempMsg);
        }

        //send all package relationships
        private void sendAllTypeRelation()
        {
            handleArgs(rcvdMsg.isSub);
            string remoteAddress;
            string port = rcvdMsg.port;
            remoteAddress = "http://localhost:" + port + "/ICommunicator";
            sndr = new WCF.Sender(remoteAddress);
            callCodeAnalzer();
            Message tempMsg = new Message();
            tempMsg.content = relationXML.ToString();
            tempMsg.type = "all project analysis";
            sndr.PostMessage(tempMsg);
        }

        //send XML to other servers
        private void sendToS1()
        {
            string remoteAddress;
            remoteAddress = "http://localhost:4000/ICommunicator";
            sndr = new WCF.Sender(remoteAddress);
            Message tempMsg = new Message();
            tempMsg.content = relationXML.ToString();
            tempMsg.type = "From Server";
            sndr.PostMessage(tempMsg);
        }

        //merge localRelatonXML with the xml passed by server 1 into a global one for future analysis
        private void mergeXML(string otherXML)
        {
            XDocument otherTypeTable = new XDocument();
            otherTypeTable = XDocument.Parse(otherXML);
            IEnumerable<XElement> localtypetableType = relationXML.Descendants("TypesAndFunctions");
            IEnumerable<XElement> localtypetableRelation = relationXML.Descendants("TypeRelationships");
            IEnumerable<XElement> remotetypetableType = otherTypeTable.Descendants("TypesAndFunctions");
            IEnumerable<XElement> remotetypetableRelation = otherTypeTable.Descendants("TypeRelationships");
            XElement root = new XElement("root");
            GlobalXML.Add(root);
            XElement child1 = new XElement("Server1");
            child1.Add(remotetypetableType);
            child1.Add(remotetypetableRelation);
            root.Add(child1);
            XElement child2 = new XElement("Server2");
            child2.Add(localtypetableType);
            child2.Add(localtypetableRelation);
            root.Add(child2);
            GlobalXML.Save(@".\\2.xml");
            Console.WriteLine("Global.xml saved!");
            XMLtoTable(GlobalXML);
        }

        //a function that can change an xml to a list
        private List<Elem> XMLtoTable(XDocument GlobalXML)
        {
            XElement root = GlobalXML.Element("root");
            IEnumerable<XElement> Servers = root.Elements();

            foreach (XElement ser in Servers)
            {
                XElement typeandfuncs = ser.Element("TypesAndFunctions");

                IEnumerable<XElement> files = typeandfuncs.Elements();
                foreach (XElement file in files)
                {
                    XAttribute fileEle = file.Attribute("filename");
                    XElement Type = file.Element("Types");
                    IEnumerable<XElement> types = Type.Elements();
                    foreach (XElement type in types)
                    {
                        Elem tempElem = new Elem();
                        XAttribute typename = type.Attribute("name");
                        XAttribute typetype = type.Attribute("type");
                        tempElem.serverName = type.Parent.Parent.Parent.Parent.Name.ToString();
                        tempElem.fileName = type.Parent.Parent.FirstAttribute.ToString();
                        tempElem.type = typetype.Value.ToString();
                        tempElem.name = typename.Value.ToString();
                        GlobalTable.Add(tempElem);
                    }
                }
            }
            return GlobalTable;
        }

        //to check if the dependencyInfo has already in the globalPackageTable
        private bool ifHasSame(string p1, string p2)
        {
            bool flag = false;
            foreach (dependencyInfo dI in GlobalPackageTable)
            {
                if (p1.Equals(dI.e1.fileName) && p2.Equals(dI.e2.fileName))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        //get the package name of a type
        private Elem getPackageName(string type)
        {
            foreach (Elem e in GlobalTable)
            {
                if (e.name == type)
                {
                    return e;
                }
            }
            return null;
        }

        //get the gloabl package Table for future using
        private List<dependencyInfo> getPackageTable(XDocument packageXML)
        {
            XElement root = packageXML.Element("CodeAnalyzerOutput");
            XElement child = root.Element("TypeRelationships");
            IEnumerable<XElement> Relationships = child.Elements();

            foreach (XElement rel in Relationships)
            {
                XElement type1 = rel.Element("Type1");
                XElement type2 = rel.Element("Type2");
                if (type1.Value != type2.Value)
                {
                    Elem e1 = getPackageName(type1.Value);
                    Elem e2 = getPackageName(type2.Value);
                    if (e1 != null && e2 != null && e1.fileName != e2.fileName && !ifHasSame(e1.fileName, e2.fileName))
                    {
                        dependencyInfo tempDI = new dependencyInfo();
                        tempDI.e1 = e1;
                        tempDI.e2 = e2;
                        tempDI.relation = rel.Element("Relation").Value;
                        GlobalPackageTable.Add(tempDI);
                    }
                }
            }
            return GlobalPackageTable;
        }
       
        //sendAllPackagesAnalysis
        private void sendAllPackagesAnalysis()
        {
            handleArgs(rcvdMsg.isSub);
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
                fm = new FileManager();
                foreach (string pattern in patterns)
                    fm.addPattern(pattern);
                if (clp.isOptionPresent("/S"))
                    fm.setRecurse(true);
                fm.findFiles(path);
                List<string> files = fm.getFiles();
                if (files.Count == 0)           //No files                                
                    return;
                ex.startCodeAnalysisAndDisplay(files, clp, GlobalTable);
                packageXML = ex.packageRelation;
                packageXML.Save(@".\\package.xml");
                this.getPackageTable(packageXML);

                string remoteAddress;
                string port = rcvdMsg.port;
                remoteAddress = "http://localhost:" + port + "/ICommunicator";
                sndr = new WCF.Sender(remoteAddress);
                foreach (dependencyInfo dI in GlobalPackageTable)
                {
                    Message m = new Message();
                    m.type = "Global dependency";
                    m.dI = dI;
                    //m.xml = packageXML;
                    sndr.PostMessage(m);
                }
            }
            catch (Exception e)
            {
                Display.displayStr("\n\n    " + e.Message);
            }
            Display.displayStr("\n\n\n");
        }

        //send chosen package anlysis by changing the path
        private void sendChosenPackagesAnalysis()
        {
            handleArgs(rcvdMsg.isSub);
            CommandLineParser clp = new CommandLineParser();
            clp.parseCommandLine(args);
            string path = clp.getPath();
            List<string> patterns = clp.getPatterns();
            List<string> options = clp.getOptions();
            try
            {
                fm = new FileManager();
                foreach (string pattern in patterns)
                    fm.addPattern(pattern);
                if (clp.isOptionPresent("/S"))
                    fm.setRecurse(true);
                fm.findFiles(path);
                List<string> files = fm.getFiles();
                if (files.Count == 0)           //No files                                
                    return;
                ex.startCodeAnalysisAndDisplay(files, clp, GlobalTable);
                packageXML = ex.packageRelation;
                getPackageTable(packageXML);
                packageXML.Save(@".\\package.xml");
                Console.WriteLine("package.xml saved!");
                string remoteAddress;
                string port = rcvdMsg.port;
                remoteAddress = "http://localhost:" + port + "/ICommunicator";
                sndr = new WCF.Sender(remoteAddress);
                foreach (dependencyInfo dI in GlobalPackageTable)
                {
                    string s = "filename=\"" + rcvdMsg.content + "\"";
                    Console.WriteLine("---   " + s);
                    if (dI.e1.fileName.Equals(s) || dI.e2.fileName.Equals(s))
                    {
                        Console.WriteLine("+++++++++");
                        Message m = new Message();
                        m.type = "Chosen dependency";
                        m.dI = dI;
                        sndr.PostMessage(m);
                    }
                }
            }
            catch (Exception e)
            {
                Display.displayStr("\n\n    " + e.Message);
            }
        }

        //Call Code Analyzer
        private void callCodeAnalzer()
        {
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
                fm = new FileManager();
                foreach (string pattern in patterns)
                    fm.addPattern(pattern);
                if (clp.isOptionPresent("/S"))
                    fm.setRecurse(true);
                fm.findFiles(path);
                List<string> files = fm.getFiles();

                if (files.Count == 0)           //No files                                
                    return;

                ex.startCodeAnalysisAndDisplay(files, clp);
                ex.displayFunctionsSummary();
                relationXML = ex.typeRelation;
            }
            catch (Exception e)
            {
                Display.displayStr("\n\n    " + e.Message);
            }
            Display.displayStr("\n\n\n");
        }

        //Server 2 will start at the beginning of the program.
        static void Main(string[] args)
        {
                Server2 s2 = new Server2();
                s2.args = args;
                s2.callCodeAnalzer();
                s2.sendToS1();
                Console.WriteLine("s2 starts:");
                s2.listen();
        }
    }
}
