﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using SmartDots.Helpers;
using SmartDots.Model;
using SmartDots.Model.Extension;
using SmartDots.View;
using File = SmartDots.Model.File;
using MessageBox = System.Windows.MessageBox;
using System.Net;
using System.Windows.Controls;
using Line = System.Windows.Shapes.Line;
using DevExpress.Utils;
using System.Dynamic;

namespace SmartDots.ViewModel
{
    public class LarvaeFileViewModel : LarvaeBaseViewModel
    {
        private ObservableCollection<LarvaeFile> larvaeFiles;
        private LarvaeFile selectedFile;
        //private Folder currentFolder;
        private bool changingFile;
        //private bool needsSampleLink;
        //private bool canDetach;
        //private string nextFileLocation;
        //private string nextLocalFileLocation;
        //private bool loadingNextPicture;
        //private bool useSampleStatus;
        //private bool loadingfolder;
        private bool showNavButtons;
        //private string sampleNumberAlias;
        //private Visibility canAttachDetachSampleVisibility;
        private Visibility toolbarVisibility;

        public LarvaeFileViewModel()
        {
            //ShowNavButtons = Properties.Settings.Default.ShowFileNavButtons;
        }

        public ObservableCollection<LarvaeFile> LarvaeFiles
        {
            get
            {
                return larvaeFiles;
            }
            set
            {
                larvaeFiles = value;
                /*if (larvaeFiles != null && larvaeFiles.Any())*/ 
                SelectedFile = larvaeFiles?.FirstOrDefault();
                RaisePropertyChanged("LarvaeFiles");
            }
        }

