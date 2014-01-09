using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{

    public static class ByteHelper
    {
        public static string ToBase64String(this string text, Encoding encoding)
        {
            byte[] source = encoding.GetBytes(text.ToCharArray());
            int length = source.Length + 4;
            byte[] data = new byte[length];
            byte[] result = new byte[length];
            int offset = length - 4;
            int seed = Generate();
            Random rnd = new Random(seed);
            Write(result, seed);
            rnd.NextBytes(data);
            for (int i = 4; i < length; i++)
            {
                result[i] = (byte)(source[i - 4] ^ data[i - 4]);
            }
            return Convert.ToBase64String(result);
        }

        public static string ToBase64String(this string text)
        {
            return ToBase64String(text, Encoding.UTF8);
        }

        public static string Mask(this string text, Encoding encoding)
        {
            byte[] source = encoding.GetBytes(text.ToCharArray());
            int length = source.Length + 4;
            byte[] data = new byte[length];
            byte[] result = new byte[length];
            int offset = length - 4;
            int seed = Generate();
            Random rnd = new Random(seed);
            Write(result, seed);
            rnd.NextBytes(data);
            for (int i = 4; i < length; i++)
            {
                result[i] = (byte)(source[i - 4] ^ data[i - 4]);
            }
            return Convert.ToBase64String(result);
        }

        public static string Mask(this string text)
        {
            return Mask(text, Encoding.UTF8);
        }

        public static string FromBase64String(this string text, Encoding encoding)
        {
            byte[] source = Convert.FromBase64String(text);
            int length = source.Length - 4;
            byte[] data = new byte[length];
            byte[] result = new byte[length];
            int seed = Read(source);
            Random rnd = new Random(seed);
            rnd.NextBytes(data);
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(data[i] ^ source[i + 4]);
            }
            return encoding.GetString(result);
        }

        public static string FromBase64String(this string text)
        {
            return FromBase64String(text, Encoding.UTF8);
        }

        public static string Unmask(this string text, Encoding encoding)
        {
            byte[] source = Convert.FromBase64String(text);
            int length = source.Length - 4;
            byte[] data = new byte[length];
            byte[] result = new byte[length];
            int seed = Read(source);
            Random rnd = new Random(seed);
            rnd.NextBytes(data);
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(data[i] ^ source[i + 4]);
            }
            return encoding.GetString(result);
        }

        public static string Unmask(this string text)
        {
            return Unmask(text, Encoding.UTF8);
        }

        private static void Write(byte[] result, int seed)
        {
            result[0] = (byte)((seed & 0xff) >> 0);
            result[1] = (byte)((seed & 0xff00) >> 8);
            result[2] = (byte)((seed & 0xff0000) >> 16);
            result[3] = (byte)((seed & 0xff000000) >> 24);
        }

        private static int Read(byte[] source)
        {
            byte b0 = source[0];
            byte b1 = source[1];
            byte b2 = source[2];
            byte b3 = source[3];
            return (b0 << 0) | (b1 << 8) | (b2 << 16) | (b3 << 24);
        }

        public static int Generate()
        {
            Random rnd = new Random();
            int seed = rnd.Next();
            return seed;
        }
    }
    class Program
    {
        static string lyrics =
@"
queen lyrics
""it's a beautiful day""

it's a beautiful day
the sun is shining
i feel good
and no-one's gonna stop me now, oh yeah

it's a beautiful day
i fell good, i fell right
and no-one, no-one's gonna stop me now
mama

sometimes i fell so sad, so sad, so bad
but no-one's gonna stop me now, no-one
it's hopeless - so hopeless to even try

ура, работает!
";


        static void Main(string[] args)
        {
            string alphabet = "~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?`1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮйцукенгшщзхъфывапролджэячсмитьбю\r\n\t ";
            string[] codes = Initialize(alphabet);
            string input = lyrics;
            Console.Write("Enter password:");
            string passwordText = Console.ReadLine();
            string encodedText = input.Mask();
            byte[] randomPassword = Encoding.UTF8.GetBytes(passwordText.ToCharArray());
            for (int i = 0; i < randomPassword.Length; i++)
            {
                encodedText = Encode(randomPassword[i], encodedText, codes, alphabet);
            }
            encodedText = encodedText.ToBase64String();
            Console.WriteLine(encodedText);

            string decodedText = encodedText.FromBase64String();
            randomPassword = randomPassword.Reverse().ToArray();
            for (int i = 0; i < randomPassword.Length; i++)
            {
                decodedText = Decode(randomPassword[i], decodedText, codes, alphabet);
            }
            Console.WriteLine(decodedText.Unmask());

            Console.ReadKey(false);
        }

        private static string[] Initialize(string alphabet)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                char[] chars = alphabet.ToCharArray();
                chars[i] = '\0';
                result.Add(chars.Aggregate(new StringWriter(), (c, p) => { c.Write(p); return c; }).ToString());
            }
            return result.ToArray();
        }

        static string Encode(int seed, string s, string[] codetable, string alphabet)
        {
            Random rnd = new Random(seed);
            StringWriter sw = new StringWriter();
            for (int i = 0; i < s.Length; i++)
            {
                int j = alphabet.IndexOf(s[i]);
                if (j != -1)
                {
                    int index;
                    bool found = false;
                    do
                    {
                        index = (j + rnd.Next()) % codetable.Length;
                        if (codetable[j][index] != 0)
                        {
                            found = true;
                            sw.Write(alphabet[index]);
                            break;
                        }
                    } while (!found);
                }
                else
                {
                    sw.Write(s[i]);
                }
            }
            return sw.ToString();
        }

        static string Decode(int seed, string s, string[] codetable, string alphabet)
        {
            Random rnd = new Random(seed);
            StringWriter sw = new StringWriter();
            for (int i = 0; i < s.Length; i++)
            {
                int k = alphabet.IndexOf(s[i]);
                if (alphabet.IndexOf(s[i]) != -1)
                {
                    int random;
                    bool found;
                    do
                    {
                        random = rnd.Next();
                        found = false;
                        int index;
                        for (int j = 0; j < codetable.Length; j++)
                        {
                            index = (j + random) % codetable.Length;
                            if (codetable[j][index] == s[i])
                            {
                                found = true;
                                sw.Write(alphabet[j]);
                                break;
                            }
                        }
                    } while (!found);
                }
                else
                {
                    sw.Write(s[i]);
                }
            }
            return sw.ToString();
        }
    }
}
