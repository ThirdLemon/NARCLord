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
        private List<Byte[]> _data;

       

        /**
         * <summary>
         *      Generate an empty narc
         * </summary>
         */
        public NARC()
        {
            _data = new List<byte[]> ();
        }

        public static NARC Build(string file)
        {
            return Build(new FileStream(file, FileMode.Open));
        }

        public static NARC Build(Stream stream)
        {
            using (BinaryStream bStream = new BinaryStream(stream))
            {
                string filePrefix = bStream.ReadString(4);

                if (filePrefix != "NARC")
                    throw new InvalidDataException("File given is not a narc file.");

                //Next four bytes should be FE FF 00 01
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
                for (uint fileNum = 0; fileNum < fileCount; fileNum++)
                {
                    fileStartLocs[fileNum] = bStream.ReadUInt32();
                    fileEndLocs[fileNum] = bStream.ReadUInt32();
                }

                //now go get the data
                NARC to_return = new NARC();

                for (uint fileNum = 0; fileNum < fileCount; fileNum++)
                {
                    bStream.Position = fileSpaceStart + fileStartLocs[fileNum];
                    to_return._data.Add(bStream.ReadBytes((int)(fileEndLocs[fileNum] - fileStartLocs[fileNum])));
                }

                return to_return;
            }
        }

        public byte[] Compile()
        {
            //get file packed order
            int[] fileStartLocs = new int[Length];
            int[] fileEndLocs = new int[Length];

            for(int fileNum = 0; fileNum < Length; fileNum++)
            {
                if (fileNum == 0)
                    fileStartLocs[fileNum] = 0;
                else
                    //Round up to nearest multiple of four
                    fileStartLocs[fileNum] = ((fileEndLocs[fileNum - 1] + 3) / 4) * 4;

                fileEndLocs[fileNum] = fileStartLocs[fileNum] + _data[fileNum].Length;
            }

            //get start of gmif section in total file
            //1C is initial header+subheader, 8*length is total btaf section, 18 is second header+subheader
            int gmifStart = 0x1C + (8 * Length) + 0x18;

            //get total length of gmif section, rounded to the nearest multiple of four
            int gmifEnd = ((fileEndLocs[Length - 1] + 3) / 4) * 4;

            //total file size. 1C is the first header and subheader size, then the entire encoding of file lengths, then the secondary header and its subheader, then the length of the file
            int fileSize = 0x1C + (8 * Length) + 0x18 + gmifEnd;

            byte[] to_return = new byte[fileSize];

            using (MemoryStream stream = new MemoryStream(to_return))
            {
                using (BinaryStream bStream = new BinaryStream(stream, stringCoding: StringCoding.Raw))
                {
                    //Write initial boilerplate
                    bStream.WriteString("NARC");
                    bStream.WriteBytes(new byte[] { 0xFE, 0xFF, 0x00, 0x01});
                    //Write filesize
                    bStream.WriteUInt32((uint)fileSize);
                    //more boilerplate
                    bStream.WriteBytes(new byte[] { 0x10, 0x00, 0x03, 0x00 });
                    bStream.WriteString("BTAF");
                    //write btaf size
                    bStream.WriteUInt32(0xC + (0x8 * (uint)Length));
                    //write file count
                    bStream.WriteUInt32((uint)Length);
                    //Write file locs
                    for(int fileNum = 0; fileNum < Length; fileNum++)
                    {
                        bStream.WriteUInt32((uint)fileStartLocs[fileNum]);
                        bStream.WriteUInt32((uint)fileEndLocs[fileNum]);
                    }
                    //write second header boilerplate
                    bStream.WriteString("BTNF");
                    bStream.WriteBytes(new byte[] { 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00 });
                    bStream.WriteString("GMIF");
                    //WRite gmif size
                    bStream.WriteUInt32((uint)gmifEnd + 8);
                    //write every file
                    for (int fileNum = 0; fileNum < Length; fileNum++)
                    {
                        bStream.Position = gmifStart + fileStartLocs[fileNum];
                        bStream.WriteBytes(_data[fileNum]);
                    }
                }
            }

            return to_return;
        }

        public byte[] this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        public int Length
        {
            get { return _data.Count; }
        }

        public void Add(byte[] item)
        {
            _data.Add(item);
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
