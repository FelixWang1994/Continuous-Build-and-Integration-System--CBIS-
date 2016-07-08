////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs  - Client1's functions                             //
//                                                                        //
//                                                                        //
//  ver 1.0                                                               //
//  Language:     Visual C# 2013, Ultimate                                //
//  Platform:     Mac Pro, Microsoft Windows 7                            //
//  Application:  CSE681 Pr4, Dependency Analysis Project                 //
//  Author:       Kejian Wang,                                            //
//				  Master's - Computer Engineering,                        //
//				  Syracuse University,                                    //
//				  Kwang100@syr.edu                                          //
////////////////////////////////////////////////////////////////////////////
/*
Package Operations:
===================
The purpose of this module is to implement the client,
using WPF to provide users with easy operationsoversee the program flow.
This is the entry point to the application. All the following actions
are based on this package.

Public Interfaces:
==================
MainWindow()  // for initialize the window

Build Process:
==============
 
Required Files:
---------------
RulesAndActions.cs, dependency.cs

Build Command:
--------------
csc /target:exe RulesAndActions.cs, dependency.cs

Maintanence History:
====================
ver 1.2 - 21 Nov 2014
- add sub directories
ver 1.1 - 16 Nov 2014
- add package dependency button
ver 1.0 - 14 Nov 2014
- first release
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Project4;
using dependency;

namespace Client1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WCF.Receiver recvr;
        WCF.Sender sndr;
        Message rcvdMsg = new Message();
        Message sendMsg = new Message();

        Thread rcvThrd = null;
        delegate void NewMessage(Message msg);
        event NewMessage OnNewMessage;
        event NewMessage OnNewMessage1;
        event NewMessage OnNewMessage2;
        event NewMessage OnNewMessage3;
        event NewMessage OnNewMessage4;
        //define a server list for recording all the servers. Here use two servers as examples.
        List<String> servers = new List<string>();

        //Initialize the window
        public MainWindow()
        {
            InitializeComponent();
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            OnNewMessage1 += new NewMessage(OnNewMessageHandler1);
            OnNewMessage2 += new NewMessage(OnNewMessageHandler2);
            OnNewMessage3 += new NewMessage(OnNewMessageHandler3);
            OnNewMessage4 += new NewMessage(OnNewMessageHandler4);
            servers.Add("http://localhost:4000");    //Server # 1 address;
            servers.Add("http://localhost:4001");    //Server # 2 address;
            GetProjectsButton.IsEnabled = false;
            TypeAnalysisButton.IsEnabled = false;
            AllTypeRelation.IsEnabled = false;
            AllPackageRelation.IsEnabled = false;
            PackageAnalysisButton.IsEnabled = false;
            SubDir.IsChecked = true;
            CurrentDir.IsChecked = false;
        }
        //----< receive thread processing >------------------------------

        void ThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty
                rcvdMsg = recvr.GetMessage();
                if (rcvdMsg.type == "project names")
                {
                    // call window functions on UI thread
                    this.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      OnNewMessage,
                      rcvdMsg);
                }
                else if (rcvdMsg.type == "chosen project analysis")
                {
                    // call window functions on UI thread
                    this.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      OnNewMessage1,
                      rcvdMsg);
                }
                else if (rcvdMsg.type == "Global dependency")
                {
                    // call window functions on UI thread
                    this.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      OnNewMessage2,
                      rcvdMsg);
                }
                else if (rcvdMsg.type == "Chosen dependency")
                {
                    // call window functions on UI thread
                    this.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      OnNewMessage3,
                      rcvdMsg);
                }
                else if (rcvdMsg.type == "all project analysis")
                {
                    // call window functions on UI thread
                    this.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      OnNewMessage4,
                      rcvdMsg);
                }
                else 
                {
                }
            }
        }

        //----< called by UI thread when dispatched from rcvThrd >-------
        //handlewith all projects names
        void OnNewMessageHandler(Message msg)
        {
            
            mainListBox.Items.Add(msg.content);
            if (TypeAnalysisButton.IsEnabled == false)
            {
                TypeAnalysisButton.IsEnabled = true;
            }
            if (PackageAnalysisButton.IsEnabled == false)
            {
                PackageAnalysisButton.IsEnabled = true;
            }
            AllTypeRelation.IsEnabled = true;
            AllPackageRelation.IsEnabled = true;
        }

        //chosen projects type relation
        void OnNewMessageHandler1(Message msg)
        {
            mainTextBox.Text = msg.content;
        }

        //Global dependency
        void OnNewMessageHandler2(Message msg)
        {
            string m = "\n" + msg.dI.e1.fileName + "\n DEPENDS ON \n" + msg.dI.e2.fileName + "\n\n";
            string n = msg.dI.e1.name + "\n" + msg.dI.relation + "\n" + msg.dI.e2.name + "\n~~~~~~~~~~~";
            mainTextBox.AppendText(m);
            mainTextBox.AppendText(n);
        }

        //chosen package's dependency info
        void OnNewMessageHandler3(Message msg)
        {
            string m = "\n" + msg.dI.e1.fileName + "\n DEPENDS ON \n" + msg.dI.e2.fileName + "\n\n";
            string n = msg.dI.e1.name + "\n" + msg.dI.relation + "\n" + msg.dI.e2.name + "\n~~~~~~~~~~~";
            mainTextBox.AppendText(m);
            mainTextBox.AppendText(n);
        }
        
        //all projects's relationships
        void OnNewMessageHandler4(Message msg)
        {
            mainTextBox.Text = msg.content;
        }

        //Start listen
        private void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            string localPort = LocalPortTextBox.Text;
            string endpoint = "http://localhost:" + localPort + "/ICommunicator";

            try
            {
                recvr = new WCF.Receiver();
                recvr.CreateRecvChannel(endpoint);

                // create receive thread which calls rcvBlockingQ.deQ() (see ThreadProc above)
                rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
                rcvThrd.IsBackground = true;
                rcvThrd.Start();
                ConnectButton.IsEnabled = true;
                ListenButton.IsEnabled = false;
                GetProjectsButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(localPort.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }

        //Connect to the chosen server.
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string remoteAddress;
            string endpoint;
            if (ServerComboBox.SelectedIndex == 0)   //Server # 1 is chosen
            {
                remoteAddress = servers[0];
                endpoint = remoteAddress + "/ICommunicator";
                sndr = new WCF.Sender(endpoint);
                GetProjectsButton.IsEnabled = true;
            }
            else if (ServerComboBox.SelectedIndex == 1)   //Server # 2 is chosen
            {
                remoteAddress = servers[1];
                endpoint = remoteAddress + "/ICommunicator";
                sndr = new WCF.Sender(endpoint);
                
            }
            else                                      //No server is chosen 
            {
                MessageBox.Show("Please choose a server!");
            }
        }

        private void GetProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainListBox.Items.Clear();
                sendMsg.port = LocalPortTextBox.Text;
                sendMsg.type = "Get Projects";
                sendMsg.isSub = CurrentDir.IsChecked == true ? "" : "/S";
                sndr.PostMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }

        private void TypeAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sendMsg.port = LocalPortTextBox.Text;
                sendMsg.type = "Chosen Project Analysis";
                sendMsg.isSub = CurrentDir.IsChecked == true ? "" : "/S";
                sendMsg.content = mainListBox.SelectedItem.ToString();
                sndr.PostMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }

        private void AllPackageRelation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainTextBox.Clear();
                sendMsg.port = LocalPortTextBox.Text;
                sendMsg.isSub = CurrentDir.IsChecked == true ? "" : "/S";
                sendMsg.type = "All Packages Analysis";
                sndr.PostMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }

        private void PackageAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainTextBox.Clear();
                sendMsg.port = LocalPortTextBox.Text;
                sendMsg.content = mainListBox.SelectedItem.ToString();
                sendMsg.isSub = CurrentDir.IsChecked == true ? "" : "/S";
                sendMsg.type = "Chosen Packages Analysis";
                sndr.PostMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }

        private void AllTypeRelation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sendMsg.port = LocalPortTextBox.Text;
                sendMsg.type = "Projects Analysis";
                sendMsg.isSub = CurrentDir.IsChecked == true ? "" : "/S";
                sndr.PostMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }
    }
}
