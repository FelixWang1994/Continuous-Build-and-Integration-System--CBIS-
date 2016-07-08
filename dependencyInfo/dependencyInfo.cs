////////////////////////////////////////////////////////////////////////////
//  dependencyInfo.cs  - to record dependency informatian                 //
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
The purpose of this module is to record the dependency information

Public Interfaces:
==================
none

Build Process:
==============
 
Required Files:
---------------
RulesAndActions.cs

Build Command:
--------------
csc /target:exe RulesAndActions.cs

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
using CodeAnalysis;

namespace dependency
{
    public class dependencyInfo
    {
        public Elem e1 { get; set; }
        public Elem e2 { get; set; }
        public string relation { get; set; }

        //----< Test Stub >--------------------------------------------------

        #if(TEST_DEPENDENCYINFO)

        static void Main(string[] args)
        {
            Elem e1 = new Elem();
            Elem e2 = new Elem();
            dependencyInfo di = new dependencyInfo();
            di.e1 = e1;
            di.e2 = e2;
        }
        #endif
    }
}
