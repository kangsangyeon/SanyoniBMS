using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SanyoniLib.SystemHelper
{


    public static class FileHelper
    {
        private const string EscapeLetterTextRegex = @"[\r\n\b]";



        public static bool LoadFileAndSplitByLines(string filePath, out string[] outLines)
        {
            return LoadFileAndSplitByLines(filePath, out outLines, 1, 0);
        }

        /// <summary>
        /// 파일의 특정 라인에서부터 특정 개수만큼의 줄을 읽어 outLines로 리턴합니다.
        /// 파일을 성공적으로 읽어오면 true를 리턴합니다.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="outLines">파일에서 읽어온 줄이 리턴되는 out변수.</param>
        /// <param name="startLine">몇 번째의 줄부터 읽어올 것인지를 설정하는 변수. 0을 전달하면 파일의 시작부분부터 읽습니다.</param>
        /// <param name="lineCount">몇 개의 줄을 읽어올 것인지를 설정하는 변수. 0을 전달하면 파일의 끝까지 읽습니다.</param>
        /// <returns></returns>
        public static bool LoadFileAndSplitByLines(string filePath, out string[] outLines, int startLine, int lineCount)
        {
            bool isFileExist = File.Exists(filePath);
            if (isFileExist == false)
            {
                Debug.LogError("존재하지 않는 파일에 대한 접근 시도입니다.");

                outLines = null;
                return false;
            }

            Encoding encoding = GetEncoding(filePath);

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs, encoding, true);


                string originalText;
                for (int i = 1; i < startLine; i++)
                {
                    sr.ReadLine();
                }

                if (lineCount == 0)
                {
                    originalText = sr.ReadToEnd();
                    outLines = originalText.Split('\n');
                }
                else
                {
                    outLines = new string[lineCount];

                    for (int i = 0; i < lineCount; i++)
                    {
                        outLines[i] = sr.ReadLine();

                        if (sr.EndOfStream == true)
                            break;
                    }
                }

                // escape문자를 제거한다.
                for (int i = 0; i < outLines.Length; i++)
                {
                    string actualLine = Regex.Replace(outLines[i], EscapeLetterTextRegex, "");
                    outLines[i] = actualLine;
                }

            }

            return true;
        }

        public static bool LoadRawFile(string filePath, out byte[] rawBinary)
        {

            bool isFileExist = File.Exists(filePath);
            if (isFileExist == false)
            {
                Debug.LogError("존재하지 않는 파일에 대한 접근 시도입니다.");

                rawBinary = null;
                return false;
            }

            /*
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    rawBinary = br.ReadBytes(int.MaxValue);
                }
            }
            */
            rawBinary = File.ReadAllBytes(filePath);

            return true;
        }

        private static Encoding GetEncoding(string filename)
        {
            // This is a direct quote from MSDN:
            // The CurrentEncoding value can be different after the first
            // call to any Read method of StreamReader, since encoding
            // autodetection is not done until the first call to a Read method.

            using (var reader = new StreamReader(filename, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                return reader.CurrentEncoding;
            }
        }

    }

}