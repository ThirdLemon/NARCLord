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

        private bool _initialized;
        public bool Initialized { get { return _initialized; } }

        /**
         * <summary>
         *      Generate an empty narc
         * </summary>
         */
        public NARC()
        {
            _data = new byte[][] { null };
            _initialized = false;
        }

        public void InitializeWithFile(string file)
        {


            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                using (BinaryStream bStream = new BinaryStream(stream))
                {
                    string filePrefix = bStream.ReadString(4);

                    if (filePrefix != "NARC")
                        throw new InvalidDataException("File given is not a narc file.");

                    //Next four bytes should be FE FF 01 00

                    //After that, the length of the file is encoded

                    //After that, should always be 10 00 03 00
                    bStream.Position += 12;
                    //This ends initial header. check that the next text is "BTAF"
                    Debug.Assert(bStream.ReadString(4) == "BTAF");



                    _initialized = true;
                }

            }
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
