using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Supaplex
{
    class AiModel
    {
        public static ModelManager ModelUsage;
        public List<Vector> SelectedPath;
        public AiModel(GameModel model)
        {
            ModelUsage = new ModelManager(model);
            SelectedPath = new List<Vector>();
        }

        public void ConvertPath()
        {
            var pathBuilder = new PathTreeBuilder();
            var path = pathBuilder.GetFinalPath();//getting branches
            var pathTree = pathBuilder.MajorBranches;
            //log output
            Directory.CreateDirectory("logs");
            using (var fs = new StreamWriter("logs/debug.txt", false, Encoding.UTF8))
            {
                for (int i = 0; i < pathTree.Count; i++)
                {
                    var pathBranch = pathTree[i];
                    fs.Write(@"<BRANCH START {0}>  ", i);
                    var pnts = new List<string>();
                    foreach (var point in pathBranch.ExceptPoints)
                    {
                        if (i == 0) break;
                        pnts.Add(string.Format("{0},{1}", point.X, point.Y));
                    }
                    fs.WriteLine(string.Join(" | ", pnts));

                    foreach (var point in pathBranch.AllPath)
                    {
                        fs.WriteLine(@"{0},{1}", point.Location.X, point.Location.Y);
                    }
                    fs.WriteLine(@"<BRANCH END {0}>", i);
                    fs.WriteLine();
                }
            }
            //converting to directions cosequence
            for (int i = 1; i < path.Count; i++)
            {
                SelectedPath.Add(path[i].Location - path[i - 1].Location);
            }
        }
    }
}
