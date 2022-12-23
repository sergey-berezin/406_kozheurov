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
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using System.Text;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.Data;

namespace WpfApp1
{

    public class EnumerableConvert : IValueConverter
    {
        public object Convert(object emota, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Dictionary<string, float> res = new Dictionary<string, float>();
                var value = Encoding.ASCII.GetString((byte[])emota).Split('|');
                for (var i = 0; i < 8; i++)
                {
                    string key;
                    float num;
                    key = value[i * 2];
                    num = float.Parse(value[i * 2 + 1]);
                    res.Add(key, num);
                }
                var con_val =  res.OrderByDescending(rt => rt.Value);
                var val = (IOrderedEnumerable < KeyValuePair<string, float> > )con_val;
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
    internal class DBContext : DbContext
    {
        public DbSet<emo> emo { get; set; }
        public DbSet<ImageDetails> Details { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o) =>
            o.UseSqlite("Data Source=Processed_Images.db");
        public DBContext()
        {
            Database.EnsureCreated();
        }
    }
    public class emo
    {
        static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        private static Byte[] ToArray(Image image)
        {
            var str = image.ToString();
            return Encoding.ASCII.GetBytes(str);
        }
        [Key]
        public int imageId { get; set; }
        public string hash { get; set; }
        public string name
        {
            get
            {
                var list = image.Split('\\');
                return list[list.Length-1];
            }
        }
        public string image { get; set; }
        public ImageDetails Details { get; set; }
        public emo(string image)
        {
            this.image = image;
            this.Details = new ImageDetails();
            var pk = Image.Load<Rgb24>(image);
            HashAlgorithm hash_func = MD5.Create();
            byte[] Hash(byte[] pk) => hash_func.ComputeHash(pk);
            this.hash = ByteArrayToString(Hash(ToArray(pk)));

        }
    }

    public class ImageDetails
    {
        [Key]
        [ForeignKey(nameof(emo))]
        public int Id { get; set; }
        public byte[] emota { get; set; }
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
        IOrderedEnumerable<KeyValuePair<string, float>> data_get(byte[] emota)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            var value = Encoding.ASCII.GetString(emota).Split('|');
            for (var i = 0; i < 8; i++)
            {
                string key;
                float num;
                key = value[i * 2];
                num = float.Parse(value[i * 2 + 1]);
                result.Add(key, num);
            }
            return result.OrderByDescending(rt => rt.Value);
        }

        byte[] data_set(IOrderedEnumerable<KeyValuePair<string, float>> value)
        {
            string result = "";
            for (var i = 0; i < 8; i++)
            {
                result += value.ToList<KeyValuePair<string, float>>()[i].Key.ToString() + '|';
                result += value.ToList<KeyValuePair<string, float>>()[i].Value.ToString() + '|';
            }
            return Encoding.ASCII.GetBytes(result);
        }

        string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        public static Byte[] ToArray(Image image)
        {
            var str = image.ToString();
            return Encoding.ASCII.GetBytes(str);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public List<bool> sort { get; private set; } = new List<bool> { true, false, false, false, false, false, false, false };
        private emotion session = new emotion();
        private ObservableCollection<emo> image_list = new ObservableCollection<emo>();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private int _barFill = 0;
        public bool open_flag = false;

        private HashAlgorithm hash_func = MD5.Create();
        private byte[] Hash(byte[] pk) => hash_func.ComputeHash(pk);
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

        private void LoadDB()
        {
            using (var db = new DBContext())
            {
                db.Database.ExecuteSqlRaw("DROP TABLE \"emo\"");
                db.Database.ExecuteSqlRaw("DROP TABLE \"Details\"");
                db.Database.ExecuteSqlRaw("CREATE TABLE \"emo\"(\"imageId\" INTEGER NOT NULL CONSTRAINT \"PK_emo\" PRIMARY KEY AUTOINCREMENT,\"image\" TEXT NULL,\"hash\" TEXT NOT NULL,\"DetailsId\" INTEGER NOT NULL, CONSTRAINT \"FK_emo_Details_DetailsId\" FOREIGN KEY(\"DetailsId\") REFERENCES \"Details\"(\"Id\") ON DELETE CASCADE);");
                db.Database.ExecuteSqlRaw("CREATE TABLE \"Details\" (\"Id\" INTEGER NOT NULL CONSTRAINT \"PK_Details\" PRIMARY KEY AUTOINCREMENT, \"emota\" BLOB NOT NULL);");
                /*var images = db.emo.ToList();

                var a = new ObservableCollection<emo>(images);
                image_list = a;*/
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //LoadDB();
            Image_List.ItemsSource = image_list;

        }
        private async Task ProceedImages(string image)
        {
            try
            {
                var pk = Image.Load<Rgb24>(image);
                var hash = ByteArrayToString(Hash(ToArray(pk)));
                using (DBContext db = new DBContext())
                {
                    if (db.emo.Any(x => Equals(x.hash, hash)))
                    {
                    var query = db.emo.Where(x => Equals(x.hash, hash));
                        foreach (var item in query)
                        {
                            var imag = item;
                            var det = db.Details.First(i => i.Id == imag.imageId);
                            imag.Details = det;
                            imag.image = image;
                            image_list.Add(imag);
                        }
                        return;
                    }
                    else
                    {
                        var rt = Task.Run(() =>
                                session.EmotionAsync(Image.Load<Rgb24>(image), cts.Token));
                        var task = await session.EmotionAsync(Image.Load<Rgb24>(image), cts.Token);
                        var photo_obj = new emo(image);
                        var sort_rt = await rt;
                        var sort = sort_rt.OrderByDescending(rt => rt.Value);
                        photo_obj.Details.emota = data_set(sort);
                        Thread.Sleep(1000);
                        image_list.Add(photo_obj);
                        db.emo.Add(photo_obj);
                        db.Details.Add(photo_obj.Details);
                        db.SaveChanges();
                    }
                }
            }
            catch (TaskCanceledException ae){ }
        }

        public async void Open_Click(object sender, RoutedEventArgs? e = null)
        {
            if (open_flag == false)
            {
                open_flag = true;
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
                open_flag = false;
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

        public void Delete_Click(object sender, RoutedEventArgs? e = null)
        {
            var i = Image_List.SelectedIndex;
            var delete_image = (emo)Image_List.SelectedItem;
            if (i != -1)
            {
                while (image_list.Any(x => x.hash == delete_image.hash))
                {
                    var a = image_list.First(x => x.hash == delete_image.hash);
                    image_list.Remove(a);
                }
                using (var db = new DBContext())
                {
                    db.emo.Remove(delete_image);
                    db.SaveChanges();
                }
            }
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            List<string> list_emotions = new List<string> { "happiness", "neutral", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            var num_emotion = sort.FindIndex(x => x == true);
            var str_emotion = list_emotions[num_emotion];
            image_list = new ObservableCollection<emo>(image_list.OrderBy(x => -data_get(x.Details.emota).ToList()[data_get(x.Details.emota).ToList().FindIndex(i => i.Key == str_emotion)].Value));
            Image_List.ItemsSource = image_list;
        }
    }
}
