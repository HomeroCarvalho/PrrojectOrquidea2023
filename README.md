# ProjectOrquidea2023

This is a holistic study project on programming languages. Contains a Prolog interpreter, a LISP interpreter,
an OOP language (similar syntax as C,C++,C sharp,Java), OOP (Aspects), and functional programming.
In addition to the programming language: an improved switch, data object wrappers, full implementation of operators (
if Data Oriented Programming comes, we will be up to date).
The project switch: [casesOsUse], is the result of frustrating experiences with the switch of many programming languages,
allows conditional operators, not just equals, plus greater, smaller, equal, greater than equal, etc... and expressions as cases, not just constant values.
Operators come from other frustrating experiences, many programming languages ​​extend operators
arithmetic, conditionals, but not custom operators such as [dot], [?]. This covers a gap and prepares
the groundwork for Data-Driven Programming, if it ever comes.

In the next updates, structured programming commands will be described, but you can check them with the HTML documentation.

HOW TO RUN A PROGRAM IN ORQUIDEA LANGUAGE:

Clone the project, and compile. Don't forget to install NUGET-packages: Math Numerics, Math .NET Spatial.
In the bin/debug or bin/release directory of the compiled project, place the ORQUIDEA.xml file.
To run the program, code the code: ParserAFile.ExecuteProgram(nomeDoArquivoDoPrograma), and see the result.
A directory of example programs will be made available in the next update.

LIBRARIES:
In the Orquidea Project archive, there is;
1- string manipulation library.
2- Math library, in the double class.
3- Prompt library (for writing/reading in the Terminal).
Externally:
1- file manipulation library: Archive.
2- library of playing sounds/music (songs contained in a game loop).

It's extremely easy to create a library:
1- build a project in Visual Studio 2022, and define the project type as: dll library, in .NET 7 (or .NET 6, also works) Framework;
2- compile.
3- place the compiled .dll in the [libs] folder, in the root of the compiled orquidea 2023 project.
In the code, enter the command, for example: Library "Sounds" {Sound}, where Sounds is the name of the .dll library file, and Sound is the library class. Ready.
Call the function: ParserAFile.ExecuteProgram(ProgramFilename), if you want to check if the library is working.
----> don't forget to put any .dll C++ libraries that implement a sharp C library in the compiled orquidea 2023 project folder
as a C++ code wrapper.
libraries can "industrially" scale library extensions, starting from the base language, C Sharp.


WRAPPER DATA OBJECTS:
After the 2nd. project maintenance, it was realized that inserting new types of data structures in programming instructions,
It is difficult, increasing the code for each instruction, and practically impossible to compose with the specifics of each new data structure.
BUT, if the data structures are method calls, the extra code is to call the relevant function, e.g.
M[1,5]= M.GetElement(1,5). The notation M[1,5] is a WRAPPER Data Object, and M.GetElement(1,5) is the method call that executes the code to get an M[1,5] element.

Wrapper Data Object Types Implemented:
1- Vector (a vector, and list).
2- Matrix (double).
3- JaggedArray.
4- DictionaryText (keey map;string--> value:string): name["surname"]="trivelato", e.g.

New types of Data Wrappers can be created by implementing the [WrapperData] abstract class. the code works from method calls..



Go to action:
To run an orquidea program via the project terminal,
go to the ProgramasTestes folder, and run the terminal:
./terminalOrquidea.exe HelloWorld.txt
this terminal line will run the HelloWorld program.

There is no output in the console for other programs, except the programLeNome.txt,
which asks for a name to be typed, and returns a sentence composing the name.

terminalOrquidea.exe is in 1st place. branch, after main, where
There is also the test programs folder.
