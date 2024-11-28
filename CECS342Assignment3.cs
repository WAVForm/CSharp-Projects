// Yunis Nabiyev
// CECS 342 Assignment 3
// File Type Report
// Run from the command line. Given the path to a folder as the first argument and a path to output an HTML file to as the second, list each extension, the amount of files with that extension, and the total size in bytes of all the files with that extension. The program recursively finds all subdirectories. 

using System.Xml.Linq;

namespace FileTypeReport
{
    internal static class Program
    {
        // 1. Enumerate all files in a folder recursively
        private static IEnumerable<string> EnumerateFilesRecursively(string path)
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(path)) //get files in current path
            {
                yield return file;
            }
            foreach (var dir in System.IO.Directory.EnumerateDirectories(path)) //go one level in
            {
                foreach (var file in EnumerateFilesRecursively(dir)) //get files in next dir
                {
                    yield return file;
                }
            }
        }

        // Human readable byte size
        private static string FormatByteSize(long byteSize)
        {
            string[] sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
            int suffixIndex = 0;

            var temp = (float)byteSize;
            while (temp >= 1000)
            {
                suffixIndex++;
                temp /= 1000;
            }
            return (suffixIndex > 0) ? string.Format("{0:N2}", temp) + sizeSuffixes[suffixIndex] : byteSize + sizeSuffixes[suffixIndex];
        }

        // Create an HTML report file
        private static XDocument CreateReport(IEnumerable<string> files)
        {

            var query = //create query
              from file in files //for each file path in files collection
              group file by new FileInfo(file).Extension into fileGroup //group all alike extensions into fileGroup
              let totalSize = fileGroup.Sum(file => file.Length) //calculate the size (Length) of each file in fileGroup, then sum together
              orderby totalSize descending //order groups by size in descending order
              select new //return anonymous type
              {
                  Type = fileGroup.Key, // file extension of group
                  Count = fileGroup.Count(), //number of files in group
                  TotalSize = FormatByteSize(totalSize) //size of all files in group
              };

            var light_row_style = new XAttribute("style", "background-color:#dad2bc; color:#252323; border:3px solid #f5f1ed"); //styling for every even row
            var dark_row_style = new XAttribute("style", "background-color:#c2b6a1; color:#252323; border:3px solid #f5f1ed"); //styling for every odd row

            // 3. Functionally construct XML
            var tableRows = new List<XElement>();
            var evenOrOdd = 0;
            foreach (var fileGroup in query) //for each group in the previous query
            {
                var styling = (evenOrOdd++ % 2 == 0) ? light_row_style : dark_row_style; //which styling to use?
                tableRows.Add( //add below row to list
                    new XElement("tr", styling, //create table row with styling
                      new XElement("td", fileGroup.Type), //add group extension to row
                      new XElement("td", fileGroup.Count), //add # of files in group to row
                      new XElement("td", fileGroup.TotalSize) //add total size of group to row
                    )
                );
            }

            return new XDocument( //create markup object for output
                new XElement("html", //root html element
                  new XElement("head", //root head element
                    new XElement("title", "Assignment 3 Report"), //title of page
                    new XElement("meta", new XAttribute("charset", "UTF-8")) //encoding
                  ),
                  new XElement("body", new XAttribute("style", "background-color:#a99985"), //body of page, w/ styling
                    new XElement("table", new XAttribute("style", "position:absolute; left:0px; top:0px; width:100%; text-align:center; border: solid 5px #252323; border-collapse: collapse;"), // table of info w/ styling
                      new XElement("tr", new XAttribute("style", "background-color:#70798c; color:#f5f1ed; font-size:32px; border:3px solid #f5f1ed;"), //table header w/ styling
                        new XElement("th", "Type"), //column 1: extension
                        new XElement("th", "Count"), //column 2: # of files w/ extension
                        new XElement("th", "Size") //column 3: total size of files w/ extension
                      ),
                      tableRows //add all the previously generated rows to table
                    )
                  )
                )
            );
        }

        // Console application with two arguments
        public static void Main(string[] args)
        {
            try
            {
                string inputFolder = args[0];
                string reportFile = args[1];

            }
            catch
            {
                Console.WriteLine("Usage: FileTypeReport <folder> <report file>");
            }
        }
    }
}