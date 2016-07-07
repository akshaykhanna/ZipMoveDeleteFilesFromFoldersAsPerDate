using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ionic.Zip;
using System.IO;
using System.Text.RegularExpressions;

namespace Hmail_Server_Utility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string msg;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            String source = tb_S.Text;
            String dest = tb_D.Text;
            String zip_name = tb_zip_name.Text;
            
            int timeDays= Convert.ToInt32(tb_time.Text);
            int timeHrs = Convert.ToInt32(tb_hours.Text);
            int timeMins = Convert.ToInt32(tb_minutes.Text);
            //imp step
            /************ TimeSpan(int days, int hours, int minutes, int seconds) ****/
            var timeThreshold = DateTime.Now - new TimeSpan(timeDays, timeHrs, timeMins, 0);

            //IEnumerable<string> subdirectoryEntries = Directory.GetDirectories(source);

            var subdirectoryEntries = lb_folders_to_zipped.Items;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want files of "+ subdirectoryEntries.Count +" folders which are older than " +timeDays+" days, "+timeHrs+ "hrs and " + timeMins+ "mins (" + timeThreshold.ToShortDateString() +" : " +timeThreshold.ToShortTimeString()+") to be ziped, moved and then deleted?", "Zip Move & Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    if (!subdirectoryEntries.IsEmpty)
                    {
                        msg = "";
                        foreach (string subdirectory in subdirectoryEntries)
                        {
                            string sudDirNameWhereZipToBeMoved = subdirectory.Substring(subdirectory.LastIndexOf(@"\") + 1);
                            msg += sudDirNameWhereZipToBeMoved + ": ";
                            IEnumerable<string> fileEntries = Directory.GetFiles(subdirectory);
                            if (fileEntries.Any())
                            {
                                List<string> fileEntriesToBeZippedAndDeleted = new List<string>();
                                foreach (string fileName in fileEntries)
                                {
                                    FileInfo fi = new FileInfo(fileName);
                                    var creationTime = fi.CreationTime;

                                    //imp step
                                    if (creationTime < timeThreshold)
                                    {
                                        fileEntriesToBeZippedAndDeleted.Add(fileName);
                                    }
                                }
                                if (fileEntriesToBeZippedAndDeleted.Any())
                                {

                                    //zipping files
                                    String sourceOfZipFile;
                                    using (ZipFile myZip = new ZipFile())
                                    {
                                        myZip.AddFiles(fileEntriesToBeZippedAndDeleted, "");
                                        sourceOfZipFile = subdirectory + "\\" + zip_name + ".zip";
                                        myZip.Save(sourceOfZipFile);
                                    }
                                    msg = msg + "Files ziped. ";
                                    //deleting files
                                    foreach (string fileName in fileEntriesToBeZippedAndDeleted)
                                    {
                                        File.Delete(fileName);
                                    }
                                    msg = msg + "Files deleted. ";
                                    //moving zip file to other direcrtory

                                    string destOfZipFile = tb_D.Text + "\\" + sudDirNameWhereZipToBeMoved + "\\" + zip_name + ".zip";
                                    moveFile(sourceOfZipFile, destOfZipFile);
                                    msg = msg + "Zip Moved. ";
                                }
                                else
                                {
                                    msg = msg + "No file required to be managed. ";
                                }
                            }
                            else
                            {
                                msg = msg + "Folder empty. ";
                            }
                            msg = msg + " \n";
                        }
                        MessageBoxResult result = MessageBox.Show(msg, "Eexcution Log");
                        lb_display_folders.ItemsSource = null;
                        lb_folders_to_zipped.ItemsSource = null;
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("No folder is added to  be managed", "Right list box empty");
                    }
                }
                catch (Exception f)
                {
                    MessageBoxResult result = MessageBox.Show("Some error occured! "+f.Message, "Error:");
                }
            }
            
            
        }

    void moveFile(string source, string dest)
    {
       
        // To move a file or folder to a new location:
        System.IO.File.Move(source, dest);
    }

        private void btn_get_folders_Click(object sender, RoutedEventArgs e)
        {
            String source = tb_S.Text;
            IEnumerable<string> subdirectoryEntries = Directory.GetDirectories(source);
            lb_display_folders.ItemsSource = subdirectoryEntries;
        }

        private void btn_add_folders_Click(object sender, RoutedEventArgs e)
        {
           var subdirectoryEntries = lb_display_folders.SelectedItems;
            lb_folders_to_zipped.ItemsSource = subdirectoryEntries;
        }
    }
}
