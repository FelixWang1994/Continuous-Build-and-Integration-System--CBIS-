////////////////////////////////////////////////////////////////////////////
//  Message.cs  - to record message informatian                           //
//                                                                        //
//                                                                        //
//  ver 1.0                                                               //
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
The purpose of this module is to record the message information with the objectives of the class

Public Interfaces:
==================
none

Build Process:
==============
 
Required Files:
---------------
dependencyInfo.cs

Build Command:
--------------
csc /target:exe dependencyInfo.cs

Maintanence History:
====================
ver 1.0 - 20 Nov 2014
- first release
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dependency;
using System.Xml.Linq;

namespace Project4
{
    public class Message
    {
        public string content { get; set; }      //to record main content
        public string url { get; set; }          //to record address
        public string port { get; set; }         //to record port
        public string type { get; set; }         //to record message types
        public string isSub { get; set; }    //to record message condition
        public dependencyInfo dI { get; set; }   //to record depency infomatio
        
        //----< Test Stub >--------------------------------------------------

        #if(TEST_MESSAGE)
        static void Main(string[] args)
        {
            Message m = new Message();
            string content = "love C#";
            m.content = content;
        }
        #endif
    }
}
