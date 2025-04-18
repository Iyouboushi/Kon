# Kon version 1.6.0

"Kon" is my attempt at creating a "chatterbot" written in C#. This bot will 
connect to an IRC server and join a channel whereupon it will "watch" the 
conversations. It will then try to "learn" from them so that people can hold 
conversations with it.

It has several different "brains" to learn/use in conversations. It has a 
basic brain that is simply a "monkey-see, monkey-do." This brain will repeat 
something someone has said earlier in full, without manipulating it. And it
has a "Language Bot" brain which is based on a chatterbot called "Language Bot" 
(written by BishounenNightBird of Esper.net). This brain attempts to take 
what people says and manipulate it so it can create semi-original thoughts. 
This brain is currently set to default.

# Setup
Setup is simple. Clone or download the kon.exe, LB Brain Utility.exe
kon.brain, lb.brain, config.xml, and throwitems.txt files and put them
into a folder.  Then edit the config file.  

Open config.xml in notepad or whatever editor you would like to use,
and change the values for your IRC server, port, the bot's nick, the
default channel you would like the bot to join, and what value you
would like to set "Random Talk" (the bot's ability to speak randomly
without being addressed).  Finally you will want to set your nick as
the admin.

After you have finished editing the config file, just double click 
kon.exe and it will automatically join the server and channel you
have set.

To address the bot, use

Kon: text goes here

OR

YourBot'sName: text goes here


** Note: although the bot comes with a small LB Brain filled with some
Internet memes and other sayings, until the bot views some conversations 
it will still have very little to say.

# Commands
There are several commands in the bot that can be used.

CONSOLE COMMANDS
These are the commands that can only be used by the owner of the bot in 
the bot's console window.

/QUIT
Causes the bot to quit the IRC server.

/JOIN #channel
Will cause the bot to join the #channel specified.

/PART [#channel]
By itself, /PART will cause the bot to quit the DEFAULT channel that's set in the config.xml. 
With a channel specified, the bot will try to leave the channel.

/SAY [#channel] message
By itself, /SAY message will speak to the DEFAULT channel that's set in the config.xml.
If you pick a channel, you can have the bot talk to more than one channel that it is sitting in.

/MSG person message
This command will let you message non-channels (i.e. users and services).

/RECONNECT
This should let the bot quit the IRC server and reconnect to it.

/RAW
This command will send RAW commands to the IRC server.  For those out there that
know how to use IRC commands via RAW that haven't been added yet to the bot, this
is how you do them.

/CLEAR
Will clear the console window.

/DINFO
Will display some information about the bot's current status.

/TWITTERINFO
Will display information about the built-in twitter client including user name/password and whether or not
the client is currently enabled/running.

/TOGGLEPONG
Will toggle the display of the ping/pong messages. Useful for debugging in some cases.



CHANNEL COMMANDS
These are the commands that are done in an IRC channel by users.  There are three
user levels that can be defined in config.xml, each allowing multiple commands. 
Note that ADMIN level has access to all commands, USER level has access to user and
everyone commands.


* ADMIN LEVEL COMMANDS

!addUser nick
Will add the nick specified to the user-level list.

!remUser (nick)
Will remove the nick specified from the user-level list.

!quit
Will cause the bot to quit. Same as the console command.

!reconnect
Will cause the bot to reconnect to the server. 

!msg #chan/nick message
Allows you to send a message via command.

!raw RAW IRC COMMAND STRUCTURE
Allows you to send a raw IRC command


* USER LEVEL COMMANDS

!toggleLB
Will turn on the Language Bot ("LB") Brain on/off.  Turning this brain on will
turn the others off.

!channelUsers
Will display the users in the channel the command is used.  May not always be
accurate.

!utter
Will cause the bot to utter a random line.

!toggleTopic
Will toggle the bot's ability to try and locate the "topic" of what people
say to it. 

!randomTalk [#] 
By itself, !randomTalk will tell the channel what setting the option has been set
to.  If the user specifies a number, the setting will be changed.  Setting it to 0
will turn it off.  100 will make the bot respond to every line in the channel.

!join #channel
Will cause the bot to join the channel specified.  Same as the console command.

!part #channel      
Will cause the bot to leave the channel specified.      

!toggleDoubleSentences
Will toggle the bot's ability to string two sentences together if the first reply
is too short.  Recommended to leave this off until the bot's LB brain is big
enough to handle it, or else you'll get repeating lines back to back.

!Dinfo
Will show a few tidbits of internal info (reconnect attempt/total, if ping 
control is set to true, and the # of ping attempts so far).   Really only 
useful for myself, but you might find it interesting.


* EVERYONE LEVEL COMMANDS

!splash
Will cause the bot to utter its namesake's catch phrase from Bleach.

!konInfo
Will display the current version number, along with my email.

!brainInfo
Will display the number of lines in its basic brain.

!LBinfo
Will display the number of lines in the LB Brain.  LB Brain must be on in
order to use this command.

!haiku
Will cause the bot to attempt to create a haiku using the 5/7/5 syllable structure.
It's not perfect and the counts will often be a little off but it's a start. 
Needs the LB Brain to be on in order to work properly.

!throw [line number] [Target]
Will pick a random item from throwitems.txt to "throw" at a random channel user. 
Can specify the line number if you want to throw a specific item and/or a target to 
pick a specific person.

!roll xDy  or !dice xDy
Will return a dice roll.  Use it like !roll 1d20 or !dice 2d4