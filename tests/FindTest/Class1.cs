using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestProject3
{
    // Utilities to make JUnit act a little more like Go's "testing" package.
    public class GoTestUtils
    {
        // Other utilities:

        public static int len(Object[] array)
        {
            return array == null ? 0 : array.Length;
        }

        public static int len(int[] array)
        {
            return array == null ? 0 : array.Length;
        }

        public static int len(byte[] array)
        {
            return array == null ? 0 : array.Length;
        }

        public static byte[] utf8(String s)
        {
            try
            {
                var r2 = Encoding.UTF8.GetBytes(s);
                return r2;
            }
            catch (Exception e)
            {
                throw new Exception("can't happen");
            }
        }

        // Beware: logically this operation can fail, but Java doesn't detect it.
        public static String fromUTF8(byte[] b)
        {
            try
            {
                return System.Text.Encoding.UTF8.GetString(b);
            }
            catch (Exception e)
            {
                throw new Exception("can't happen");
            }
        }

        // Convert |idx16|, which are Java (UTF-16) string indices, into the
        // corresponding indices in the UTF-8 encoding of |text|.
        //
        // TODO(adonovan): eliminate duplication w.r.t. ExecTest.
        public static int[] utf16IndicesToUtf8(int[] idx16, String text)
        {
            try
            {
                int[] idx8 = new int[idx16.Length];
                for (int i = 0; i < idx16.Length; ++i)
                {
                    idx8[i] =
                        idx16[i] == -1 ? -1 : Encoding.UTF8.GetBytes(text.Substring(0, idx16[i] - 0)).Length; // yikes
                }

                return idx8;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}