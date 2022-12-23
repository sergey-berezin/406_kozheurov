using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using kps7_1;
using System.Threading;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using System.ComponentModel;
using System.Windows.Markup;

namespace WpfApp1
{
    public class EnumerableConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var val = (IOrderedEnumerable < KeyValuePair<string, float> > )value;
                var list = val.ToList();
                string result = "";
                foreach (var i in list)
                {
                    result += i.Key + " : " + i.Value + " \n";
                }
                return result;
            }
            catch (Exception ex)
            {
                return "__";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class emo
    {
        public string name { get; set; }
        public string image { get; set; }
        public IOrderedEnumerable<KeyValuePair<string, float>> data { get; set; }
        public emo( string name, string image)
        {
            this.name = name;
            this.image = image;
        }
    }

    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _type.GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).Select(x => x.Description).ToList();
        }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public List<bool> sort { get; private set; } = new List<bool> { true, false, false, false, false, false, false, false };
        private emotion session = new emotion();
        private ObservableCollection<emo> image_list = new ObservableCollection<emo>();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private int _barFill = 0;
        public int barFill
        {
            get
            {
                return _barFill;
            }
            set
            {
                _barFill = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(barFill)));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Image_List.ItemsSource = image_list;


        }
        private async Task ProceedImages(string image)
        {
            try
            {
                var tmp = image.Split("\\");
                var name = tmp[tmp.Length - 1];
                var task = await session.EmotionAsync(Image.Load<Rgb24>(image), cts.Token);
                var photo_obj = new emo(name, image);
                var rt = Task.Run(() =>
                    session.EmotionAsync(Image.Load<Rgb24>(image), cts.Token));
                var sort_rt = await rt;
                var sort = sort_rt.OrderByDescending(rt => rt.Value);
                photo_obj.data = sort;
                image_list.Add(photo_obj);
            }
            catch (TaskCanceledException ae ){ }
        }

        public async void Open_Click(object sender, RoutedEventArgs? e = null)
        {
            cts = new CancellationTokenSource();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Images (*.jpg, *.png)|*.jpg;*.png";
            var projectRootFolder = System.IO.Path.GetFullPath("../../../../Images");
            ofd.InitialDirectory = projectRootFolder;
            var response = ofd.ShowDialog();
            if (response == true)
            {
                barFill = 0;
                ProgressBar.Maximum = ofd.FileNames.Length;
                foreach (var image in ofd.FileNames)
                {
                    try
                    {
                        await ProceedImages(image);
                        barFill += 1;
                    }
                    catch (AggregateException ae) { }
                }
            }
        }

        public void Stop_Click(object sender, RoutedEventArgs? e = null)
        {
            try
            {
                cts.Cancel();
            }
            catch(AggregateException ae)
            {
            }
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            List<string> list_emotions = new List<string> { "happiness", "neutral", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            var num_emotion = sort.FindIndex(x => x == true);
            var str_emotion = list_emotions[num_emotion];
            image_list = new ObservableCollection<emo>(image_list.OrderBy(x => -x.data.ToList()[x.data.ToList().FindIndex(i => i.Key == str_emotion)].Value));
            Image_List.ItemsSource = image_list;
        }
    }
}
