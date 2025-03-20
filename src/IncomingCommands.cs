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
// This file was last updated on: 03/20/2025  //
////////////////////////////////////////////////

using System;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections;
using System.Xml;



namespace Kon
{
    partial class IRC
    {
        String configFile = "config.xml";  // the Configuration File

#region IncomingCommandParser
        /////////////////////////////////////////////////
        /////////////INCOMING COMMAND PARSER/////////////
        // The command parser takes the incoming text  //
        // from the channel and determines if it's a   // 
        // command it recognizes and will perform it,  //
        // if it is.                                   //
        /////////////////////////////////////////////////
        // Available Commands:                         //
        // ADMIN LEVEL:                                //
        // !addUser (nick), !remUser (nick),  !quit    //
        // !raw, !msg                                  //
        // USER LEVEL:                                 //
        // !toggleLB,                                  //
        // !channelUsers, !utter,                      //
        // !toggleTopic, !randomTalk [#],              //
        // !join #channel, !part #channel,             //
        // !toggleDoubleSentences,                     //
        // !toggleAnswerSearch, !toggleCopyAnswer      //
        // ALL LEVEL:                                  //
        // !splash, !konInfo, !brainInfo, !LBinfo      //
        // !haiku, !throw                              //
        /////////////////////////////////////////////////


