using System.Text;
using NiceIO;
using Unity.CodeEditor;
using UnityEngine;

namespace UnityZed
{
    public class ZedProcess
    {
        private static readonly ILogger sLogger = ZedLogger.Create();

        private readonly NPath m_ExecPath;
        private readonly NPath m_ProjectPath;
        private readonly NPath m_FlakePath;
        private readonly NPath m_NixPath;

        public ZedProcess(string execPath)
        {
            m_ExecPath = execPath;
            m_ProjectPath = new NPath(Application.dataPath).Parent;
            m_FlakePath = m_ProjectPath.Combine("flake.nix");
            m_NixPath = new NPath("/run/current-system/sw/bin/nix");
        }

        public bool OpenProject(string filePath = "", int line = -1, int column = -1)
        {
            sLogger.Log("OpenProject");

            var execCalled = m_ExecPath;

            var args = new StringBuilder();

            // Add flags for running `nix develop` if a flake exists
            if (m_FlakePath.FileExists() && m_NixPath.FileExists()) {
                execCalled = m_NixPath;
                args.Append("develop ");
                args.Append($"\"{m_ProjectPath.ToString()}\"");
                args.Append(" --command ");
                args.Append($"\"{m_ExecPath.ToString()}\"");
                args.Append(" -- ");
            }

            // always add project path
            args.Append($"\"{m_ProjectPath}\"");

            // if file path is provided, add it too
            if (!string.IsNullOrEmpty(filePath))
            {
                args.Append(" -a ");
                args.Append($"\"{filePath}\"");

                if (line >= 0)
                {
                    args.Append(":");
                    args.Append(line);

                    if (column >= 0)
                    {
                        args.Append(":");
                        args.Append(column);
                    }
                }
            }

            return CodeEditor.OSOpenFile(execCalled.ToString(), args.ToString());
        }
    }
}
