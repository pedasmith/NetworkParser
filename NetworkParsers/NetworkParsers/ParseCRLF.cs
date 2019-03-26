using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkParsers
{
    public static class ParseCRLF
    {
        /// <summary>
        /// Results from splitting a byte array into lines. 
        /// </summary>
        public struct SplitState
        {
            public SplitState(IList<byte[]> lines = null)
            {
                Lines = lines;
                LastLinePartial = false;
                lastWasUnusedCRorLF = false;
                lastChar = 0; // requirement is that it's not \r or \n
            }

            /// <summary>
            /// List of lines
            /// </summary>
            public IList<byte[]> Lines;

            /// <summary>
            /// Set to true if the last line was properly ended with an end of line or false if not. This can be used when dealing with a series of buffers which are not neatly ended on an end of line.
            /// </summary>
            public bool LastLinePartial;

            /// <summary>
            /// Part of the state variables. 
            /// </summary>
            public bool lastWasUnusedCRorLF;
            public byte lastChar; 


            /// <summary>
            /// Adds in a line of data given a buffer and inclusive start and end indexes
            /// </summary>
            /// <param name="rawData">raw data to extract a line from</param>
            /// <param name="startIndex">starting index; must be within the rawData range</param>
            /// <param name="endIndex">ending index (inclusive); must be >= startIndex</param>
            public void AddLine(IList<byte> rawData, int startIndex, int endIndex)
            {
                int length = endIndex - startIndex + 1;
                var line = new byte[length];
                for (int i=0; i<length; i++)
                {
                    line[i] = rawData[i+startIndex];
                }
                Lines.Add (line);
            }

            /// <summary>
            /// Appends this new line with the last previous line
            /// </summary>
            /// <param name="rawData">raw data to extract a line from</param>
            /// <param name="startIndex">starting index; must be within the rawData range</param>
            /// <param name="endIndex">ending index (inclusive); must be >= startIndex</param>
            public void AppendLine(IList<byte> rawData, int startIndex, int endIndex)
            {
                if (Lines.Count == 0)
                {
                    AddLine(rawData, startIndex, endIndex);
                    return;
                }

                int length = endIndex - startIndex + 1;
                var prevLine = Lines[Lines.Count-1];
                int prevLength = prevLine.Length;
                var line = new byte[length + prevLength];
                for (int i = 0; i < prevLength; i++)
                {
                    line[i] = prevLine[i];
                }
                for (int i = 0; i < length; i++)
                {
                    line[i+prevLength] = rawData[i + startIndex];
                }
                Lines[Lines.Count-1] = line;
            }

        }

        /// <summary>
        /// Splits a string of bytes into lines. Each line is seperated by CR, LF, CR-LF or LF-CR, but CR-CR is considered two lines.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="prevResults"></param>
        /// <returns></returns>
        public static SplitState SplitCRLF(IList<byte> input, SplitState state)
        {
            int nbytes = input.Count();

            if(state.Lines == null)
            {
                state.Lines = new List<byte[]>();
            }

            int start = 0;
            int nline = 0;
            bool appendNextLine = state.LastLinePartial;

            for (int i=0; i<nbytes; i++)
            {
                var currChar = input[i];
                var currIsCRorLF = currChar == (byte)'\r' || currChar == (byte)'\n';


                if (state.lastWasUnusedCRorLF)
                {
                    // When I see CR-CR or LF-LF then it's two lines
                    // Handle the case of CR-CR-LF which is two lines
                    if (currChar == state.lastChar)
                    {
                        if (appendNextLine)
                        {
                            state.AppendLine(input, start, start - 1);
                            appendNextLine = false;
                        }
                        else
                        {
                            state.AddLine(input, start, start - 1);
                        }
                        nline++;
                        state.lastWasUnusedCRorLF = true;
                    }
                    else if (currIsCRorLF) // e.g. CR LF or LF CR
                    {
                        // All of the CR and LF are used up. Next 
                        // CR or LF must be a new line.
                        state.lastWasUnusedCRorLF = false;
                        start = i+1;
                    }
                    else // Is e.g. CR Q; the Q is the start of the next line.
                    {
                        start = i;
                        state.lastWasUnusedCRorLF = false;
                    }
                }
                else if (currIsCRorLF)
                {
                    if (appendNextLine)
                    {
                        state.AppendLine(input, start, i - 1);
                        appendNextLine = false;
                    }
                    else
                    {
                        state.AddLine(input, start, i - 1); // line is zero bytes long
                    }
                    nline++;
                    state.lastWasUnusedCRorLF = true;
                }
                else
                {
                    state.lastWasUnusedCRorLF = false;
                }

                state.lastChar = currChar;
            }

            if (state.lastWasUnusedCRorLF)
            {
                state.LastLinePartial = false;
            }
            else if (start >= input.Count)
            {
                // Ended exactly gracefully with CR LF or LF CR
                // Where there's a CR LF (or LF CR), the lastWasUnusedCRorLF is false because the last char LF (or CR) is used up.
                state.LastLinePartial = false;
            }
            else
            {
                // Have a bit more to add
                if (appendNextLine)
                {
                    state.AppendLine(input, start, input.Count - 1);
                    appendNextLine = false;
                }
                else
                {
                    state.AddLine(input, start, input.Count - 1); 
                }
                nline++;
                state.LastLinePartial = true;
            }
            return state;
        }
    }
}
