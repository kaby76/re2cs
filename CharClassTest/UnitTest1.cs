using System;
using System.Linq;
using Xunit;
using engine;

namespace CharClassTest
{
    public class CharClassTest
    {
        private static CharClass cc(params int[] x)
        {
            return new CharClass(x);
        }

        private static int[] i(params int[] x)
        {
            return x;
        }

        private static int[] s(string s)
        {
            return Utils.stringToRunes(s);
        }

        private static void assertClass(CharClass cc, params int[] expected)
        {
            int[] actual = cc.toArray();
            if (!actual.SequenceEqual(expected))
            {
                throw new Exception(
                    "Incorrect CharClass value:\n"
                    + "Expected: "
                    + expected.ToString()
                    + "\n"
                    + "Actual:   "
                    + actual.ToString());
            }
        }

        [Fact]
        public void testCleanClass()
        {
            assertClass(cc().cleanClass());

            assertClass(cc(10, 20, 10, 20, 10, 20).cleanClass(), 10, 20);

            assertClass(cc(10, 20).cleanClass(), 10, 20);

            assertClass(cc(10, 20, 20, 30).cleanClass(), 10, 30);

            assertClass(cc(10, 20, 30, 40, 20, 30).cleanClass(), 10, 40);

            assertClass(cc(0, 50, 20, 30).cleanClass(), 0, 50);

            assertClass(
                cc(10, 11, 13, 14, 16, 17, 19, 20, 22, 23).cleanClass(),
                10,
                11,
                13,
                14,
                16,
                17,
                19,
                20,
                22,
                23);

            assertClass(
                cc(13, 14, 10, 11, 22, 23, 19, 20, 16, 17).cleanClass(),
                10,
                11,
                13,
                14,
                16,
                17,
                19,
                20,
                22,
                23);

            assertClass(
                cc(13, 14, 10, 11, 22, 23, 19, 20, 16, 17).cleanClass(),
                10,
                11,
                13,
                14,
                16,
                17,
                19,
                20,
                22,
                23);

            assertClass(cc(13, 14, 10, 11, 22, 23, 19, 20, 16, 17, 5, 25).cleanClass(), 5, 25);

            assertClass(cc(13, 14, 10, 11, 22, 23, 19, 20, 16, 17, 12, 21).cleanClass(), 10, 23);

            assertClass(cc(0, Unicode.MAX_RUNE).cleanClass(), 0, Unicode.MAX_RUNE);

            assertClass(cc(0, 50).cleanClass(), 0, 50);

            assertClass(cc(50, Unicode.MAX_RUNE).cleanClass(), 50, Unicode.MAX_RUNE);
        }

        [Fact]
        public void testAppendLiteral()
        {
            assertClass(cc().appendLiteral('a', 0), 'a', 'a');
            assertClass(cc('a', 'f').appendLiteral('a', 0), 'a', 'f');
            assertClass(cc('b', 'f').appendLiteral('a', 0), 'a', 'f');
            assertClass(cc('a', 'f').appendLiteral('g', 0), 'a', 'g');
            assertClass(cc('a', 'f').appendLiteral('A', 0), 'a', 'f', 'A', 'A');

            assertClass(cc().appendLiteral('A', RE2.FOLD_CASE), 'A', 'A', 'a', 'a');
            assertClass(cc('a', 'f').appendLiteral('a', RE2.FOLD_CASE), 'a', 'f', 'A', 'A');
            assertClass(cc('b', 'f').appendLiteral('a', RE2.FOLD_CASE), 'a', 'f', 'A', 'A');
            assertClass(cc('a', 'f').appendLiteral('g', RE2.FOLD_CASE), 'a', 'g', 'G', 'G');
            assertClass(cc('a', 'f').appendLiteral('A', RE2.FOLD_CASE), 'a', 'f', 'A', 'A');

            // ' ' is beneath the MIN-MAX_FOLD range.
            assertClass(cc('a', 'f').appendLiteral(' ', 0), 'a', 'f', ' ', ' ');
            assertClass(cc('a', 'f').appendLiteral(' ', RE2.FOLD_CASE), 'a', 'f', ' ', ' ');
        }

