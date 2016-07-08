﻿/////////////////////////////////////////////////////////////////////
// ICommunicator.cs - Peer-To-Peer Communicator Service Contract   //
// ver 2.1                                                         //
// Jim Fawcett, CSE681 - Software Modeling & Analysis, Summer 2011
// Editor: Kejian Wang
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 2.1 : 18 Nov 14
 * - change the message from string to a class
 * ver 2.0 : 10 Oct 11
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 * ver 1.0 : 14 Jul 07
 * - first release
 */

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Project4;

namespace WCF
{
    [ServiceContract]
    public interface ICommunicator
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(Project4.Message msg);

        // used only locally so not exposed as service method

        Message GetMessage();
    }
}
