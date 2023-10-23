#region ------------Infomation------------
/*----------------------------------------------------------------
 * 模仿python中pathlib包进行优雅的路径处理
 *----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace UtilityTools.Core.Helper
{
    public class PathHelper
    {
        public PathHelper(string path)
        {
            _path = Path.GetFullPath(path);
            _isRoot = Path.GetDirectoryName(_path) == null;
        }

        public string stem { get { return Path.GetFileNameWithoutExtension(_path); } }
        public string name { get { return Path.GetFileName(_path); } }
        public PathHelper parent { get { return _isRoot ? this : new PathHelper(Path.GetDirectoryName(_path)); } }

        public List<PathHelper> parents
        {
            get
            {
                if (_isRoot) return new List<PathHelper>() { };
                var ret = new List<PathHelper>() { parent };
                var p = parent;
                while (!p._isRoot)
                {
                    p = p.parent;
                    ret.Add(p);
                }

                return ret;
            }
        }

        public void MakeDir()
        {
            if (!Directory.Exists(_path)) { Directory.CreateDirectory(_path); }
        }

        public static PathHelper operator /(PathHelper a, PathHelper b)
        {
            return new PathHelper(Path.Combine(a._path, b._path));
        }

        public static PathHelper operator /(PathHelper a, string b)
        {
            return new PathHelper(Path.Combine(a._path, b));
        }
        public override string ToString()
        {
            return _path;
        }

        public static implicit operator string(PathHelper p)
        {
            return p._path;
        }

        private string _path;
        private bool _isRoot;
    }
}
