using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

namespace SanyoniLib.SystemHelper
{

    public static class PathHelper
    {
        private const string FileNameRegex = @"(?<FileName>[\w\-. ]+)(\.(?<FileExtension>[\w]*){0,1})$";
        private const string FileNameRegexSubgroupFileName = "FileName";
        private const string FileNameRegexSubgroupFileExtension = "FileExtension";

        private const string BmpExtensionText = ".bmp";
        private const string JpgExtensionText = ".jpg";
        private const string PngExtensionText = ".png";

        private const string OggVorbisExtensionText = ".ogg";
        private const string WavExtensionText = ".wav";
        private const string MP3ExtensionText = ".mp3";

        public static void ExtractFileNameAndExtension(string fileName, out string fileNameWithoutExtension, out string fileExtension)
        {
            Regex fileNameRegex = new Regex(FileNameRegex);

            // 가능하다면 파일 이름만을 분리해내서 저장한다.
            var fileNameRegexMatchGroup = fileNameRegex.IsMatch(fileName) ? fileNameRegex.Match(fileName).Groups : null;
            fileNameWithoutExtension = fileNameRegexMatchGroup != null ? fileNameRegexMatchGroup[FileNameRegexSubgroupFileName].Value : null;
            fileExtension = fileNameRegexMatchGroup != null ? fileNameRegexMatchGroup[FileNameRegexSubgroupFileExtension].Value : null;
        }

        /// <summary>
        /// 실제 이미지파일의 이름을 반환한다. 이 때 이름에는 확장자도 포함된다.
        /// </summary>
        /// <param name="dir">파일의 디렉토리 주소.</param>
        /// <param name="fileName">실제 파일 이름과 동일하지 않을 수도 있는 확장자를 포함한 이미지 이름.</param>
        /// <returns></returns>
        public static string GuessRealImageFileName(string dir, string fileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string realFileName;
            if (File.Exists(Path.Combine(dir, realFileName = (fileNameWithoutExtension + BmpExtensionText)))) return realFileName;
            else if (File.Exists(Path.Combine(dir, realFileName = (fileNameWithoutExtension + JpgExtensionText)))) return realFileName;
            else if (File.Exists(Path.Combine(dir, realFileName = (fileNameWithoutExtension + PngExtensionText)))) return realFileName;

            return null;
        }

        public static string GuessRealAudioFileName(string dir, string fileName)
        {
            string fileNameDir = Path.GetDirectoryName(fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string realFileName;
            if (File.Exists(Path.Combine(dir, realFileName = Path.Combine(fileNameDir, (fileNameWithoutExtension + OggVorbisExtensionText))))) return realFileName;
            else if (File.Exists(Path.Combine(dir, realFileName = Path.Combine(fileNameDir, (fileNameWithoutExtension + WavExtensionText))))) return realFileName;
            else if (File.Exists(Path.Combine(dir, realFileName = Path.Combine(fileNameDir, (fileNameWithoutExtension + MP3ExtensionText))))) return realFileName;

            return null;
        }

        public static string GuessExistAudioFullPath(string dir, string fileNameWithoutExtension)
        {
            string fileFullPath;
            if (File.Exists(fileFullPath = Path.Combine(dir, fileNameWithoutExtension) + OggVorbisExtensionText)) return fileFullPath;
            else if (File.Exists(fileFullPath = Path.Combine(dir, fileNameWithoutExtension) + WavExtensionText)) return fileFullPath;
            else if (File.Exists(fileFullPath = Path.Combine(dir, fileNameWithoutExtension) + MP3ExtensionText)) return fileFullPath;

            return null;
        }

        /// <summary>
        /// 인자로 받은 파일명 문자열에 파일로 저장할 수 없는 문자가 포함되어 있다면 이것을 _(underbar)로 바꾸고 반환한다.
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public static string ConvertToValidFileName(string _fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                _fileName = _fileName.Replace(c, '_');
            }

            return _fileName;
        }

    }

}