        private void incomingCommand(string message, string nick, string location)
        {

            // Break the command into their tokenized parts so we can determine what to do
            string[] commandString;
            commandString = message.Split(new char[] { ' ' });

            // Get the first element of the command string and convert it to upper case
            string command = commandString[0].ToString();
            command = command.ToUpper();



#region CTCPCommands
            // Is it a CTCP Ping?
            if (command == "PING")
            {
                // Then let's ping back!
                String pingReply = "PING " + commandString[1];
                writer.WriteLine("NOTICE " + nick + " :" + pingReply);
                writer.Flush();
                return;
            }

            // Is it a CTCP Version Request?
            if (command == "VERSION")
            {
                // Then let's respond back!
                String versionReply = "VERSION " + version + "";
                writer.WriteLine("NOTICE " + nick + " :" + versionReply);
                writer.Flush();
                return;
            }
#endregion


            // Let's check to see if it's a user command first.  Now, if it happens to be !addUser or !remUser we 
            // need to send the user the admin is trying to add/remove.  Else, 

            if ((((((command == "!ADDUSER") || (command == "!REMUSER") || (command == "!THROW") || (command == "!ROLL") || (command == "!DICE") || (command == "!RANDOMTALK") || (command == "!JOIN") || (command == "!PART"))))))
                userCommand(command, commandString.Length > 1 ? string.Join(" ", commandString, 1, commandString.Length - 1) : "", nick, location);
            else if ((((command == "!RAW") || (command == "!MSG") || (command == "!HAIKU") || (command == "!MSN"))))
                userCommand(command, message, nick, location);
            else
                userCommand(command, " ", nick, location);

            // Check to see if RandomTalk is going to fire.  If so, we'll change the command slightly.
            Thread.Sleep(50);
            int rnd = randnum.Next(100);
            bool goingToRandomTalk = false;

            if ((rnd <= randomTalk) && (rnd != 0))
                goingToRandomTalk = true;

            // Time to see if someone is talking to the bot or if random talk was used.
            string addressingBot = nickNameMain;
            addressingBot = addressingBot.ToUpper();

            if ((((((command == addressingBot) || (command == addressingBot + ":") || (command == nickNameMain.ToUpper() + ",") || (command == "KON:") || (command == "KON,") || (goingToRandomTalk))))))
            {
                string conversation = "";
                string[] messageWithoutCommand;
                messageWithoutCommand = message.Split(new char[] { ' ' });
                int msgLength = messageWithoutCommand.Length - 1;

                for (int i = 1; i <= msgLength; i++)
                {
                    conversation = conversation + messageWithoutCommand[i] + " ";
                }

                converse(location, conversation, nick);
            }
            else
            {
                // We're going to force the bot to learn something
                myAI.addToBrain(message, nick);

                if (LB)
                {
                    try
                    {
                        myLB.addToBrain(message, nick);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
#endregion

#region converse(location, conversation, channelNick)
        private void converse(string location, string conversation, string channelNick)
        {

            if (!location.StartsWith("#"))
                location = channelNick;


            // Time to get the list of channel users.
            writer.WriteLine("NAMES " + channel);
            writer.Flush();
            Thread.Sleep(10);
            
                try
                {

                    // Replace some punctuation with spaces to prevent it from going nuts
                    conversation = conversation.Replace(",", "");

                    // Set the reply string that will be the reply from the bot
                    String reply = "";

                    // Throttle it a little bit in case there's too many people talking to it at once.
                    Thread.Sleep(500);


#region LBbrain
                    if (LB)
                    {
                        reply = myLB.pullFromBrain(conversation, topics, useAnswerLine);

                        if ((doubleSentences) && (reply.Length < 5))
                        {
                            Thread.Sleep(10);
                            string reply2 = myLB.pullFromBrain(reply, true, useAnswerLine);

                            string tempReply1 = reply.Remove(reply.Length - 1);
                            string tempReply2 = reply2.Remove(reply2.Length - 1);

                            if (tempReply1.ToUpper() != tempReply2.ToUpper())
                                reply = reply + " " + reply2;
                        }

                        
                        // Is the last character a comma, semi-colon or colon? If so, let's remove it.
                        string lastReplyCharacter = reply.Substring(reply.Length - 1, 1);
                        if (((lastReplyCharacter == ",") || (lastReplyCharacter == ";") || (lastReplyCharacter == ":")))
                            reply = reply.Remove(reply.Length - 1, 1);

                        // Let's clean up the reply a little bit.
                        if ((reply.StartsWith(" ACTION")) || (reply.StartsWith("ACTION")))
                        {
                            reply = reply.Replace("ACTION", "");
                            reply = reply.Replace("", "");
                            reply = reply.Trim();
                            reply = "ACTION " + reply + "";
                        }
                        else
                        {
                            reply = reply.Replace(" ACTION", " *");
                            reply = reply.Replace("ACTION", "*");
                        }


                        reply = reply.Trim();
                    }
#endregion

                    // This is a last ditch effort to try and respond to the user. If the other methods have failed,
                    // this will reach into kon's most basic brain and pull out a reply.
                    if (reply == "")
                        reply = myAI.pullFromBrain();


                    // Let's split the reply up and try to replace the UNNAMED_USER with an actual channel user.
                    string[] words = reply.Split(' ');
                    int wordCount = words.Length - 1;

                    for (int i = 0; i <= wordCount; i++)
                    {
                        if (words[i].ToString() == "UNNAMED_USER:")
                            words[i] = channelNick + ":";
                        else
                            words[i] = words[i].Replace("UNNAMED_USER", randomChannelUser());
                    }

                    // now to rebuild the reply
                    reply = "";
                    for (int i = 0; i < words.Length; i++)
                    {
                        reply += words[i] + " ";
                    }

                    reply = reply.Trim();

                    // Make sure the reply isn't too long so it doesn't crash the bot.
                    int replyLength = reply.Length;
                    if (replyLength >= 440)
                        reply = reply.Substring(0, 440);
                    else
                        reply = reply.Substring(0, replyLength);

                    // Let's try to correct some grammar
                    reply = grammarCheck(reply);

                    // Display the response and log it.
                    sendToChannel(reply, location);
                    Console.WriteLine(">> " + location + " : " + reply);
                }
                catch (Exception e)
                {
                    // Show the exception
                    Console.WriteLine(e.ToString() + CRLF);

                    // Sleep, before we try again
                    Thread.Sleep(1000);
                }
        }
#endregion


#region grammarCheck(reply)
        private string grammarCheck(string reply)
        {
            reply = reply.Replace("am you ", "are you ");
            reply = reply.Replace("are I ", "am I ");
            reply = reply.Replace("are i ", "am I ");
            reply = reply.Replace("I are ", "I am ");
            reply = reply.Replace("i are ", "I am ");
            reply = reply.Replace("thats", "that's");
            reply = reply.Replace("do you got", "did you get ");
            return reply;
        }
#endregion


#region UserCommands
        private void userCommand(string command, string commandParam, string nick, string location)
        {

            #region All Level Commands
            if (command == "!SPLASH")
            {
                Thread.Sleep(1000);   // throttle it so it won't commit a flood if tons of people use it.
                sendToChannel("Kon Splash!", location);
                Console.WriteLine(">> " + channel + " : Kon Splash!");
            }

            // !throw [line #] [Target]
            if (command.StartsWith("!THROW"))
            {
                Thread.Sleep(1000); // Prevent flooding

                try
                {
                    // Read all the lines from the file and store them in an array
                    string[] throwItems = File.ReadAllLines("throwitems.txt"); 

                    // If the file isn't empty let's move on..
                    if (throwItems.Length > 0)
                    {
                        
                        Random rand = new Random();
                        int randomIndex = rand.Next(throwItems.Length);

                        // Split commandParam into potential line number and target
                        string[] commandParts = commandParam.Split(new char[] { ' ' }, 2);
  
                        int lineNumber = 0;
                        string targetName = "";

                        // Check if the first part is a valid line number (such as !throw 25)
                        if (commandParts.Length > 0 && int.TryParse(commandParts[0], out lineNumber))
                        {
                            // Ensure the line number is within the valid range of the file
                            if (lineNumber > 0 && lineNumber <= throwItems.Length)
                            {
                                randomIndex = lineNumber - 1; // Adjust for zero-based index
                            }
                            else
                            {
                                sendToChannel($"Kon can't throw item # {lineNumber}, invalid line number!", location);
                                return;
                            }

                            // If a target was specified let's set that now. (such as !throw 25 Iyouboushi)
                            if (commandParts.Length > 1)
                            {
                                targetName = commandParts[1].Trim();
                            }
                        }
                        else
                        {
                            // If no number is detected, then just set the target to the commandParam
                            targetName = commandParam.Trim();
                        }

                        // Get the item and remove any extra spaces
                        string randomItem = throwItems[randomIndex].Trim(); 
                        randomIndex++; 

                        // If no target is provided, pick a random user from the channel
                        if (string.IsNullOrWhiteSpace(targetName))
                        {
                            targetName = randomChannelUser();

                            if (string.IsNullOrWhiteSpace(targetName))
                            {
                                // If no users are found let's just use "someone"
                                targetName = "someone"; 
                            }
                        }

                        // Send the message with the line number and target
                        string message = $"\u0001ACTION throws {randomItem} [Random item # {randomIndex}] at {targetName}";
                        sendToChannel(message, location);
                        Console.WriteLine($">> {channel} : {message}");
                    }
                    else
                    {
                        sendToChannel("Kon has nothing to throw!", location);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading throwitems.txt: " + ex.Message);
                    sendToChannel("Kon couldn't find anything to throw!", location);
                }
            }

            // !roll XdY (e.g., !roll 2d6)
            if ((command.StartsWith("!ROLL")) || (command.StartsWith("!DICE")))
            {
                Thread.Sleep(1000); // Prevent flooding

                try
                {
                    string rollInput = commandParam.Replace("!roll ", "").Trim();
                    int total = 0;
                    string resultString = "";

                    // Ensure valid format XdY (e.g., 2d6)
                    if (rollInput.Contains("d"))
                    {
                        string[] parts = rollInput.Split('d');

                        if (parts.Length == 2 && int.TryParse(parts[0], out int rolls) && int.TryParse(parts[1], out int limit))
                        {
                            if (rolls > 0 && limit > 0) // Ensure valid dice numbers
                            {
                                Random rand = new Random();

                                for (int i = 0; i < rolls; i++)
                                {
                                    int number = rand.Next(1, limit + 1);
                                    total += number;
                                    resultString += (i == 0) ? $"{number}" : $", {number}";
                                }

                                // Send roll results
                                string message = $"{nick} rolled {rolls}d{limit}: {resultString} (Total: {total})";
                                sendToChannel(message, location);
                                Console.WriteLine($">> {channel} : {message}");
                            }
                            else
                            {
                                sendToChannel("Invalid dice format! Use !roll XdY (e.g., !roll 2d6)", location);
                            }
                        }
                        else
                        {
                            sendToChannel("Invalid dice format! Use !roll XdY (e.g., !roll 2d6)", location);
                        }
                    }
                    else
                    {
                        sendToChannel("Invalid dice format! Use !roll XdY (e.g., !roll 2d6)", location);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing !roll command: " + ex.Message);
                    sendToChannel("An error occurred while rolling the dice!", location);
                }
            }


            // Haiku!
            if (command == "!HAIKU")
            {
                if (LB)
                {
                    String inputMessage = "";

                    if (commandParam != "")
                    {
                        string[] messageLine = commandParam.Split(new char[] { ' ' });
                        int msgLength = messageLine.Length - 1;
                        for (int i = 1; i <= msgLength; i++)
                        {
                            inputMessage = inputMessage + messageLine[i] + " ";
                        }
                    }

                    inputMessage = inputMessage.Trim();
                    string haiku = LBbrain.generateHaiku(inputMessage);
                    sendToChannel(haiku, location);
                }
                else
                    sendToChannel("4LB Brain not active and the Haiku cannot be generated", location);
            }


            if (command == "!BRAININFO")
            {
                sendToChannel("My brain contains " + myAI.getBrainLength() + " lines.", location);
            }

            if (command == "!LBINFO")
            {
                if (LB)
                    sendToChannel("My LB brain contains " + myLB.getBrainLength() + " lines.", location);
                else
                    sendToChannel("4LB Brain must be turned on in order to use this command.", location);
            }

            if (command == "!KONINFO")
            {
                sendToChannel(version, location);
            }
#endregion

#region User Level Commands
            if (command == "!TOGGLELB")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    if (LB)
                    {
                        LB = false;
                        sendToChannel("4No longer using the LB Brain for replies...", location);
                        myLB = null;
                    }
                    else
                    {
                        LB = true;
                        sendToChannel("4Now using the LB Brain in addition to my brain for replies...", location);
                        myLB = new LBbrain();
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!CHANNELUSERS")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                    sendToChannel(channelUsers, location);
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            //Private UTTER.. will send a random channel user a message
            if (command == "!PUTTER")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                    converse(location, "a", randomChannelUser());
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            // Regular UTTER.. will send a random message to the location the command was used
            if (command == "!UTTER")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                    converse(location, "thisisanutteryoushouldnthaveanylinesthatmatchthisstufflol", nick);
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            // Total nonsense... won't try to generate coherent sentences.
            if (command == "!TOTALNONSENSE")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    string nonsense = LBbrain.totalNonsense();
                    sendToChannel(nonsense, location);
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }
            
            if (command == "!JOIN")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                    join(commandParam); 
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!PART")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                    // Get the channel and leave
                    part(commandParam);  
                else
                    sendToChannel("4You do not have access to this command.", location);
            }


            if (command == "!TOGGLETOPIC")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    if (topics)
                    {
                        topics = false;
                        sendToChannel("4I will no longer try to locate the topic in what you say to me.", location);
                    }
                    else
                    {
                        topics = true;
                        sendToChannel("4I will now try to locate the topic in what you say to me.", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!TOGGLECOPYANSWER")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    if (useAnswerLine == false)
                    {
                        useAnswerLine = true;
                        sendToChannel("4I will reply with answers I find.", location);
                    }
                    else
                    {
                        useAnswerLine = false;
                        sendToChannel("4I will now stop replying with direct answers I find.", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!TOGGLEDOUBLESENTENCES")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    if (doubleSentences)
                    {
                        doubleSentences = false;
                        sendToChannel("4I will no longer try to combine two sentences.", location);
                    }
                    else
                    {
                        doubleSentences = true;
                        sendToChannel("4I will now try to combine two sentences together.", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!TOGGLEANSWERSEARCH")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    if (answerSearch)
                    {
                        answerSearch = false;
                        sendToChannel("4I will no longer try to find appropriate answers.", location);
                    }
                    else
                    {
                        answerSearch = true;
                        sendToChannel("4I will now try to find approprate answers.", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!RANDOMTALK")
            {
                if ((isUser(nick)) || (isAdmin(nick)))
                {

                    if ((commandParam != "") && (commandParam != " "))
                    {
                        int randomTalkP;
                        try
                        {
                            randomTalkP = System.Convert.ToInt32(commandParam);
                        }
                        catch (Exception e)
                        {
                            sendToChannel("4!randomTalk # where # is 1 to 100", location);
                            randomTalkP = randomTalk;
                            Console.WriteLine("Error: " + e.ToString());
                        }

                        randomTalk = randomTalkP;
                        sendToChannel("3RandomTalk now set to: " + randomTalk + "%", location);
                        Console.WriteLine("** Random Talk changed to: " + randomTalk + "%");
                    }


                    else
                    {
                        sendToChannel("3RandomTalk is currently set to: " + randomTalk + "%", location);
                        Thread.Sleep(100);
                        sendToChannel("3To change the percent use: !randomTalk # where # is 0 to 100", location);
                     }
                    
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!DINFO")
            {
                // For now, the nicks allowed to use this will be hardcoded
                if ((isUser(nick)) || (isAdmin(nick)))
                {
                    sendToChannel("Reconnects: " + reconnectAttempts + "/" + retryAttempts + " :: PING control is currently: " + canPing + " :: Ping Attempt #: " + pongAttempt, location);
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }


#endregion

#region Admin Level Commands
            if (command == "!QUIT")
            {
                // For now, the nicks allowed to use this will be hardcoded
                if (isAdmin(nick))
                {
                    writer.WriteLine("QUIT " + ":" + version);
                    writer.Flush();

                    Console.WriteLine("Kon can now be closed");
                    Thread.Sleep(500);
                    quit_program();
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!ADDUSER")
            {
                // For now, the nicks allowed to use this will be hardcoded
                if (isAdmin(nick))
                {
                    if ((commandParam == " ") || (commandParam == ""))
                        sendToChannel("4A name must be attached to this command.  !addUser (name)", location);
                    else
                    {
                        addUser(commandParam);
                        sendToChannel(commandParam + " added to the list of users", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!REMUSER")
            {
                // For now, the nicks allowed to use this will be hardcoded
                if (isAdmin(nick))
                {
                    if ((commandParam == " ") || (commandParam == ""))
                        sendToChannel("4A name must be attached to this command.  !addUser (name)", location);
                    else
                    {
                        remUser(commandParam);
                        sendToChannel(commandParam + " removed from the list of users", location);
                    }
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!RECONNECT")
            {
                // For now, the nicks allowed to use this will be hardcoded
                if (isAdmin(nick))
                {
                    writer.WriteLine("QUIT :Reconnecting");
                    writer.Flush();
                    Thread.Sleep(100);
                    setConnectionState(false);
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!RAW")
            {
                if (isAdmin(nick))
                {
 
                    string[] IncomingMessage;
                    IncomingMessage = commandParam.Split(new char[] { ' ' });

                    int messageStartingPoint = 1;

                    if (IncomingMessage.Length < 1)
                        sendToChannel("4Error with command.", location);

                    messageStartingPoint = 1;
                    // Determine what the message is.
                    int commandLength = IncomingMessage.Length - 1;
                    string message = "";

                    for (; messageStartingPoint <= commandLength; messageStartingPoint++)
                    {
                        message = message + IncomingMessage[messageStartingPoint] + " ";
                    }

                    writer.WriteLine(message);
                    writer.Flush();
                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }

            if (command == "!MSG")
            {
                if (isAdmin(nick))
                {
                    string[] IncomingMessage;
                    IncomingMessage = commandParam.Split(new char[] { ' ' });

                    string whereToSendMessage = "";
                    int messageStartingPoint = 1;

                    if (IncomingMessage.Length >= 2)
                        whereToSendMessage = IncomingMessage[1].ToString();
                    else
                        sendToChannel("4Error with command.", location);

                    messageStartingPoint = 2;
                    // Determine what the message is.
                    int commandLength = IncomingMessage.Length - 1;
                    string message = "";

                    for (; messageStartingPoint <= commandLength; messageStartingPoint++)
                    {
                        message = message + IncomingMessage[messageStartingPoint] + " ";
                    }

                    if (whereToSendMessage != "")
                        sendToChannel(message, whereToSendMessage);

                }
                else
                    sendToChannel("4You do not have access to this command.", location);
            }
            /*
             * I was going to try to get an MSN client to work with Kon, but it isn't working right.  Disabled for now.
            if (command == "!MSN")
            {
                if (isAdmin(nick))
                {

                    string[] IncomingMessage;
                    IncomingMessage = commandParam.Split(new char[] { ' ' });

                    if (IncomingMessage.Length >= 2)
                    {
                        string msnUserName = IncomingMessage[1];
                        string msnPassWord = IncomingMessage[2];

                        // start a new MSN client
                       msnClient = new msn(msnUserName, msnPassWord);
                    }

                    else
                        sendToChannel("4Error in command.", location);

                }
                else
                    sendToCha
             * nnel("4You do not have access to this command.", location);
            }*/

#endregion

        }


#endregion


#region isAdmin(user)
        private bool isAdmin(string user)
        {
            ArrayList adminList = new ArrayList();

            // Check for the admin and return true if the user matches, false if not.
            XmlTextReader configReader = new XmlTextReader(configFile);
            configReader.WhitespaceHandling = WhitespaceHandling.None;
            configReader.Namespaces = false;

            try
            {
                while (configReader.Read())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.LocalName.Equals("admin"))
                            adminList.Add(configReader.ReadString());
                    }
                }
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error found: " + e.ToString() + "\n");
            }

            configReader.Close();

            // Now look through the arrayList
            foreach (string name in adminList)
            {
                if (name.ToString().ToUpper() == user.ToUpper())
                    return true;
            }
            return false;
        }
#endregion


#region isUser(user)
        private bool isUser(string user)
        {
            ArrayList userList = new ArrayList();

            // Check for the admin and return true if the user matches, false if not.
            XmlTextReader configReader = new XmlTextReader(configFile);
            configReader.WhitespaceHandling = WhitespaceHandling.None;
            configReader.Namespaces = false;

            try
            {
                while (configReader.Read())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.LocalName.Equals("user"))
                            userList.Add(configReader.ReadString());
                    }
                }
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error found: " + e.ToString() + "\n");
            }

            configReader.Close();

            // Now look through the arrayList
            foreach (string name in userList)
            {
                if (name.ToString().ToUpper() == user.ToUpper())
                    return true;
            }
                return false;

        }
#endregion


#region addUser(user)
        private void addUser(string user)
        {
            // Make an XmlDocument that contains the original XML.
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);
            
            // What we want to insert
            string toInsert = @"<user>" + user + "</user>";

            // Create a document fragment to contain the XML to be inserted.
            XmlDocumentFragment docFrag = doc.CreateDocumentFragment();

            // Set the contents of the document fragment.
            docFrag.InnerXml = toInsert;

            // Add the children of the document fragment to the
            // original document.
            doc.DocumentElement.AppendChild(docFrag);

            // Save the new file.
            doc.Save(configFile);
        }
#endregion


#region remUser(user)
private void remUser(string user)
        {
            // Create an array list that holds the entire contents of the config file.
            if (File.Exists(configFile))
            {
                ArrayList lines = new ArrayList();

                using (StreamReader cFile = File.OpenText(configFile))
                {
                    string line;

                    while ((line = cFile.ReadLine()) != null)
                        lines.Add(line);
                }

                // Erase the old list.
                File.Delete(configFile);

                // Create a new one, writing all the lines in the array except for the one that's being removed.
                string toDelete = @"<user>" + user + "</user>";

                foreach (string line in lines)
                {
                    if (line.Trim() != toDelete)
                    {
                        using (StreamWriter cFile = File.AppendText(configFile))
                        {
                            cFile.WriteLine(line);
                        }
                    }
                }
                
            }
            
        }
#endregion


    }

  
}

