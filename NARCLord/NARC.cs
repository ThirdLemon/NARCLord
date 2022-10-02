using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Diagnostics;
using Syroot.BinaryData;

namespace NARCLord
{
    /**
     * <summary>A class which holds all the data of a .narc file.</summary>
     */
    public class NARC : IEnumerable<byte[]>
    {
        private byte[][] _data;

       

        /**
         * <summary>
         *      Generate an empty narc
         * </summary>
         */
        public NARC()
        {
            _data = new byte[][] { null };
        }

        public static NARC BuildFromFile(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                using (BinaryStream bStream = new BinaryStream(stream))
                {
                    string filePrefix = bStream.ReadString(4);

                    if (filePrefix != "NARC")
                        throw new InvalidDataException("File given is not a narc file.");

                    //Next four bytes should be FE FF 01 00
                    bStream.Position += 4;
                    //After that, the length of the file is encoded
                    uint fileSize = bStream.ReadUInt32();
                    //make sure the file size is accurate

                    if (fileSize != bStream.Length)
                        throw new InvalidDataException("File size does not match with value encoded at 0x08.");

                    //After that, should always be 10 00 03 00
                    bStream.Position += 4;
                    //This ends initial header. check that the next text is "BTAF"
                    string textBTAF = bStream.ReadString(4);

                    if (textBTAF != "BTAF")
                        throw new InvalidDataException("File given does not have correct subheader.");

                    uint btafSize = bStream.ReadUInt32();
                    uint fileCount = bStream.ReadUInt32();

                    uint[] fileStartLocs = new uint[fileCount];
                    uint[] fileEndLocs = new uint[fileCount];

                    //This shouldn't move the position, but just in case, make sure we're at the start of the seecond header.
                    bStream.Position = 0x10 + btafSize;

                    string textBTNF = bStream.ReadString(4);
                    if (textBTNF != "BTNF")
                        throw new InvalidDataException("File given does not have correct secondary header.");

                    //Next four bytes should be 10 00 00 00. the 10 represents the size of the second header, which is always 16.

                    //Next four bytes should be 04 00 00 00

                    //Next four bytes should be 00 00 01 00.
                    bStream.Position += 12;
                    //End secondary header. check that next text is "GMIF", to make sure that the secondary subheader is good
                    string textGMIF = bStream.ReadString(4);

                    if (textGMIF != "GMIF")
                        throw new InvalidDataException("File given does not have correct secondary subheader.");

                    uint gmifSize = bStream.ReadUInt32();

                    if (gmifSize + btafSize + 0x20 != fileSize)
                        throw new InvalidDataException("File does not have correctly encoded subsizes.");

                    //0x28 accounts for both headers and then the combination of "GMIF" and the encoded size of the GMIF
                    uint fileSpaceStart = btafSize + 0x28;

                    //now go get the size of the data
                    bStream.Position = 0x1C;
                    for(uint fileNum = 0; fileNum < fileCount; fileNum++)
                    {
                        fileStartLocs[fileNum] = bStream.ReadUInt32();
                        fileEndLocs[fileNum] = bStream.ReadUInt32();
                    }

                    //now go get the data
                    NARC to_return = new NARC();
                    to_return._data = new byte[fileCount][];

                    for(uint fileNum = 0; fileNum < fileCount; fileNum++)
                    {
                        bStream.Position = fileSpaceStart + fileStartLocs[fileNum];
                        to_return._data[fileNum] = bStream.ReadBytes((int)(fileEndLocs[fileNum] - fileStartLocs[fileNum]));
                    }

                    return to_return;
                }
            }
        }

        public byte[] this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        public int Length
        {
            get { return _data.Length; }
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return (IEnumerator<byte[]>)_data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
