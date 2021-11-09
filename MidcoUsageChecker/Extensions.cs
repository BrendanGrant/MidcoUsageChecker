using System;
using System.Collections.Generic;
using System.Text;

namespace MidcoUsageChecker
{
    internal static class Extensions
    {
        //Borrowed from https://codereview.stackexchange.com/questions/149326/nth-index-of-char-in-string
        public static int NthIndexOfC(this string input, char charToFind, int n)
        {
            int position;

            switch (Math.Sign(n))
            {
                case 1:
                    position = -1;
                    do
                    {
                        position = input.IndexOf(charToFind, position + 1);
                        --n;
                    } while (position != -1 && n > 0);
                    break;
                case -1:
                    position = input.Length;
                    do
                    {
                        position = input.LastIndexOf(charToFind, position - 1);
                        ++n;
                    } while (position != -1 && n < 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(message: "param cannot be equal to 0", paramName: nameof(n));
            }

            return position;
        }
    }
}
