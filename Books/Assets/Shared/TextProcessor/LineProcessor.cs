using System.Linq;

namespace Shared.Disposable 
{
    public static class LineProcessor
    {
        public static bool TryProcessLine(this string line, out string header, out string attributes, out string body) 
        {
            header = string.Empty;
            attributes = string.Empty;
            body = string.Empty;

            line = line.Trim();

            if (string.IsNullOrEmpty(line)) return false;

            var rawTexts = line.Split(":");
            header = rawTexts.Length > 1 ?
                rawTexts[0].Split("(").FirstOrDefault().Trim() :
                string.Empty;
            attributes = rawTexts[0].Contains("(") ?
                rawTexts[0].Split("(").LastOrDefault().Split(")").FirstOrDefault().Trim() :
                string.Empty;
            body = rawTexts.Length > 1 ?
                rawTexts[1].Trim() :
                line;

            return true;
        }
    }
}

