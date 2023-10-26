using System;
using Microsoft.VisualBasic.FileIO;
using System.Reflection.PortableExecutable;
using System.Globalization;

public class CustomerData
{
    public DateTime Date{ get; set; }
    public FileInfo DataLocation{ get; set; }
    public List<string> originalData{get; set;}
    public List<string> parsedData{get; set;}
    public List<string> parseLog{get; set;}
    public CustomerData(string fileLoction)
    {
        DataLocation = new FileInfo(fileLoction);
        Date = this.getDate(new FileInfo(fileLoction));
        this.ParseData();
    }

    private DateTime getDate(FileInfo currentFile)
    {
        string file_day = currentFile.Directory.Name;
        string file_month = currentFile.Directory.Parent.Name;
        string file_year = currentFile.Directory.Parent.Parent.Name;
        DateTime Date = new DateTime(Convert.ToInt32(file_year),Convert.ToInt32(file_month), Convert.ToInt32(file_day));
        return Date;
    }


    private void ParseData()
    {
        string file_loc = this.DataLocation.ToString();
        List<string> lines = new List<string>();
        List<string> parsed_lines = new List<string>();
        List<string> logs = new List<string>();
        string[] header = new string[0];

        using (TextFieldParser parser = new TextFieldParser(file_loc))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = false;
            parser.TrimWhiteSpace = true;

            int lineNumber = 0;

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                string currentLine = string.Join(",", fields);
                lines.Add(currentLine);


                if (lineNumber == 0)
                {
                    lineNumber++;
                    header = fields;
                    continue;
                }

                if(fields.Length != 10)
                {
                    logs.Add("skipped line " + lineNumber + " for not enough element in " + this.DataLocation.ToString());
                    lineNumber++;
                    continue;
                }

                bool badValue = false;
                foreach(string field in fields)
                {
                    if(field == "" || field == "\"\"")
                    {
                        badValue = true;
                        break;
                    }
                }

                if(badValue == true)
                {
                    logs.Add("skipped line " + lineNumber + " for not enough element in " + this.DataLocation.ToString());
                    lineNumber++;
                    continue;
                }
               
                parsed_lines.Add(currentLine+","+this.Date.ToString("yyyy/MM/dd"));
                lineNumber++;
            }
        }
        this.originalData = lines;
        this.parsedData = parsed_lines;
        this.parseLog = logs;
    }
}

class Program
{
    static void Main()
    {
        string outputFileName = "output.csv";
        string outputFileDirectory = "Output";
        if (!Directory.Exists(outputFileDirectory))
        {
            Directory.CreateDirectory(outputFileDirectory);
        }
        string outputFilePath = Path.Combine(outputFileDirectory, outputFileName);

        string outputLogFileName = "parse_log.txt";
        string outputLogDirectory = "logs";
        if (!Directory.Exists(outputLogDirectory))
        {
            Directory.CreateDirectory(outputLogDirectory);
        }
        string outputLogPath = Path.Combine(outputLogDirectory, outputLogFileName);
        
        
        string header = "First Name,Last Name,Street Number,Street,City,Province,Postal Code,Country,Phone Number,email Address,Date";
        string[] allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv", System.IO.SearchOption.AllDirectories);  
        Console.WriteLine(Directory.GetCurrentDirectory());
        
        List<string> parsedData = new List<string>();
        List<string> outputLogs = new List<string>();
        List<string> allFileLength = new List<string>();
        parsedData.Add(header);

        List<CustomerData> all_Files = new List<CustomerData>();
        foreach(string eachFile in allFiles)
        {
            FileInfo newFile = new FileInfo(eachFile);
            if(newFile.Name == "output.csv")
            {
                continue;
            }
            CustomerData each_file = new CustomerData(eachFile);
            all_Files.Add(each_file);
        }
        all_Files.Sort((a, b)=> a.Date.CompareTo(b.Date));
        foreach(CustomerData file in all_Files)
        {
            parsedData = parsedData.Concat(file.parsedData).ToList();
            outputLogs = outputLogs.Concat(file.parseLog).ToList();
            allFileLength = allFileLength.Concat(file.originalData).ToList();
        }
        File.WriteAllLines(outputFilePath, parsedData);
        File.WriteAllLines(outputLogPath, outputLogs);
        using (StreamWriter sw = File.AppendText(outputLogPath))
        {
            sw.WriteLine("Total processed file: "+ all_Files.Count);
            sw.WriteLine("Total line processed: "+ allFileLength.Count);
            sw.WriteLine("Total line removed(including header): "+ (outputLogs.Count + all_Files.Count - 1));
            sw.WriteLine("Bad data line removed: "+ outputLogs.Count);
        }
        Console.WriteLine("Ding");

    }
}