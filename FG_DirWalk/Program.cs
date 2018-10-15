using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;


//https://msdn.microsoft.com/en-us/library/bb513869.aspx

public class FG_DirWalk
{
    static void Main(string[] args)
    {

        Console.WriteLine("Usage: StageFiles [Directory] [Server] [Database] [User] [Password]");
        //string connectionString = ConfigurationManager.ConnectionStrings["FileGardenConn"].ConnectionString;
        //string name = Properties.Settings.Default.FileGardenConn;
        //public string FileGardenConn = ConfigurationManager.ConnectionStrings["FileGardenConn"];

        // Specify the starting folder on the command line, or in 
        // Visual Studio in the Project > Properties > Debug pane.
        TraverseTree(args[0]);

        Console.WriteLine("Press any key");
        Console.ReadKey();
    }

    public static void TraverseTree(string root)
    {
        // Data structure to hold names of subfolders to be
        // examined for files.
        Stack<string> dirs = new Stack<string>(20);

        if (!System.IO.Directory.Exists(root))
        {
            throw new ArgumentException();
        }
        dirs.Push(root);

        //SqlConnection myConnection = new SqlConnection("user id=FileGardenUser;" +
        //                   "password=GhiES35DSS1_23;server=localhost;" +
        //                   "Trusted_Connection=yes;" +
        //                   "database=FileGarden; " +
        //                   "connection timeout=30");

        var FileGardenConn = ConfigurationManager.ConnectionStrings["FileGardenConn"].ToString();
        SqlConnection myConnection = new SqlConnection(FileGardenConn);
        try
        {
            myConnection.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        SqlCommand myCommand = new SqlCommand("Command String", myConnection);
        myCommand.CommandText = "TRUNCATE TABLE [Stage].[Dir]; TRUNCATE TABLE [Stage].[File]";
        myCommand.ExecuteNonQuery();
        Console.WriteLine("Stage tabeller slettet");

        while (dirs.Count > 0)
        {
            string currentDir = dirs.Pop();
            //string previousDir = "";
            string[] subDirs;
            try
            {
                //previousDir = currentDir;
                subDirs = System.IO.Directory.GetDirectories(currentDir);
                //Console.WriteLine("Scanning {0}", currentDir);

                myCommand.CommandText = "INSERT INTO Stage.Dir (DirName) Values ('" + currentDir + "');";
                myCommand.ExecuteNonQuery();
                //Console.WriteLine(myCommand.CommandText);

            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder or file. It may or may not be acceptable 
            // to ignore the exception and continue enumerating the remaining files and 
            // folders. It is also possible (but unlikely) that a DirectoryNotFound exception 
            // will be raised. This will happen if currentDir has been deleted by
            // another application or thread after our call to Directory.Exists. The 
            // choice of which exceptions to catch depends entirely on the specific task 
            // you are intending to perform and also on how much you know with certainty 
            // about the systems on which this code will run.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            string[] files = null;
            try
            {
                files = System.IO.Directory.GetFiles(currentDir);
            }

            catch (UnauthorizedAccessException e)
            {

                Console.WriteLine(e.Message);
                continue;
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            // Perform the required action on each file here.
            // Modify this block to perform your required task.
            foreach (string file in files)
            {
                try
                {
                    // Perform whatever action is required in your scenario.
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);

                    //Console.WriteLine("{0}: {1}, {2}, {3}, {4}", fi.Name, fi.Length, fi.CreationTime, fi.LastWriteTime, currentDir);
                    myCommand.CommandText = "INSERT INTO Stage.[File] ([FileName],[FileLength],[FileCreationTime],[FileModifiedTime], [DirName])" +
                        " Values ('" + fi.Name + "'," + fi.Length + ",'" + fi.CreationTime.ToString("yyyy.MM.dd HH_mm_ss").Replace("_", ":") + "','" + fi.LastWriteTime.ToString("yyyy.MM.dd HH_mm_ss").Replace("_",":") + "','" + currentDir + "');";
                    myCommand.ExecuteNonQuery();
                    //Console.WriteLine(myCommand.CommandText);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    // If file was deleted by a separate application
                    //  or thread since the call to TraverseTree()
                    // then just continue.
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

            // Push the subdirectories onto the stack for traversal.
            // This could also be done before handing the files.
            foreach (string str in subDirs)
                dirs.Push(str);
        }

        try
        {
            myConnection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }
}

