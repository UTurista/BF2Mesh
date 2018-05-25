BF2Mesh - Battlefield 2 mesh file parser

This code documents how to parse mesh files of the DICE/EA games Battlefield 2,
Battlefield 2142, Battlefield Heroes, Battlefield Play4Free, which share the
same internal layout. These files typically have the extension ".StaticMesh",
".BundledMesh" or ".SkinnedMesh".

The code does not include rendering code or otherwise convert or output the data
to file. However, the included sample application (main.cpp) is a command-line
program that prints some useful information, and can be used to verify the
integrity of mesh files.

For more information, see: http://www.bytehazard.com


###  
BF2Console used for testing loading from levels
1. Set PR_REPO for modpath in sysenv variables 
2. change LEVEL in Program 

TODO: proper modpath detection, sysargs, omit missing objects, improve conparser, use .tweaks, gui for users, and many more that probably will never get done...