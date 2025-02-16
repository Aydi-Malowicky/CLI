using System.CommandLine;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.IO.Compression;
using System.Linq.Expressions;
string filePath = @"C:\MyPage\myfile.txt";
//יצירת אופציות
var outputOption = new Option<String>("--output", "File path and name");
var languageOption = new Option<String>("--language", "language of the code files")
{
    IsRequired = true
};
var noteOption = new Option<bool>("--note", "if to make in the document notes");
var sortOption = new Option<String>("--sort", "the way to sort the files");
var removeOption = new Option<bool>("--remove-empty-lines", "to remove empty lines");
var authorOption = new Option<String>("--author", "the name of the author");
//יצירת אליאסים לכל האופציות
outputOption.AddAlias("-o");
languageOption.AddAlias("-l");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeOption.AddAlias("-r");
authorOption.AddAlias("-a");
//יצירת ברירת מחדל לoutput
outputOption.SetDefaultValue(Path.Combine(Directory.GetCurrentDirectory(), "CLI.txt"));

//יצירת הפקודה בנדל
var bundleCommand = new Command("bundle", "bundel for code files to a single file ");

//הוספת אופשנים
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeOption);
bundleCommand.AddOption(authorOption);
//סט הנדלר
bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
{
    //אם המשתמש לא הקיש ניתוב מלא - שימוש במיקום בו הוא נמצא
    if (!output.Contains("\\"))
        output = Path.Combine(Directory.GetCurrentDirectory(), output);
    //בדיקות תקינות השפה
    if (language != "html" && language != "java script" && language != "c#" && language != "sql" && language != "c" && language != "c++" && language != "java"  &&language!= "python"&&language!="all")
    {
        Console.WriteLine("The language isn't exist");
        return;
    }
    //בדיקת תקינות המיון
    if(sort!=null&&sort!="AB"&&sort!="type"&&sort!="ab")
    {
        Console.WriteLine("The sort mast be AB or type");
        return;
    }
    
    //נסיון אגירת הקבצים
    try
    {
        //אגירת הקבצים
        //בידוד הניתוב - התיקייה בה המשתמש הזין באאוטפוט
        string filePath = Path.GetDirectoryName(output);
        //בחירת סוג הקובץ - לפי מה שהמשתמש הזין בשפה
        string searchPattern = "";
        if (language.Equals("python"))
            searchPattern = "*.py";
        else if (language.Equals("java"))
            searchPattern = "*.java";
        else if (language.Equals("c++"))
            searchPattern = "*.cpp";
        else if (language.Equals("c"))
            searchPattern = "*.c";
        else if (language.Equals("sql"))
            searchPattern = "*.sql";
        else if (language.Equals("c#"))
            searchPattern = "*.cs";
        else if (language.Equals("java script"))
            searchPattern = "*.js";
        else if (language.Equals("html"))
            searchPattern = "*.html";
        //איסוף הקבצים לפי הפרמטרים - תיקייה, שפה
        string[] files = Directory.GetFiles(filePath, searchPattern, SearchOption.AllDirectories);
        //מיון הקבצים לפי סוג
        if(sort == "type")
            files = files.OrderBy(h => Path.GetExtension(h)).ToArray();
        //מיון הקבצים לפי הא ב
        else
            files = files.OrderBy(f => Path.GetFileName(f)).ToArray();
        
        string fileContent = "";
        //כתיבת שם המחבר אם המשתמש בחר בזה
        if (author != null)
            fileContent = author + Environment.NewLine;
        //קבלת כל הניתובים הנמצאים בתיקייה הראשית
        string[] subDirectories = Directory.GetDirectories(filePath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            bool skipFile = false;
            // בדיקה אם הקובץ נמצא בתיקיית "bin" או "Debug"
            foreach (string subDir in subDirectories)
            {
                if (subDir.Contains("bin") || subDir.Contains("Debug"))
                {
                    if (file.StartsWith(subDir))
                    {
                        skipFile = true;
                        break;
                    }
                }
            }
            //אם בשפה נבחר הכל, בדיקה האם שם הקובץ מסתיים באחד מהשפות הקיימות
            bool isTrueLanguage = true;
            ;
            if (language.Equals("all"))
            {
                isTrueLanguage = false;
                if (file.EndsWith(".py"))
                    isTrueLanguage = true;
                if (file.EndsWith(".java"))
                    isTrueLanguage = true;
                if (file.EndsWith(".spp"))
                    isTrueLanguage = true;
                if (file.EndsWith(".c"))
                    isTrueLanguage = true;
                if (file.EndsWith(".sql"))
                    isTrueLanguage = true;
                if (file.EndsWith(".cs"))
                    isTrueLanguage = true;
                if (file.EndsWith(".js"))
                    isTrueLanguage = true;
                if (file.EndsWith(".html"))
                    isTrueLanguage = true;
            }
            if (!skipFile && isTrueLanguage)
            {
                                  //הוספת הערה עם הניתוב אם המשתמש בחר את זה
                    if (note)
                    {
                        fileContent += file;
                        fileContent += Environment.NewLine;
                    }
                //קריאת הקבצים והוספה למשתנה
                fileContent += File.ReadAllText(file);
                fileContent += Environment.NewLine;
            }
        }
        //מחיקת שורות ריקות אם המשתמש בחר בזה
        if (remove == true)
            fileContent = string.Join("\n", fileContent.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)));
        //כתיבת הטקסט לקובץ
        File.AppendAllText(output, fileContent);
    }
    //זריקת שגיאה אם זה לא עבד
    catch
    {
        Console.WriteLine("It doesn't sucsses, I don't know why...");
    }
}, outputOption, languageOption, noteOption, sortOption, removeOption, authorOption);
var rootCommand = new RootCommand("Root command for a File bundle CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);