        public LarvaeFile SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                RaisePropertyChanged("SelectedFile");
                if (LarvaeViewModel.LarvaeView != null)
                {
                    LoadFile();

                    //AgeReadingViewModel.AgeReadingAnnotationViewModel.Outcomes = selectedFile.BoundOutcomes ??
                    // new ObservableCollection<Annotation>();
                    //AgeReadingViewModel.AgeReadingEditorViewModel.OriginalMeasureShapes = new ObservableCollection<Line>();
                    //AgeReadingViewModel.AgeReadingEditorViewModel.TextShapes = new ObservableCollection<TextBlock>();
                    //AgeReadingViewModel.AgeReadingView.BrightnessSlider.EditValue = 0;
                    //AgeReadingViewModel.AgeReadingEditorViewModel.Brightness = 0;
                    //AgeReadingViewModel.AgeReadingView.ContrastSlider.EditValue = 0;
                    //AgeReadingViewModel.AgeReadingEditorViewModel.Contrast = 0;
                    //AgeReadingViewModel.AgeReadingEditorViewModel.ActiveCombinedLine = null;
                }
                //LarvaeViewModel.AgeReadingSampleViewModel.SetSample();
            }
        }

        public bool ChangingFile
        {
            get { return changingFile; }
            set { changingFile = value; }
        }

        public void LoadImage(string imagepath)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var bitmap = new BitmapImage();


            if (imagepath.StartsWith("http"))
            {
                Helper.Log("filetimer.txt", $"Start downloading image {imagepath}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);

                var buffer = new WebClient().DownloadData(imagepath);

                Helper.Log("filetimer.txt", $"Finished downloading image {imagepath}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);


                using (var stream = new MemoryStream(buffer))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
            }
            else
            {
                bitmap = new BitmapImage(new Uri(imagepath, UriKind.RelativeOrAbsolute))
                {
                    CacheOption = BitmapCacheOption.OnLoad
                };
            }

            Helper.Log("filetimer.txt", $"End bitmap conversion {imagepath}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);

            LarvaeViewModel.LarvaeEditorViewModel.LarvaeImage = bitmap;
            LarvaeViewModel.LarvaeEditorViewModel.OriginalImage = bitmap;
            LarvaeViewModel.LarvaeView.Opacity = 1;

            LarvaeViewModel.LarvaeStatusbarViewModel.IsFittingImage = true;

        }

        //public void SetNextPicture()
        //{
        //    try
        //    {
        //        Directory.CreateDirectory("temp");
        //        DirectoryInfo di = new DirectoryInfo("temp");
        //        foreach (FileInfo file in di.GetFiles())
        //        {
        //            try
        //            {
        //                System.IO.File.Delete("temp/" + file.Name);
        //            }
        //            catch (Exception e)
        //            {
        //                //current file will be locked
        //            }
        //        }
        //        //Load the image in a seperate thread
        //        var filename = Files.SkipWhile(item => item != SelectedFile).Skip(1).FirstOrDefault()?.Filename;
        //        var extension =
        //            Path.GetExtension(
        //                Files.SkipWhile(item => item != SelectedFile).Skip(1).FirstOrDefault()?.FullFileName);
        //        var path = "";

        //        if (currentFolder.Path.StartsWith("\\")) path = Path.Combine(CurrentFolder.Path, filename + extension);
        //        else
        //        {
        //            if (CurrentFolder.Path.EndsWith("/")) path = CurrentFolder.Path + SelectedFile.FullFileName;
        //            else
        //            {
        //                path = CurrentFolder.Path + "/" + SelectedFile.FullFileName;
        //            }
        //        }
        //        nextFileLocation = path;
        //        nextLocalFileLocation = "temp/" + filename + extension;
        //        if (!System.IO.File.Exists(nextLocalFileLocation)) CopyFileAsync(path, nextLocalFileLocation);
        //    }
        //    catch (Exception e)
        //    {
        //        // ignored
        //        loadingNextPicture = false;
        //    }
        //}

        //public bool FolderExists(string path)
        //{
        //    if (path.StartsWith("http"))
        //    {
        //        //todo have to implement
        //        return true;
        //    }
        //    else
        //    {
        //        return Directory.Exists(path);
        //    }
        //}

        //public async Task CopyFileAsync(string sourceFile, string destinationFile)
        //{
        //    loadingNextPicture = true;
        //    if (sourceFile.StartsWith("http"))
        //    {
        //        using (var client = new WebClient())
        //        {
        //            try
        //            {
        //                client.DownloadFile(sourceFile, destinationFile);
        //            }
        //            catch (Exception)
        //            {
        //                //
        //            }

        //        }
        //    }
        //    else
        //    {
        //        using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
        //        using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
        //            await sourceStream.CopyToAsync(destinationStream);
        //    }

        //    loadingNextPicture = false;
        //}

        public void UpdateList()
        {
            LarvaeViewModel.LarvaeFileView.LarvaeFileGrid.RefreshData();
        }

        //public void Attach()
        //{
        //    try
        //    {
        //        var analysisSamples = Global.API.GetAnalysisSamples(AgeReadingViewModel.Analysis.ID).Result;

        //        AgeReadingViewModel.AttachSampleDialog = new AttachSampleDialog(AgeReadingViewModel);
        //        AgeReadingViewModel.AttachSampleDialogViewModel =
        //            AgeReadingViewModel.AttachSampleDialog.AttachSampleDialogViewModel; //todo problem
        //        AgeReadingViewModel.AttachSampleDialogViewModel.Samples = analysisSamples;
        //        ShowDialog(AgeReadingViewModel.AttachSampleDialog);
        //        AgeReadingViewModel.AgeReadingAnnotationViewModel.RefreshActions();
        //        AgeReadingViewModel.AgeReadingEditorViewModel.UpdateButtons();

        //    }
        //    catch (Exception e)
        //    {
        //        Helper.ShowWinUIMessageBox("Error mapping SampleStates", "Error", MessageBoxButton.OK,
        //            MessageBoxImage.Error, e);
        //    }
        //}

        public void FileList_BeforeLayoutRefresh(object sender, CancelRoutedEventArgs e)
        {
            //if (AgeReadingViewModel.AgeReadingAnnotationViewModel.Outcomes.Any() &&
            //    !AgeReadingViewModel.AgeReadingFileViewModel.LoadingFolder &&
            //    AgeReadingViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation != null &&
            //    AgeReadingViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation?.QualityID == null &&
            //    !AgeReadingViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.IsFixed
            //    )
            //{
            //    //savechecks
            //    if (!AgeReadingViewModel.AgeReadingAnnotationViewModel.EditAnnotation())
            //        //AgeReadingViewModel.SaveAnnotations();
            //        return;
            //}

            //else
            //{
            //    AgeReadingViewModel.SaveAnnotations();
            //}
        }

        public void FileList_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (LarvaeViewModel.LarvaeFileView.FileList.FocusedRowData.Row != null)
            {
                var file = LarvaeFiles.FirstOrDefault(x => x.ID == ((dynamic)LarvaeViewModel.LarvaeFileView.FileList.FocusedRowData.Row).ID);
                if (file != null && SelectedFile != null && file.ID != SelectedFile.ID)
                {
                    SelectedFile = file;
                }
            }
            //LoadFile();
        }

        public void LoadFile()
        {
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            try
            {
                LarvaeViewModel.ShowWaitSplashScreen();
            }
            catch (Exception ex)
            {
            }

            ChangingFile = true;

            LarvaeFile file = SelectedFile;
            LarvaeViewModel.LarvaeEditorViewModel.Brightness = 0;
            LarvaeViewModel.LarvaeEditorViewModel.Contrast = 0;
            LarvaeViewModel.LarvaeEditorViewModel.RemoveShapes();



            if (file != null)
            {

                //file.StatusCode = temp.StatusCode;
                //sample.StatusColor = temp.StatusColor;
                //sample.StatusRank = temp.StatusRank;

                //var dynSample = DynamicSamples.FirstOrDefault(x => x.ID == sample.ID);

                //dynSample.StatusRank = sample.StatusRank;
                //dynSample.StatusColor = sample.StatusColor;
                //dynSample.StatusCode = sample.StatusCode;
                //dynSample.Status = sample.Status;

                //if (temp.SampleProperties != null)
                //{
                //    Dictionary<string, string> values = temp.SampleProperties;
                //    var columnNames = values.Keys.ToList();
                //    foreach (var column in columnNames)
                //    {
                //        ((IDictionary<string, string>)sample.SampleProperties)[column] = values[column];
                //        ((IDictionary<String, Object>)dynSample)[column] = values[column];
                //    }
                //}



                //SelectedSample = sample;

                UpdateList();

                //Helper.Log("filetimer.txt", $"Start loading image {file.Path}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);


                LoadImage(file.Path);

                //Helper.Log("filetimer.txt", $"End loading image{file.Path}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);

                LarvaeViewModel.LarvaeEditorViewModel.Mode = EditorModeEnum.Measure;

                //Helper.DoAsync(SetNextPicture);
                //AgeReadingViewModel.AgeReadingStatusbarViewModel.IsFittingImage = true;
            }
            else
            {
                if(LarvaeViewModel.LarvaeEditorViewModel.LarvaeImage != null) LarvaeViewModel.LarvaeEditorViewModel.LarvaeImage = null;
                if (LarvaeViewModel.LarvaeEditorViewModel.OriginalImage != null) LarvaeViewModel.LarvaeEditorViewModel.OriginalImage = null;
            }

            try
            {

                LarvaeViewModel.CloseSplashScreen();
                if (selectedFile != null && selectedFile.Scale != null && selectedFile.Scale != 0.0m)
                {
                    LarvaeViewModel.LarvaeStatusbarViewModel.Info = $"Scale (px/mm): {selectedFile.Scale.ToString()}";
                }
                else if (selectedFile == null)
                {
                    LarvaeViewModel.LarvaeStatusbarViewModel.Info = $"";
                }
                else
                {
                    LarvaeViewModel.LarvaeStatusbarViewModel.Info = $"Scale (px/mm): ?";
                }
                ChangingFile = false;
                RefreshNavigationButtons();

                //timer.Stop();

                //Helper.Log("filetimer.txt", $"Complete loading image{file.Path}: {timer.ElapsedMilliseconds} ms" + Environment.NewLine);

            }
            catch (Exception ex)
            {
            }
        }

        //public void btnAttach_Click(object sender, RoutedEventArgs e)
        //{
        //    Attach();
        //}

        //public void btnDetach_Click(object sender, RoutedEventArgs e)
        //{
        //    Guid fileid = SelectedFile.ID;

        //    var file = Global.API.GetFile(fileid, false, false);
        //    if (!file.Succeeded)
        //    {
        //        Helper.ShowWinUIMessageBox("Error loading File from Web API\n" + file.ErrorMessage, "Error", MessageBoxButton.OK,
        //            MessageBoxImage.Error);
        //        return;
        //    }
        //    file.Result.SampleID = null;
        //    file.Result.Sample = null;
        //    file.Result.SampleNumber = null;
        //    var updateFileResult = Global.API.UpdateFile(file.Result);
        //    if (!updateFileResult.Succeeded)
        //    {
        //        Helper.ShowWinUIMessageBox("Error saving File to Web API\n" + updateFileResult.ErrorMessage , "Error", MessageBoxButton.OK,
        //            MessageBoxImage.Error);
        //        return;
        //    }

        //    SelectedFile.SampleID = null;
        //    SelectedFile.Sample = null;
        //    SelectedFile.IsReadOnly = true;
        //    ((dynamic)AgeReadingViewModel.AgeReadingFileView.FileList.FocusedRowData.Row).SampleID = null;
        //    ((dynamic)AgeReadingViewModel.AgeReadingFileView.FileList.FocusedRowData.Row).Sample = null;
        //    ((dynamic)AgeReadingViewModel.AgeReadingFileView.FileList.FocusedRowData.Row).IsReadOnly = true;
        //    SelectedFile.FetchProps((dynamic)AgeReadingViewModel.AgeReadingFileView.FileList.FocusedRowData.Row);

        //    UpdateList();
        //    AgeReadingViewModel.AgeReadingSampleViewModel.Sample = null;
        //    AgeReadingViewModel.AgeReadingSampleViewModel.SetSample();
        //    AgeReadingViewModel.AgeReadingAnnotationViewModel.RefreshActions();
        //}




        //public void LoadImages()
        //{
        //    try
        //    {
        //        AgeReadingViewModel.ShowWaitSplashScreen();
        //        //todo check if real image

        //        ObservableCollection<dynamic> dynamicFiles = new ObservableCollection<dynamic>();
        //        List<File> filelist = new List<File>();

        //        List<string> fullImageNames = new List<string>();

        //        if (Global.API.Settings.ScanFolder)
        //        {
        //            if (CurrentFolder.Path.StartsWith("http"))
        //            {
        //                WebClient w = new WebClient();
        //                w.Headers["user-agent"] = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR1.0.3705;)";
        //                string s = w.DownloadString(CurrentFolder.Path);

        //                // 2.
        //                foreach (LinkItem i in LinkFinder.Find(s))
        //                {
        //                    fullImageNames.Add(i.Href.Split('/').Last());
        //                }
        //            }
        //            else
        //            {
        //                fullImageNames = Directory.EnumerateFiles(CurrentFolder.Path).ToList();

        //            }
        //            fullImageNames = fullImageNames.Where(file => file.ToLower().EndsWith(".tif") || file.ToLower().EndsWith(".jpg")
        //                                   || file.ToLower().EndsWith(".jpeg") || file.ToLower().EndsWith(".png") ||
        //                                   file.ToLower().EndsWith(".gif") || file.ToLower().EndsWith(".bmp")).ToList();
        //            List<string> tempImageNames = new List<string>();
        //            foreach (var image in fullImageNames)
        //            {
        //                tempImageNames.Add(image.Split('\\').Last());
        //            }
        //            fullImageNames = tempImageNames;
        //        }


        //        var filesResult = Global.API.GetFiles(AgeReadingViewModel.Analysis.ID, fullImageNames);
        //        if (!filesResult.Succeeded)
        //        {
        //            Helper.ShowWinUIMessageBox("Error loading Files from Web API\n" + filesResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }
        //        if (!Global.API.Settings.ScanFolder)
        //        {
        //            fullImageNames = ((List<DtoFile>) filesResult.Result).Select(x => x.Filename).ToList();
        //        }

        //        List<string> columnNames = new List<string>();
        //        AgeReadingViewModel.AgeReadingFileView.FileGrid.Dispatcher.Invoke(() =>
        //        {
        //            foreach (string image in fullImageNames)
        //            {
        //                var dtoFile = filesResult.Result.FirstOrDefault(x => x.Filename.ToLower() == image.ToLower());
        //                if (dtoFile == null)
        //                {
        //                    continue;
        //                }
        //                var file = (File)Helper.ConvertType(dtoFile, typeof(File));

        //                if(file.SampleProperties != null)
        //                {
        //                    Dictionary<string, string> values = file.SampleProperties.ToObject<Dictionary<string, string>>();
        //                    columnNames = values.Keys.ToList();
        //                }

        //                if (dtoFile.Sample != null) file.Sample = (Sample)Helper.ConvertType(dtoFile.Sample, typeof(Sample));
        //                if (file != null)
        //                {
        //                    file.Filename = file.Filename;
        //                    int index = fullImageNames.IndexOf(image);
        //                    file.FullFileName = fullImageNames[index];
        //                    file.FetchProps((dynamic)AgeReadingViewModel.AgeReadingFileView.FileList.FocusedRowData.Row);
        //                }

        //                dynamic dynFile = CreateDynamicFile(file);

        //                dynamicFiles.Add(dynFile);
        //                filelist.Add(file);
        //            }

        //            List<GridColumn> colsToDelete = new List<GridColumn>();
        //            foreach (var column in AgeReadingViewModel.AgeReadingFileView.FileGrid.Columns.Where(x => x.Tag != null && x.Tag.ToString() == "Dynamic"))
        //            {
        //                colsToDelete.Add(column);
        //            }

        //            foreach (var column in colsToDelete)
        //            {
        //                AgeReadingViewModel.AgeReadingFileView.FileGrid.Columns.Remove(column);
        //            }

        //            List<GridColumn> columns = new List<GridColumn>();

        //            columns.AddRange(columnNames.Select(columnName => new GridColumn() { FieldName = columnName, AllowSorting = DefaultBoolean.True, Tag = "Dynamic", AllowEditing = DefaultBoolean.False }));
        //            foreach (var col in columns)
        //            {
        //                AgeReadingViewModel.AgeReadingFileView.FileGrid.Columns.Add(col);
        //            }

        //            //if (filelist.Any())
        //            //{
        //            //    List<GridColumn> columns = new List<GridColumn>();
        //            //    columns.AddRange(columnNames.Select(columnName => new GridColumn() { FieldName = columnName, AllowSorting = DefaultBoolean.True, Tag = "Dynamic" }));
        //            //    foreach (var column in columns)
        //            //    {
        //            //        AgeReadingViewModel.AgeReadingFileView.FileGrid.Columns.Add(column);
        //            //    }
        //            //}


        //            Files = filelist;
        //            DynamicFiles = dynamicFiles;
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        Helper.ShowWinUIMessageBox("Error loading images\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,e);
        //    }
        //    finally
        //    {
        //        AgeReadingViewModel.CloseSplashScreen();
        //        AgeReadingViewModel.FirstLoad = false;
        //        LoadingFolder = false;
        //    }
        //}

        //public dynamic CreateDynamicSample(LarvaeSample sample)
        //{
        //    dynamic dynSample = new ExpandoObject();
        //    dynSample.ID = sample.ID;
        //    dynSample.StatusCode = sample.StatusCode;
        //    dynSample.StatusRank = sample.StatusRank;
        //    dynSample.StatusColor = sample.StatusColor;
        //    dynSample.Status = sample.Status;


        //    if (sample.SampleProperties != null)
        //    {
        //        List<string> columnNames = new List<string>();
        //        Dictionary<string, string> values = (Dictionary<string, string>)sample.SampleProperties;
        //        columnNames = values.Keys.ToList();
        //        foreach (var column in columnNames)
        //        {
        //            ((IDictionary<String, Object>)dynSample)[column] = values[column];

        //        }
        //    }

        //    return dynSample;
        //}

        //public void OnCustomColumnSort(object sender, CustomColumnSortEventArgs e)
        //{
        //    if (e.Column.FieldName == "Status")
        //    {
        //        var current = e.Value1 as StatusIcon;
        //        var other = e.Value2 as StatusIcon;
        //        int index1 = current.Rank;
        //        int index2 = other.Rank;
        //        e.Result = index1.CompareTo(index2);
        //        e.Handled = true;
        //    }
        //}

        //public void Refresh()
        //{
        //    CanDetach = SelectedFile?.SampleID != null && !AgeReadingViewModel.AgeReadingAnnotationViewModel.Outcomes.Any();
        //}



        //private void ToggleFileOptions(bool toggle)
        //{
        //    AgeReadingViewModel.EnableUI(!toggle);
        //    AgeReadingViewModel.AgeReadingEditorView.IsEnabled = !toggle;
        //    AgeReadingViewModel.AgeReadingFileView.IsEnabled = true;
        //    AgeReadingViewModel.AgeReadingFileView.FileGrid.IsEnabled = !toggle;
        //    AgeReadingViewModel.AgeReadingFileView.AnnotationsOperations.IsEnabled = !toggle;
        //    if (toggle)
        //    {
        //        AgeReadingViewModel.AgeReadingFileView.FileSettingsPanel.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        AgeReadingViewModel.AgeReadingFileView.FileSettingsPanel.Visibility = Visibility.Collapsed;
        //    }
        //}

        //public void FileSettings()
        //{
        //    AgeReadingViewModel.AgeReadingFileView.ShowNavBtns.EditValue = Properties.Settings.Default.ShowFileNavButtons;

        //    ToggleFileOptions(true);
        //}

        //public void SaveSettings()
        //{
        //    ToggleFileOptions(false);

        //    // code for saving the usersetting
        //    Properties.Settings.Default.ShowFileNavButtons = (bool) AgeReadingViewModel.AgeReadingFileView.ShowNavBtns.EditValue;
        //    ShowNavButtons = (bool) AgeReadingViewModel.AgeReadingFileView.ShowNavBtns.EditValue;

        //    Properties.Settings.Default.Save();
        //    RefreshToolbarVisibility();
        //}

        //public void CancelSettings()
        //{
        //    ToggleFileOptions(false);
        //}


        //public void Next(object sender, RoutedEventArgs e)
        //{
        //    LarvaeViewModel.LarvaeFileView.FileList.MoveNextRow();
        //}

        //public void Previous(object sender, RoutedEventArgs e)
        //{
        //    LarvaeViewModel.LarvaeFileView.FileList.MovePrevRow();
        //}

        public void RefreshNavigationButtons()
        {
            var index = LarvaeViewModel.LarvaeFileView.FileList.FocusedRowHandle;
            if (index <= 0)
            {
                LarvaeViewModel.LarvaeView.FilePrevious.IsEnabled = false;
            }
            else
            {
                LarvaeViewModel.LarvaeView.FilePrevious.IsEnabled = true;
            }

            if (index >= LarvaeFiles.Count - 1)
            {
                LarvaeViewModel.LarvaeView.FileNext.IsEnabled = false;
            }
            else
            {
                LarvaeViewModel.LarvaeView.FileNext.IsEnabled = true;
            }
        }

    }
}
