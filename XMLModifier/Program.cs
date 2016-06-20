using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace XMLModifier
{
    class Program
    {
        private static string root, file, selectNode;

        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                ParseParameter(arg);
            }

            string backup = @"\" + file.Replace(".", "_") + "_Backup_" + DateTime.Today.ToShortDateString();

            if (file == "BatchConfig.xml")
            {
                selectNode = "/JobList/job";
            }
            else
            {
                selectNode = "/ArrayOfJobClass/JobClass/Tasks/TaskClass/Execute/NetworkCredential";
            }

            DirectoryInfo dirPrograms = new DirectoryInfo(root);
            var dirs = from dir in dirPrograms.EnumerateDirectories()
                       select new
                       {
                           ProgDir = dir,
                       };

            foreach (var di in dirs)
            {
                string filePath = root + "\\" + di.ProgDir.Name + "\\" + file;
                string backupFolder = root + "\\" + di.ProgDir.Name + backup.Replace("/", "");
                string backupPath = backupFolder + "\\" + file;

                if (File.Exists(filePath))
                {
                    //take backup
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }
                    File.Copy(filePath, backupPath, true);

                    ModifyXML(filePath, selectNode);
                }
            }
        }

        private static void ParseParameter(string arguments)
        {
            string[] argument = arguments.Split('=');

            if (argument != null)
            {
                switch (argument[0].ToLower())
                {
                    case "root":
                        root = argument[1].ToString();
                        break;
                    case "file":
                        file = argument[1].ToString();
                        break;
                }

            }
        }

        private static void ModifyXML(string filePath, string selectNode)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNodeList aNodes = doc.SelectNodes(selectNode);

            foreach (XmlNode aNode in aNodes)
            {
                if (file == "BatchConfig.xml")
                { 
                    string category = aNode["Category"].InnerText;

                    if (category == "Failure")
                    {
                        // Get the XML content of the target node
                        string commentContents = aNode.OuterXml;

                        // Create a new comment node
                        // Its contents are the XML content of target node
                        XmlComment commentNode = doc.CreateComment(commentContents);

                        // Get a reference to the parent of the target node
                        XmlNode parentNode = aNode.ParentNode;

                        // Replace the target node with the comment
                        parentNode.ReplaceChild(commentNode, aNode);

                        Console.WriteLine("Processed " + filePath);
                    }
                }

                else
                {
                    if (aNode.Name == "NetworkCredential")
                    {
                        aNode.InnerText = "2f8c18d8-9c46-4994-b469-26546ab5c975";
                    }
                }
            }
            doc.Save(filePath);
        }
    }
    
}
