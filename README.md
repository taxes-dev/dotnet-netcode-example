# C# .NET Netcode Example

This is an example project using a Unity3D client and a custom server application to show a basic way to handle a game with separated client and server. The server is responsible for handling the state of the game "world" while the client acts as a view into that world and processes input from the player.

I created and tested this on Windows 10, but I'm not using any particular platform-specific code, so I imagine it wouldn't be too difficult to get it working on Mac or Linux if you wanted to.

You will need:

* [Unity3D 5.3.5f1](http://unity3d.com/unity/whats-new/unity-5.3.5)
* [Visual Studio Community 2015](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx) (or other C# compiler that can compile the server solution)

I'm releasing this code into the public domain (see [license](LICENSE.md)), so feel free to use it as the basis for your own work. It's far from complete, but it should give you a springboard for writing your own netcode implementation. I've tried to add as many helpful comments in the code as possible. Also continue reading below for a high-level explanation of how things work.

## example-server

You'll need to compile the server and get it running first. It should just work out of the box. If you need to change the IP/port that it listens on, there's a settings file you can edit in the Example.Server.Console project. This also the startup project which is a console application that acts as the server.

Make sure you restore the NuGet packages first. It only depends on one, which is [protobuf-net](https://www.nuget.org/packages/protobuf-net/).

## example-client

This is the Unity3D client. I'd like to point out that nothing in this example requires you to use Unity3D, it was just an easy to provide a simple 3D space to play around in. You can take the principles learned here and adapt it to other game engines or applications.

Once you have the server running, you should be able to open the example-client project in Unity3D, and then open the "BasicMap" scene. Hit the play button in the editor and you're off!

It's not the most exciting game ever, but you should able to control the knight with your keyboard and if you connected to the server you should be able to see a bunch of slimes spawn and move around. The slimes are the interesting part since their movement updates are being sent from the server and not generated inside the client.

# Architecture

Both the client and the server use a system of message queues to avoid tightly coupling components to each other, so the network code embraces this architecture by sending data back and forth as discrete messages. In particular, the Msg struct (from Example.Server.Messages) forms the basis for both network messages and internal software messages. You'll see the client generate these messages in response to keyboard input, and the player object respond to them by moving the player avatar around. Likewise, when the server wants one of the slimes to move, it generates an positional update message, transmits it to the client, and the client interprets that as moving the slime 3D models around the playing field. There's no requirement to really keep this architecture if you want to do something different, but it would require a bit of surgery to go with a different model.

I use [Google Protocol Buffers](https://developers.google.com/protocol-buffers/) as the serialization format for efficiency. I felt it was important to use something like protocol buffers because a lot of other examples already exist using more weighty formats like JSON. Using a binary format isn't easy to set up or debug, but the gains in efficiency of bandwidth and parsing speed are important for scalability. Take a look at the Framing class in particular if you want to follow how this works.

Communication between the client and server uses a custom binary protocol over TCP. Socket listening is handled by the core server Program class and the ConnectedClient class. On the client side, the ServerConnection class handles the same purpose, and honestly it's the only class on the client that's really relevant to this example. If you want to use something other than Unity3D for your game client, this is the minimum code you'll need to port.

## Updating the Client

For simplicity, the client and server share code in a few assemblies. This is mostly the message formats, serialization/deserialization, and basic game structures. There's no hard requirement to sharing this code, but it makes things a lot easier if the client is written in C# like the server (as is the case with the Unity3D example client).

You can find these assembles in the Assets/Scripts/Imports folder. If you make material updates to the Example.GameStructures or Example.Messages projects in the server, you'll need to copy the new assemblies to the Imports folder in the client.

# It's Incomplete

Just to re-iterate, this project is not a complete solution, but it should get you started if you were wondering how to get custom client/server netcode working.

Here's some things to consider if you want to extend this code:

* As it stands, it actually only allows one client. (You can connect with multiple clients, but the server doesn't understand what to do with multiple players and acts as if they're all the same person.) How would you support multiple players?
* There's no notion of "authentication" -- it just trusts whomever connects to the server port. You could wrap this up in the multiple player design from above.
* It uses TCP for simplicity. If you're concerned about scalability, try re-implementing the network layer with UDP.
* Everything is monolithic in design. For example, the entirety of the server is mostly in the ServerProcess class. Networked games can get fairly complicated, so you should try to separate the larger classes out into smaller, extensible/composable chunks.
* There's no notion on the server of the world geography other than some absolute bounds based on the plane in the Unity3D scene. How would you export your level geography so that the server could utilize it?

That's just the beginning. I left everything open-ended so you could adapt to whatever sort of game you'd like to build.

# Miscellaneous

There are some included sounds, fonts, and models that I got from other sources just to make the client project a bit more interesting. You'll find their licensing information in the asset directories.
