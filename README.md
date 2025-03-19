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