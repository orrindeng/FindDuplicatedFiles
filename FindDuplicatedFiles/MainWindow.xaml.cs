using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FindDuplicatedFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            string repositoryPath = name;
            if (!name.Contains(":"))
            {
                repositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ForguncyCollaboration", name);
            }
            
            if (!Directory.Exists(repositoryPath))
            {
                ResultTextBox.AppendText($"目录 {repositoryPath} 不存在。");
                return;
            }

            var subFolders = Directory.EnumerateDirectories(repositoryPath);

            foreach (var subFolder in subFolders)
            {
                Task.Run(() =>
                {
                    var files = Directory.EnumerateFiles(subFolder, "*", SearchOption.AllDirectories);

                    var grouping = files.GroupBy(f => Path.GetFileName(f));

                    foreach (var g in grouping)
                    {
                        if (g.Count() > 1)
                        {
                            var appendText = string.Join(Environment.NewLine, g);
                            
                            if (!string.IsNullOrEmpty(appendText))
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    ResultTextBox.AppendText($"{Environment.NewLine}----------{Environment.NewLine}");
                                    ResultTextBox.AppendText(appendText);
                                });
                            }
                        }
                    }
                });
            }
        }

        private void FindDuplicatedPlugins_Click(object sender, RoutedEventArgs e)
        {
            var path = @$"C:\Users\Public\Documents\ForguncyPlugin{DesignerVersion.Text}";
            if (!Directory.Exists(path))
            {
                FindDuplicatedPluginsResultTextBox.AppendText($"目录 {path} 不存在。{Environment.NewLine}");
                path = Path.GetFullPath(DesignerVersion.Text);
                if (!Directory.Exists(path))
                {
                    FindDuplicatedPluginsResultTextBox.AppendText($"目录 {path} 不存在。{Environment.NewLine}");
                    return;
                }
            }

            FindDuplicatedPluginsResultTextBox.AppendText($"查找开始{Environment.NewLine}");

            var pluginConfigs = Directory.EnumerateFiles(path, "PluginConfig.json", SearchOption.AllDirectories).SelectMany(s =>
            {
                var file = File.ReadAllText(s);
                try
                {
                    var obj = JsonConvert.DeserializeObject<JObject>(file);
                    if (obj?.TryGetValue("assembly", out var assemblyObj) == true)
                    {
                        var list = assemblyObj.ToObject<List<string>>();
                        if (list != null)
                        {
                            return list.Select(item => (item, s));
                        }
                    }
                }
                catch (Exception ex)
                {
                    FindDuplicatedPluginsResultTextBox.AppendText($"读取PluginConfig失败，路径：{s}，异常：{ex.Message}{Environment.NewLine}");
                }
                return new List<(string, string)>();
            }).ToList();

            var groupedItems = pluginConfigs.GroupBy(p => p.item, p => p.s);
            foreach (var groupedItem in groupedItems)
            {
                if (groupedItem.Count() > 1)
                {
                    FindDuplicatedPluginsResultTextBox.AppendText($"----------------------------------{Environment.NewLine}");
                    FindDuplicatedPluginsResultTextBox.AppendText($"{groupedItem.Key} 重复了，重复的有：{Environment.NewLine}");
                    foreach (var item in groupedItem)
                    {
                        FindDuplicatedPluginsResultTextBox.AppendText(item + Environment.NewLine);
                    }
                }
            }


            FindDuplicatedPluginsResultTextBox.AppendText($"查找完毕{Environment.NewLine}");
        }
    }
}
