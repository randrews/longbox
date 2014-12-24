using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Longbox
{
    interface IPageLoader
    {
        Task<Image> LoadPage(int num);
        void OpenComic(string filename);
        int NumPages { get; }
    }
}
