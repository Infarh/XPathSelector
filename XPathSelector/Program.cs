using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

const StringComparison cmp = StringComparison.OrdinalIgnoreCase;

var files = args.Where(arg => arg is not ['-' or '/', ..] && File.Exists(arg) || arg.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToArray();
var xpaths = XPaths(args.Except(files));

if (files.Length > 0)
    try
    {
        foreach (var xml in files.Select(XDocument.Load).Select(xml => xml.CreateNavigator()))
        foreach (var xpath in xpaths)
            if (IsXmlResult(xpath, out var path))
                foreach (XPathNavigator item in xml.Select(path))
                    Console.WriteLine(item.OuterXml);
            else
                foreach (XPathNavigator item in xml.Select(path))
                    Console.WriteLine(item.Value);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }

else //if(!Console.KeyAvailable)
{
    //if (!Debugger.IsAttached)
    //{
    //    Debugger.Launch();
    //    Debugger.Break();
    //}

    var in_reader = Console.In;
    while (in_reader.Peek() != 60)
        if(in_reader.Read() == -1)
            return;

    try
    {
        var xml = XDocument.Load(in_reader).CreateNavigator();

        foreach (var xpath in xpaths)
            if (IsXmlResult(xpath, out var path))
                foreach (XPathNavigator item in xml.Select(path))
                    Console.WriteLine(item.OuterXml);
            else
                foreach (XPathNavigator item in xml.Select(path))
                    Console.WriteLine(item.Value);
    }
    catch (XmlException e)
    {
        Console.Error.WriteLine(e.Message);
    }
}

return;

static string[] XPaths(IEnumerable<string> args)
{
    var result = new List<string>();
    var parameter = false;
    foreach (var arg in args)
        if (parameter)
        {
            result[^1] += $" {arg}";
            parameter = false;
        }
        else
        {
            result.Add(arg);
            if (arg is ['-' or '/', ..])
                parameter = true;
        }

    return result.ToArray();
}

static bool IsXmlResult(string xpath, out string path)
{
    if (xpath.StartsWith("-x ", cmp)
        || xpath.StartsWith("/x ", cmp)
        || xpath.StartsWith("-xml ", cmp)
        || xpath.StartsWith("--xml ", cmp)
        || xpath.StartsWith("/xml ", cmp))
    {
        path = xpath[(xpath.IndexOf(' ') + 1)..];
        return true;
    }

    path = xpath.StartsWith("-v ", cmp)
           || xpath.StartsWith("/v ", cmp)
           || xpath.StartsWith("-val ", cmp)
           || xpath.StartsWith("--val ", cmp)
           || xpath.StartsWith("/val ", cmp)
           || xpath.StartsWith("-value ", cmp)
           || xpath.StartsWith("--value ", cmp)
           || xpath.StartsWith("/value ", cmp)
        ? xpath[(xpath.IndexOf(' ') + 1)..]
        : xpath;

    return false;
}