        [Fact]
        public void testAppendFoldedRange()
        {
            // These cases are derived directly from the program logic:

            // Range is full: folding can't add more.
            assertClass(cc().appendFoldedRange(10, 0x10ff0), 10, 0x10ff0);

            // Range is outside folding possibilities.
            assertClass(cc().appendFoldedRange(' ', '&'), ' ', '&');

            // [lo, MIN_FOLD - 1] needs no folding.  Only [...abc] suffix is folded.
            assertClass(cc().appendFoldedRange(' ', 'C'), ' ', 'C', 'a', 'c');

            // [MAX_FOLD...] needs no folding
            // DOES NOT WORK IN C# TOLOWER DOES NOT WORK THE SAME.
            //assertClass(
            //    cc().appendFoldedRange(0x10400, 0x104f0),
            //    0x10450,
            //    0x104f0,
            //    0x10400,
            //    0x10426, // lowercase Deseret
            //    0x10426,
            //    0x1044f); // uppercase Deseret, abutting.
        }

        [Fact]
        public void testAppendClass()
        {
            assertClass(cc().appendClass(i('a', 'z')), 'a', 'z');
            assertClass(cc('a', 'f').appendClass(i('c', 't')), 'a', 't');
            assertClass(cc('c', 't').appendClass(i('a', 'f')), 'a', 't');

            assertClass(
                cc('d', 'e').appendNegatedClass(i('b', 'f')), 'd', 'e', 0, 'a', 'g', Unicode.MAX_RUNE);
        }

        [Fact]
        public void testAppendFoldedClass()
        {
            // NB, local variable names use Unicode.
            // 0x17F is an old English long s (looks like an f) and folds to s.
            // 0x212A is the Kelvin symbol and folds to k.
            char ſ = (char)0x17F, K = (char)0x212A;

            assertClass(cc().appendFoldedClass(i('a', 'z')), s("akAK" + K + K + "lsLS" + ſ + ſ + "tzTZ"));

            assertClass(
                cc('a', 'f').appendFoldedClass(i('c', 't')), s("akCK" + K + K + "lsLS" + ſ + ſ + "ttTT"));

            assertClass(cc('c', 't').appendFoldedClass(i('a', 'f')), 'c', 't', 'a', 'f', 'A', 'F');
        }

        [Fact]
        public void testNegateClass()
        {
            assertClass(cc().negateClass(), '\0', Unicode.MAX_RUNE);
            assertClass(cc('A', 'Z').negateClass(), '\0', '@', '[', Unicode.MAX_RUNE);
            assertClass(cc('A', 'Z', 'a', 'z').negateClass(), '\0', '@', '[', '`', '{', Unicode.MAX_RUNE);
        }

        [Fact]
        public void testAppendTable()
        {
            assertClass(
                cc().appendTable(new int[][] {i('a', 'z', 1), i('A', 'M', 4)}),
                'a',
                'z',
                'A',
                'A',
                'E',
                'E',
                'I',
                'I',
                'M',
                'M');
            assertClass(
                cc().appendTable(new int[][] {i('Ā', 'Į', 2)}),
                s("ĀĀĂĂĄĄĆĆĈĈĊĊČČĎĎĐĐĒĒĔĔĖĖĘĘĚĚĜĜĞĞĠĠĢĢĤĤĦĦĨĨĪĪĬĬĮĮ"));
            assertClass(
                cc().appendTable(new int[][] {i('Ā' + 1, 'Į' + 1, 2)}),
                s("āāăăąąććĉĉċċččďďđđēēĕĕėėęęěěĝĝğğġġģģĥĥħħĩĩīīĭĭįį"));

            assertClass(
                cc().appendNegatedTable(new int[][] {i('b', 'f', 1)}), 0, 'a', 'g', Unicode.MAX_RUNE);
        }

        [Fact]
        public void testAppendGroup()
        {
            assertClass(cc().appendGroup(CharGroup.PERL_GROUPS["\\d"], false), '0', '9');
            assertClass(
                cc().appendGroup(CharGroup.PERL_GROUPS["\\D"], false), 0, '/', ':', Unicode.MAX_RUNE);
        }

        [Fact]
        public void testToString()
        {
            Assert.Equal("[0xa 0xc-0x14]", cc(10, 10, 12, 20).ToString());
        }
    }
}
