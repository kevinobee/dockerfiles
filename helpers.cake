#addin nuget:?package=Cake.Git

public void PrintLastCommitChanges()
{
    var lastCommit = GitLogTip(".");

    Information("Last commit sha -> {0}", lastCommit.Sha);

    var gitDiffFiles = GitDiff(".", lastCommit.Sha);

    foreach(var gitDiffFile in gitDiffFiles)
    {
        Information("File change detected -> {0}", gitDiffFile);
    }
}

public List<string> GetLastCommitChanges(string checkFile)
{
    List<string> cc = new List<string>();

    var lastCommit = GitLogTip(".");

    var gitDiffFiles = GitDiff(".", lastCommit.Sha);

    foreach(var gitDiffFile in gitDiffFiles)
    {
        bool isDirectory = !string.IsNullOrWhiteSpace(System.IO.Path.GetDirectoryName(gitDiffFile.Path));

        if (isDirectory)
        {
            string dockerDirectory = System.IO.Path.GetDirectoryName(gitDiffFile.Path);

            if (!cc.Contains(dockerDirectory))
            {
                cc.Add(dockerDirectory);
            }
        }
    }

    return cc;
}

public void ExecuteScript(string[] scripts)
{
    var scriptRunner =
        AppVeyor.IsRunningOnAppVeyor ? "powershell" :
        TravisCI.IsRunningOnTravisCI ? "bash" : "powershell";

    foreach(string script in scripts)        
    {
        if (System.IO.File.Exists(script))
        {
            Information("Starting to execute file -> {0}", script);

            int ec = 0;

            using(var process = StartAndReturnProcess(scriptRunner, new ProcessSettings{ Arguments = script }))
            {
                process.WaitForExit();

                Information("Exit code -> {0}", process.GetExitCode());

                ec = process.GetExitCode();
            }

            if (ec != 0)
            {
                throw new Exception(String.Format("Running script {0} exited with non-zero return code {1}", script, ec));   
            }
        }
    }
}