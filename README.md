PythonClient
============

PythonClient is a test solution for binding AutoCAD .NET automation with a Python sockets server.

We use ZeroMQ to pass messages between .NET client and Python information modeling server.

This is a further development of Shadowbinder.py ecosystem, an experiment in opensource structural model processing for the AEC industry.

For questions and issues use the GitHub issue tracker or contact me via bauskas@gmail.com

IMPORTANT NOTICE: this is not a release and not even an alpha version. The project is in early stages of development.


2014-07-01 Update:
=========

Work is under way to design a better protocol architecture for PythonClient.

These are the two use cases for AutoCADClient->PythonServer->AutoCADClient interaction:

1. Swap identities of two AutoCAD entities http://through-the-interface.typepad.com/through_the_interface/2008/07/swapping-identi.html

2. Advanced block manipulation - moving text inside an AutoCAD block http://through-the-interface.typepad.com/through_the_interface/2013/12/moving-text-in-an-autocad-block-using-net-part-1.html

3. Embedding a map image in an AutoCAD drawing using .NET: http://through-the-interface.typepad.com/through_the_interface/2014/07/embedding-a-map-image-in-an-autocad-drawing-using-net.html
