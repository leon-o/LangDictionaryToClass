using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using Path = System.IO.Path;
using ResourceDictionary = System.Windows.ResourceDictionary;

namespace ResourceDictionaryToClass
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            resourceText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xaml");
            classText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string inputFolderStr = this.inputFolder.Text;
            string outputFolderStr = this.outputFolder.Text;
            if (!Directory.Exists(inputFolderStr))
            {
                MessageBox.Show(inputFolderStr + "不存在");
                return;
            }
            if (!Directory.Exists(outputFolderStr))
            {
                MessageBox.Show(outputFolderStr + "不存在");
                return;
            }
            IEnumerable<string> enumerateFiles = Directory.EnumerateFiles(inputFolderStr);
            foreach (var file in enumerateFiles)
            {
                if (Path.GetExtension(file) == ".xaml")
                {
                    FileStream fs=new FileStream(file,FileMode.Open);
                    StreamReader reader=new StreamReader(fs);
                    var readToEnd = reader.ReadToEnd();
                    var classStr = Transform(readToEnd);
                    reader.Close();
                    fs.Close();
                    fs=new FileStream(Path.Combine(outputFolderStr,Path.GetFileNameWithoutExtension(file)+".cs"),FileMode.Create);
                    StreamWriter writer=new StreamWriter(fs);
                    writer.Write(classStr);
                    writer.Close();
                    fs.Close();
                }
            }

            MessageBox.Show("成功");
        }

        private string Transform(string input)
        {
            try
            {
                var strReader = new StringReader(input);
                var xmlReader = new XmlTextReader(strReader);
                var builder = new StringBuilder();
                if (System.Windows.Markup.XamlReader.Load(xmlReader) is ResourceDictionary dic)
                {
                    foreach (var dicKey in dic.Keys)
                    {
                        string value = dic[dicKey] as string;
                        builder.AppendFormat(propertyTemplate, dicKey.ToString(), value);
                    }
                }

                String result = classTemplate.Replace("{0}", builder.ToString());
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }

            
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            classText.Text = Transform(resourceText.Text);
        }

        private string classTemplate = @"
using System;
using System.Windows;

namespace LauncherX.Class.Helper
{
    public static class LangHelper
    {
        public static String GetStr(string key)
        {
            return Application.Current.Resources.MergedDictionaries[3][key].ToString();
        }
        {0}

    }
}
";

        private string propertyTemplate = @"
        /// <summary>
        /// {1}
        /// </summary>
        public static string {0} => GetStr(nameof({0}));
";

    }
}
