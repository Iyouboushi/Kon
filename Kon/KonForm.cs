////////////////////////////////////////////////
////////////////PROGRAM INFO////////////////////
// "Kon" is a "learning bot" written in C#.   //
// The bot will connect to an IRC server and  //
// "watch" the conversations.  It will then   //
// "learn" from them.  You'll then be able to //
// talk to it and hopefully it'll hold a conv-//
// ersation.                                  //
////////////////////////////////////////////////
// "Kon" is an ongoing project started by     // 
// James Iyouboushi.                          //
// Email: Iyouboushi@gmail.com                //
////////////////////////////////////////////////
// This file was last updated on: 03/09/2015  //
////////////////////////////////////////////////
// Much of the basic IRC connection code is   //
// from:                                      //
// github.com/cshivers/IrcClient-csharp       //
// The GUI version of Kon would not be poss-  //
// ible without it.                           //
////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using TechLifeForum;
using System.Windows.Forms;
using System.Threading;

namespace Kon
{
    public partial class frmMain : Form
    {
        IrcClient client;
        ChatBotBrain chatBotBrain = new ChatBotBrain(true);
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Random r = new Random();

            for (int i = 0; i < 3; i++)
                txtNick.AppendText(r.Next(10).ToString());
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
                DoConnect();
            else
                DoDisconnect();
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client.Connected && !String.IsNullOrEmpty(txtSend.Text.Trim()))
            {
                if (txtChannel.Text.StartsWith("#"))
                    client.SendMessage(txtChannel.Text.Trim(), txtSend.Text.Trim());
                else
                    client.SendMessage("#" + txtChannel.Text.Trim(), txtSend.Text.Trim());

                AddToChatWindow("Me: " + txtSend.Text.Trim());
                txtSend.Clear();
                txtSend.Focus();
            }
        }
        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSend.PerformClick();
        }

        private void DoConnect()
        {
            if (String.IsNullOrEmpty(txtServer.Text.Trim()))
            {
                MessageBox.Show("Please specify a server");
                return;
            }
            if (String.IsNullOrEmpty(txtChannel.Text.Trim()))
            {
                MessageBox.Show("Please specify a channel");
                return;
            }
            if (String.IsNullOrEmpty(txtNick.Text.Trim()))
            {
                MessageBox.Show("Please specify a nick");
                return;
            }

            int port;
            if (Int32.TryParse(txtPort.Text, out port))
                client = new IrcClient(txtServer.Text.Trim(), port);
            else
                client = new IrcClient(txtServer.Text.Trim());

            AddEvents();
            client.Nick = txtNick.Text.Trim();

            btnConnect.Enabled = false;
            txtChannel.Enabled = false;
            txtPort.Enabled = false;
            txtServer.Enabled = false;
            txtNick.Enabled = false;
            rtbOutput.Clear(); // in case they reconnect and have old stuff there


            client.Connect();
        }
        private void DoDisconnect()
        {

            btnConnect.Enabled = true;
            txtChannel.Enabled = true;
            txtPort.Enabled = true;
            txtServer.Enabled = true;
            txtNick.Enabled = true;

            lstUsers.Items.Clear();
            txtSend.Enabled = false;
            btnSend.Enabled = false;

            client.Disconnect();
            client = null;

            btnConnect.Text = "Connect";
        }
        private void AddEvents()
        {
            client.ChannelMessage += client_ChannelMessage;
            client.ExceptionThrown += client_ExceptionThrown;
            client.NoticeMessage += client_NoticeMessage;
            client.OnConnect += client_OnConnect;
            client.PrivateMessage += client_PrivateMessage;
            client.ServerMessage += client_ServerMessage;
            client.UpdateUsers += client_UpdateUsers;
            client.UserJoined += client_UserJoined;
            client.UserLeft += client_UserLeft;
            client.UserNickChange += client_UserNickChange;
        }
        private void AddToChatWindow(string message)
        {
            rtbOutput.AppendText(message + "\n");
            rtbOutput.ScrollToCaret();
        }

        #region Event Listeners

        void client_OnConnect(object sender, EventArgs e)
        {
            txtSend.Enabled = true;
            txtSend.Focus();
            btnSend.Enabled = true;

            btnConnect.Text = "Disconnect";
            btnConnect.Enabled = true;

            if (txtChannel.Text.StartsWith("#"))
                client.JoinChannel(txtChannel.Text.Trim());
            else
                client.JoinChannel("#" + txtChannel.Text.Trim());

        }

        void client_UserNickChange(object sender, UserNickChangedEventArgs e)
        {
            lstUsers.Items[lstUsers.Items.IndexOf(e.Old)] = e.New;
        }

        void client_UserLeft(object sender, UserLeftEventArgs e)
        {
            lstUsers.Items.Remove(e.User);
        }

        void client_UserJoined(object sender, UserJoinedEventArgs e)
        {
            lstUsers.Items.Add(e.User);
        }

        void client_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            lstUsers.Items.Clear();
            lstUsers.Items.AddRange(e.UserList);
            
        }

        void client_ServerMessage(object sender, StringEventArgs e)
        {
            Console.WriteLine(e.Result);
        }

        void client_PrivateMessage(object sender, PrivateMessageEventArgs e)
        {
            AddToChatWindow("PM FROM " + e.From + ": " + e.Message);
        }

        void client_NoticeMessage(object sender,NoticeMessageEventArgs e)
        {
            AddToChatWindow("NOTICE FROM " + e.From + ": " + e.Message);
        }

        void client_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        void client_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            AddToChatWindow(e.From + ": " + e.Message);
            chatbotMessageControl(sender, e);
        }

        #endregion

        void chatbotMessageControl(object sender, ChannelMessageEventArgs e)
        {
            string[] messageString;
            messageString = e.Message.Split(new char[] { ' ' });

            if (((messageString[0].ToUpper() == "KON:") || (messageString[0].ToUpper() == "KON,") || (messageString[0].ToUpper() == txtNick.ToString().ToUpper() + ":")))
            {
                string conversation = e.Message.Replace(messageString[0], "");
                string reply = chatBotBrain.pullFromBrain(conversation, true, true);
                sendReply(reply);
            }
            else
            {
                if (!messageString[0].StartsWith("!"))
                    chatBotBrain.addToBrain(e.Message, e.From);
                else
                {
                    switch (messageString[0].ToUpper())
                    {
                        case "!UTTERNONSENSE":
                            string reply = chatBotBrain.totalNonsense();
                            sendReply(reply);
                            break;
                    }
                }
            }
        }

        private void sendReply(string reply)
       {
           if (reply.Contains("UNNAMED_USER"))
           {
               // Replace UNNAMED_USER
               Random randnum = new Random();
               string totalUsers = lstUsers.Items.Count.ToString();
               if (totalUsers == "1")
                   reply = reply.Replace("UNNAMED_USER", txtNick.Text);
               else
               {
                   string user;
                   int loops = 0;
                   do
                   {
                       // Get a random user.
                       Thread.Sleep(60);
                       int rnd = randnum.Next((lstUsers.Items.Count - 1));
                       user = (string)lstUsers.Items[rnd];
                       user = user.Replace("@", "");
                       user = user.Replace("+", "");
                       loops++;
                   } while ((user != txtNick.Text) && (loops <= 5));

                   reply = reply.Replace("UNNAMED_USER", user);
               }
           }

            if (txtChannel.Text.StartsWith("#"))
                client.SendMessage(txtChannel.Text.Trim(), reply);
            else
                client.SendMessage("#" + txtChannel.Text.Trim(), reply);

            AddToChatWindow(txtNick.Text + ": " + reply);
            txtSend.Focus();
        }


        // MENU OPTIONS
        private void mnuQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
