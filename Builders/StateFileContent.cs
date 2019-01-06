using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TerraformCloudHelper.Commands
{
   public class StateFileContent: IEnumerable<string>
    {
        private  string filepath;

        public StateFileContent(string filePaths)
        {
            this.filepath = filePaths;
        }


        public IEnumerator<string> GetEnumerator()
        {
            string[] files;
            if (filepath == null || Directory.Exists(filepath))
            {
                files = Directory.EnumerateFiles(".", "*.tfstate").ToArray();
            }
            else
            {
                files = new[] { filepath };
            }

            foreach (var f in files)
            {
                yield return File.ReadAllText(f);
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
