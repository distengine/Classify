using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Classify
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Classify
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        
        public static readonly Guid CommandSet = new Guid("039b98c6-dd2a-4381-94f7-b9d553b1be13");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Classify"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Classify(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Classify Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
                Instance = new Classify(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Get a DTE instance and get the current project selected
            EnvDTE.DTE dte;
            EnvDTE.SelectedItem curSel;
            EnvDTE.Project proj;
            EnvDTE.ProjectItems projItems;
            dte = (EnvDTE.DTE)this.ServiceProvider.GetService(typeof(EnvDTE.DTE));
            curSel = dte.SelectedItems.Item(1);
            proj = curSel.Project;
            projItems = proj.ProjectItems;


            // Create a instance of our classify class and show the dialog
            ClassifyWPF.AddClass addClass = new ClassifyWPF.AddClass();
            bool? result = addClass.ShowDialog();
            if (result == true)
            {
                addClass.ClassifyCleanStrings();
                string classDecName;
                string classDecPath;
                string classImpName;    
                string classImpPath;
                ArrayList cppFile = new ArrayList();

                int index = -1;

                // Get .h and .cpp file
                if (addClass.declerationName.Contains("\\"))
                {
                    index = addClass.declerationName.LastIndexOf("\\");
                    classDecPath = addClass.declerationName.Substring(0, index);
                    classDecName = addClass.declerationName.Substring(index + 1);
                }
                else
                {
                    classDecPath = "Default";
                    classDecName = addClass.declerationName;
                }
                if (addClass.implementationName.Contains("\\"))
                {
                    index = addClass.implementationName.LastIndexOf("\\");
                    classImpPath = addClass.implementationName.Substring(0, index);
                    classImpName = addClass.implementationName.Substring(index + 1);
                }
                else
                {
                    classImpPath = "Default";
                    classImpName = addClass.implementationName;
                }

                // Get project path
                string projPath = proj.FullName;
                index = projPath.LastIndexOf("\\");
                projPath = projPath.Substring(0, index);

                // Get .cpp file path
                if (!addClass.relativePath)
                {
                    // do nothing since the path is already absolute, we just need a use case to catch the information
                }
                else if (classImpPath != "Default")
                {
                    classImpPath = projPath + '\\' + classImpPath + '\\';
                }
                else
                {
                    classImpPath = projPath + '\\';
                }

                // Get .g file path
                if (!addClass.relativePath)
                {
                    // do nothing since the path is already absolute, we just need a use case to catch the information
                }
                else if (classDecPath != "Default")
                {
                    classDecPath = projPath + '\\' + classDecPath + '\\';
                }
                else
                {
                    classDecPath = projPath + '\\';
                }

                // Create paths if they don't exist
                if (!System.IO.Directory.Exists(classDecPath))
                {
                    System.IO.Directory.CreateDirectory(classDecPath);
                }
                if (!System.IO.Directory.Exists(classImpPath))
                {
                    System.IO.Directory.CreateDirectory(classImpPath);
                }

                // Read our template text file
                string fileText;
                var assemly = Assembly.GetExecutingAssembly();
                var resourceName = "Classify.TemplateClass.txt";
                using (System.IO.Stream stream = assemly.GetManifestResourceStream(resourceName))
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    fileText = reader.ReadToEnd();
                }
                string[] lines = fileText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                ArrayList fileLines = new ArrayList(lines);

                // remove namespace from file
                if (addClass.namespaceName == null)
                {
                    fileLines.RemoveAt(12);
                    fileLines.RemoveAt(3);
                    fileLines.RemoveAt(2);
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        string temp = (string)fileLines[i];
                        if (temp.Contains("\t"))
                            fileLines[i] = temp.Replace("\t", "");
                    }
                }
                else // keep namespace and replace it with namespace name
                {
                    string namespaceLine = (string)fileLines[2];
                    namespaceLine = namespaceLine.Replace("#namespaceName", addClass.namespaceName);
                    fileLines[2] = namespaceLine;
                }

                // Removed Namespace
                if (fileLines.Count == 11)
                {
                    // Check for base class
                    if (addClass.baseClassName == null)
                    {
                        string baseClassLine = (string)fileLines[2];
                        baseClassLine = baseClassLine.Replace(" : #baseClassName", "");
                        baseClassLine = baseClassLine.Replace("#className", addClass.className);
                        fileLines[2] = baseClassLine;
                    }
                    else
                    {
                        string baseClassLine = (string)fileLines[2];
                        baseClassLine = baseClassLine.Replace("#baseClassName", addClass.baseClassName);
                        baseClassLine = baseClassLine.Replace("#className", addClass.className);
                        fileLines[4] = baseClassLine;
                    }

                    // check for creation settings
                    if (addClass.onlyConDes)
                    {
                        cppFile.Add("#include \"" + addClass.className + ".h\"" + System.Environment.NewLine);
                        cppFile.Add(System.Environment.NewLine);
                        cppFile.Add(addClass.className + "::" + addClass.className + "()");
                        cppFile.Add("{");
                        cppFile.Add("}");

                        cppFile.Add(System.Environment.NewLine);
                        cppFile.Add(addClass.className + "::" + "~" + addClass.className + "()");
                        cppFile.Add("{");
                        cppFile.Add("}");

                        string conLine = (string)fileLines[5];
                        string desLine = (string)fileLines[6];
                        fileLines[5] = conLine.Replace("#classNameCon", addClass.className + "();");
                        fileLines[6] = desLine.Replace("#classNameDes", "~" + addClass.className + "();");
                    }
                    else
                    {
                        cppFile.Add("#include \"" + addClass.className + ".h\"" + System.Environment.NewLine);
                        fileLines.Remove(6);
                        fileLines.Remove(5);
                    }
                }
                else // has namespace
                {
                    // Check for base class
                    if (addClass.baseClassName == null)
                    {
                        string baseClassLine = (string)fileLines[4];
                        baseClassLine = baseClassLine.Replace(" : #baseClassName", "");
                        baseClassLine = baseClassLine.Replace("#className", addClass.className);
                        fileLines[4] = baseClassLine;
                    }
                    else
                    {
                        string baseClassLine = (string)fileLines[4];
                        baseClassLine = baseClassLine.Replace("#baseClassName", addClass.baseClassName);
                        baseClassLine = baseClassLine.Replace("#className", addClass.className);
                        fileLines[2] = baseClassLine;
                    }

                    // check for creation settings
                    if (addClass.onlyConDes)
                    {
                        cppFile.Add("#include \"" + addClass.className + ".h\"" + System.Environment.NewLine);
                        cppFile.Add(addClass.namespaceName + "::" + addClass.className + "::" + addClass.className + "()");
                        cppFile.Add("{");
                        cppFile.Add("}");

                        cppFile.Add(System.Environment.NewLine);
                        cppFile.Add(addClass.namespaceName + "::" + addClass.className + "::" + "~" + addClass.className + "()");
                        cppFile.Add("{");
                        cppFile.Add("}");
                        string conLine = (string)fileLines[7];
                        string desLine = (string)fileLines[8];
                        fileLines[7] = conLine.Replace("#classNameCon", addClass.className + "();");
                        fileLines[8] = desLine.Replace("#classNameDes", "~" + addClass.className + "();");
                    }
                    else
                    {
                        cppFile.Add("#include \"" + addClass.className + ".h\"" + System.Environment.NewLine);
                        fileLines.Remove(8);
                        fileLines.Remove(7);
                    }
                }

                // write our strings to the .h and .cpp files
                string[] fileLinesArray = (string[])fileLines.ToArray(typeof(string));
                string[] cppLinesArray = (string[])cppFile.ToArray(typeof(string));
                System.IO.File.WriteAllLines(classDecPath + classDecName, fileLinesArray);
                System.IO.File.WriteAllLines(classImpPath + classImpName, cppLinesArray);
                projItems.AddFromFile(classDecPath + classDecName);
                projItems.AddFromFile(classImpPath + classImpName);
            }                                                                                                               
            else // user hit cancel or esc or x button                                                                                  
            {
            }
        }
    }
}
