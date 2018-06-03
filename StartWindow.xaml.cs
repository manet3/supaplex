using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.Windows.Shapes.Path;

namespace Supaplex
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window, INotifyPropertyChanged
    {
        private LevelTile _selectedTile;
        private LevelTile[][] _availableLvl;
        private List<LevelTile> _levels;
        public LevelTile[][] AvailableLvl
        {
            get { return _availableLvl; }
            set
            {
                _availableLvl = value; 
                NotifyPropertyChanged("AvailableLvl");
            }
        }

        public Visibility FolderVisibility { get; set; }

        public LevelTile SelectedTile
        {
            get { return _selectedTile; }
            set
            {
                if (value != null)
                    _selectedTile = value;
            }
        }

        public StartWindow():this(true)
        {
        }

        public StartWindow(bool isFolderVisible)
        {
            FolderVisibility = isFolderVisible?Visibility.Visible:Visibility.Collapsed;
            GetLevelTiles();
            InitializeComponent();
            DataContext = this;
            MainMenuParameters.DefaultLevel = null;
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            SetParams();
            MainMenuParameters.Mode = GameMode.PlayMode;
            NewWindow(new PlayField());
        }

        private void LvlBuild(object sender, RoutedEventArgs e)
        {
            SetParams();
            NewWindow(new ConstructWindow());
        }

        private void NewWindow(Window window)
        {
            var current = Application.Current.MainWindow;
            window.Show();
            window.Activate();
            current.Close();
            Application.Current.MainWindow = window;
        }

        private void SetParams()
        {
            MainMenuParameters.Levels = new List<string>();
            foreach (var levelTiles in AvailableLvl)
            {
                foreach (var levelTile in levelTiles)
                {
                    MainMenuParameters.Levels.Add(levelTile.Name);                  
                }
            }
            if (MainMenuParameters.DefaultLevel == null)
                MainMenuParameters.LevelNumb =
                    int.Parse(SelectedTile != null 
                    ? SelectedTile.Numb 
                    : AvailableLvl[0][0].Numb) - 1;
        }

        private void PlayAutocomp(object sender, RoutedEventArgs e)
        {
            SetParams();
            MainMenuParameters.Mode = GameMode.AiMode;
            NewWindow(new PlayField());
        }

        public void GetLevelTiles()
        {
            int cntr = 1;
            _levels = new List<LevelTile>();

            string[] lvlNames;
            try
            {
                lvlNames = File.ReadAllLines(MainMenuParameters.LvlOrderFilePath, Encoding.UTF8);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory("Resources//Levels");
                File.Create("Resources//Levels//consequence");
                File.WriteAllBytes("Resources//Levels//default", Properties.Resources._default);
                return;
            }

            foreach (string name in lvlNames)
            {
                _levels.Add(new LevelTile(cntr, name));
                cntr++;
            }
            if (TileScrollViewer == null)
            {
                StructTiles(8);
                return;
            }
            StartWindow_OnSizeChanged(null, null);
        }

        public void StructTiles(int width)
        {
            var lvlMatrics = new List<LevelTile[]>();
            var lvlRow = new List<LevelTile>();
            foreach (var lvlTile in _levels)
            {
                lvlRow.Add(lvlTile);
                if (lvlRow.Count == width || lvlTile == _levels.Last())
                {
                    lvlMatrics.Add(lvlRow.ToArray());
                    lvlRow.Clear();
                }
            }
            AvailableLvl = lvlMatrics.ToArray();
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            listView.SelectedIndex = -1;
        }

        private void NotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void StartWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var newWidth = TileScrollViewer.ActualWidth - 100;
            if (newWidth % 100 >= 0 && newWidth % 100 <= 60)
            {
                StructTiles((int)newWidth / 100);
            }
        }

        private void DeleteLevel(object sender, RoutedEventArgs e)
        {
            File.Delete("Levels/" + SelectedTile.Name + ".lvl");
            var lvlNames = File.ReadAllLines(MainMenuParameters.LvlOrderFilePath, Encoding.UTF8).ToList();
            if (lvlNames.Contains(SelectedTile.Name))
                lvlNames.Remove(SelectedTile.Name);
            File.WriteAllLines(MainMenuParameters.LvlOrderFilePath, lvlNames, Encoding.UTF8);
            GetLevelTiles();
        }

        private void OpenGuide(object sender, RoutedEventArgs e)
        {
            var window = new HelpWindow();
            window.Show();
            window.Activate();

        }

        private void StartWindow_OnStateChanged(object sender, EventArgs e)
        {
            StructTiles((int)TileScrollViewer.ActualWidth / 100 - 1);
        }

        private void AddLevelEditor(object sender, RoutedEventArgs e)
        {
            MainMenuParameters.DefaultLevel = "default";
            SetParams();
            NewWindow(new ConstructWindow());
        }
    }

    public class LevelTile
    {
        public string Numb { get; set; }

        public string Name { get; set; }

        public LevelTile(int numb, string name)
        {
            Numb = numb.ToString();
            Name = name;
        }
    }
}
