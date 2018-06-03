using System.IO;
using System.Text;
using System.Windows;
using System.Reflection;

namespace Supaplex
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            var stream = new MemoryStream(Encoding.Unicode.GetBytes(Properties.Resources.Help));
            rtfBox.Selection.Load(stream, DataFormats.Rtf);
        }

    }
}
