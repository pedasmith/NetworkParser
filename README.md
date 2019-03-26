# NetworkParser
C# Classes to handle text-based parsing for protocols like Gopher and Telnet 

## NetworkParsers.ParseCRLF.SplitCRLF (byte[] data, SplitState state)

Splits the given data into lines seperated by CRLF. Because many actual servers incorrectly use LF or CR, 
will also split on those characters by themselves. 

## NetworkParsers.UTf8.IsUtf8 (byte[] data, int length-1)

Returns True if the input is valid UTF8. Has the same function signature as a 
[popular C# class](https://archive.codeplex.com/?p=utf8checker) so that it can be 
a drop-in replacement. The original utf8checker.IsUtf8 has a couple of problems --
it's Gnu GPL, which I don't want to use for my own code, and it's got a bug where 
utf8 sections at the end of a byte array will be flagged as incorrect. It also 
hardly has any tests, and it doesn't compare its output against the Window UWP 
UTF8 conversion routines.

### Features of my IsUtf8 method

My IsUtf8 method has a couple of goals that some other checkers don't share.
1. My version is "robust" when given sub-optimal UTF8. For example, a NUl char
should be present as just a plain NUL char in UTF8 since that's the shortest possible
version. But some people prefer to encode it using 
[Modified UTF8](https://en.wikipedia.org/wiki/UTF-8#Modified_UTF-8) as **C0 80**, a two-byte 
overlong sequence that when converted into unicode chars will be a NUL.

2. My version includes tests to compare its results against the Microsoft decoders. 
The next step afer determining that a byte array is UTF8 is presumably to convert
it to a string using one of the three main converters used in UWP C# programs. 
I never want to have my IsUtf8 say that a byte array **is** UTF8 only to have the
conversion throw an exception.

3. My version includes unit tests include testing randomly generated UTF8 and invalid
UTF8 sequences. This give more assurances that the code is correct.
