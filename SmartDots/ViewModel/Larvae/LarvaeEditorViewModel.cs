﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.WindowsUI;
using SmartDots.Helpers;
using SmartDots.Model;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Image = System.Windows.Controls.Image;
using Line = System.Windows.Shapes.Line;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;

namespace SmartDots.ViewModel
{
    public class LarvaeEditorViewModel : LarvaeBaseViewModel
    {
        private object lineColor;
        private object measureColor;
        private int lineWidth;
        private int measureLineWidth;
        private int measureFontSize;
        private ObservableCollection<Line> lineShapes = new ObservableCollection<Line>();
        private ObservableCollection<Line> topLevelLineShapes = new ObservableCollection<Line>();
        private ObservableCollection<Line> scaleShapes = new ObservableCollection<Line>();
        private ObservableCollection<Line> originalMeasureShapes = new ObservableCollection<Line>();
        private ObservableCollection<Line> measureShapes = new ObservableCollection<Line>();
        private ObservableCollection<TextBlock> textShapes = new ObservableCollection<TextBlock>();
        private CombinedLine activeCombinedLine;
        private CombinedLine tempCombinedLine;
        private EditorModeEnum mode;
        private BitmapImage originalImage;
        private bool shapeChangeFlag;
        private BitmapImage _larvaeImage;
        private int originalWidth;
        private int originalHeight;
        private int width;
        private int height;
        private double brightness;
        private double contrast;
        private decimal pixelLength;
        private bool hideLines;
        private bool isScaleDrawn;
        private bool isMeasureDrawn;

        public object LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                RaisePropertyChanged("LineColor");
            }
        }

        public object MeasureColor
        {
            get { return measureColor; }
            set
            {
                measureColor = value;
                RaisePropertyChanged("MeasureColor");
            }
        }

        public object LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = int.Parse(value.ToString());
                RaisePropertyChanged("LineWidth");
            }
        }

        public object MeasureLineWidth
        {
            get { return measureLineWidth; }
            set
            {
                measureLineWidth = int.Parse(value.ToString());
                RaisePropertyChanged("MeasureLineWidth");
            }
        }

        public object MeasureFontSize
        {
            get { return measureFontSize; }
            set
            {
                measureFontSize = int.Parse(value.ToString());
                RaisePropertyChanged("MeasureFontSize");
            }
        }

        public ObservableCollection<Line> ScaleShapes
        {
            get { return scaleShapes; }
            set
            {
                scaleShapes = value;
                RaisePropertyChanged("ScaleShapes");
            }
        }

        public ObservableCollection<Line> OriginalMeasureShapes
        {
            get { return originalMeasureShapes; }
            set
            {
                originalMeasureShapes = value;
            }
        }

        public ObservableCollection<Line> MeasureShapes
        {
            get { return measureShapes; }
            set
            {
                measureShapes = value;
                RaisePropertyChanged("MeasureShapes");
            }
        }

        public ObservableCollection<TextBlock> TextShapes
        {
            get { return textShapes; }
            set
            {
                textShapes = value;
                RaisePropertyChanged("TextShapes");
            }
        }

        public EditorModeEnum Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                if (larvaeViewModel != null)
                {
                    LarvaeViewModel.LarvaeEditorView.MeasureButton.IsPressed = false;
                    LarvaeViewModel.LarvaeEditorView.ScaleButton.IsPressed = false;

                    switch (value)
                    {
                        case EditorModeEnum.Measure:
                        case EditorModeEnum.MakingMeasure:
                            LarvaeViewModel.LarvaeEditorView.MeasureButton.IsPressed = true;
                            break;
                        case EditorModeEnum.DrawScale:
                            LarvaeViewModel.LarvaeEditorView.ScaleButton.IsPressed = true;
                            break;

                    }
                }
                RaisePropertyChanged("Mode");
            }
        }

        public BitmapImage LarvaeImage
        {
            get { return _larvaeImage; }
            set
            {
                _larvaeImage = value;
                if (value != null)
                {
                    OriginalWidth = _larvaeImage.PixelWidth;
                    OriginalHeight = _larvaeImage.PixelHeight;
                }
                else
                {
                    OriginalWidth = 0;
                    OriginalHeight = 0;
                }


                //Width = _larvaeImage.PixelWidth;
                //Height = _larvaeImage.PixelHeight;
                LarvaeViewModel.LarvaeStatusbarView.lblImgSize.Content = $"{OriginalWidth} x {OriginalHeight}";

                RaisePropertyChanged("LarvaeImage");
                RaisePropertyChanged("CanMeasure");
            }
        }

        public int OriginalWidth
        {
            get { return originalWidth; }
            set
            {
                originalWidth = value;
                RaisePropertyChanged("OriginalWidth");
            }
        }

        public int OriginalHeight
        {
            get { return originalHeight; }
            set
            {
                originalHeight = value;
                RaisePropertyChanged("OriginalHeight");
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                LarvaeViewModel.LarvaeEditorView.ScrollViewer.HorizontalScrollBarVisibility = width >
                                                                                                      LarvaeViewModel
                                                                                                          .LarvaeEditorView
                                                                                                          .ScrollViewer
                                                                                                          .Width
                    ? ScrollBarVisibility.Hidden
                    : ScrollBarVisibility.Auto;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                LarvaeViewModel.LarvaeEditorView.ScrollViewer.VerticalScrollBarVisibility = height >
                                                                                                    LarvaeViewModel
                                                                                                        .LarvaeEditorView
                                                                                                        .ScrollViewer
                                                                                                        .Height
                    ? ScrollBarVisibility.Hidden
                    : ScrollBarVisibility.Auto;
                RaisePropertyChanged("Height");
            }
        }


        public double Brightness
        {
            get { return brightness; }
            set
            {
                brightness = value;
                if (LarvaeViewModel != null && !LarvaeViewModel.LarvaeFileViewModel.ChangingFile)
                {
                    AdjustImage();
                    RaisePropertyChanged("LarvaeImage");
                }
                RaisePropertyChanged("Brightness");
            }
        }

        public double Contrast
        {
            get { return contrast; }
            set
            {
                contrast = value;
                if (LarvaeViewModel != null && !LarvaeViewModel.LarvaeFileViewModel.ChangingFile)
                {
                    AdjustImage();
                    RaisePropertyChanged("LarvaeImage");
                }
                RaisePropertyChanged("Contrast");
            }
        }

        public BitmapImage OriginalImage
        {
            get { return originalImage; }
            set
            {
                originalImage = value;
                if (value != null)
                {
                    originalImage.Freeze();
                    if (Global.API.Settings.AutoMeasureScale && (LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale == null || LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale == 0.0m))
                    {
                        AutoMeasureScale();
                    }
                }

            }
        }

        public decimal PixelLength
        {
            get { return pixelLength; }
            set { pixelLength = value; }
        }


        public bool CanMeasure
        {
            get { return LarvaeImage != null; }
        }


        public bool ShapeChangeFlag
        {
            get { return shapeChangeFlag; }
            set { shapeChangeFlag = value; }
        }

        public bool IsScaleDrawn
        {
            get { return isScaleDrawn; }
            set
            {
                isScaleDrawn = value;
                //if (isScaleDrawn)
                //{
                //    LarvaeViewModel.LarvaeEditorView.ScaleDrawn.Visibility = Visibility.Visible;
                //    LarvaeViewModel.LarvaeEditorView.ScaleNotDrawn.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    LarvaeViewModel.LarvaeEditorView.ScaleDrawn.Visibility = Visibility.Collapsed;
                //    LarvaeViewModel.LarvaeEditorView.ScaleNotDrawn.Visibility = Visibility.Visible;
                //}
                RaisePropertyChanged("IsScaleDrawn");
            }
        }

        public bool IsMeasureDrawn
        {
            get { return isMeasureDrawn; }
            set
            {
                isMeasureDrawn = value;
                RaisePropertyChanged("IsScaleDrawn");
            }
        }

        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>
        { "µm", "mm", "cm" };

        public string MeasureUnit { get; set; } = "mm";

        public bool IsContextmenuOpen { get; set; }
        public System.Drawing.Rectangle ScaleRectangle { get; private set; }
        public bool IsMeasuring { get; private set; }
        public List<Guid> MeasuredFileIDs { get; set; } = new List<Guid>();




        public LarvaeEditorViewModel()
        {
            Mode = EditorModeEnum.Measure;
            Brightness = 0;
            Contrast = 0;
            LineColor = (Color)ColorConverter.ConvertFromString("#FFFF00FF");
            MeasureColor = (Color)ColorConverter.ConvertFromString("#FFFF00FF");
            LineWidth = 2;
            MeasureLineWidth = 1;
            MeasureFontSize = 12;
            LoadUserPreferences();
        }

        public async void GetPixelLength()
        {
            try
            {
                var selectedFileId = LarvaeViewModel.LarvaeFileViewModel.SelectedFile.ID;
                var oldvalue = LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale;
                LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = false;

                decimal temp = await Task.Run(() => ScaleMeasureTool.Measure(OriginalImage, 128));

                if (LarvaeViewModel.LarvaeFileViewModel.SelectedFile.ID != selectedFileId)
                {
                    LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = true;
                    return;
                }

                PixelLength = temp;

                if (PixelLength == 0) throw new Exception("Could not determine the scale");

                var newvalue = PixelLength;
                if (oldvalue != newvalue)
                {
                    LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale = PixelLength;

                    LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = true;
                    DtoLarvaeFile dtofile =
                        (DtoLarvaeFile)
                        Helper.ConvertType(LarvaeViewModel.LarvaeFileViewModel.SelectedFile, typeof(DtoLarvaeFile));
                    var updateFileResult = Global.API.UpdateLarvaeFile(dtofile);
                    if (!updateFileResult.Succeeded)
                    {
                        Helper.ShowWinUIMessageBox("Error saving Larvae File to Web API\n" + updateFileResult.ErrorMessage, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = false;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Helper.ShowWinUIMessageBox(
                    "Could not determine the scale automatically. Please measure the scale manually", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information, e);

                PixelLength = 0;
            }

            LarvaeViewModel.LarvaeStatusbarViewModel.Info = $"Scale (px/mm): {PixelLength}";

            LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = true;
        }

        public void SaveUserPreferences()
        {
            try
            {
                Properties.Settings.Default.LineColor = LineColor.ToString();
                Properties.Settings.Default.LineWidth = (int)LineWidth;
                Properties.Settings.Default.MeasureColor = MeasureColor.ToString();
                Properties.Settings.Default.MeasureLineWidth = (int)MeasureLineWidth;
                Properties.Settings.Default.MeasureFontSize = (int)MeasureFontSize;
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
                Helper.ShowWinUIMessageBox("Error saving user preferences", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, e);
            }
        }

        public void UpdateButtons()
        {
            RaisePropertyChanged("CanDrawLine");
            RaisePropertyChanged("ContextmenuVisibility");
        }

        public void LoadUserPreferences()
        {
            try
            {
                LineColor = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.LineColor);
                LineWidth = Properties.Settings.Default.LineWidth;
                MeasureColor = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.MeasureColor);
                MeasureLineWidth = Properties.Settings.Default.MeasureLineWidth;
                MeasureFontSize = Properties.Settings.Default.MeasureFontSize;
            }
            catch (Exception e)
            {
                Helper.ShowWinUIMessageBox("Error saving user preferences", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, e);
            }
        }

        public void RestoreUserPreferences()
        {
            try
            {
                LineColor = (Color)ColorConverter.ConvertFromString("#FFFF00FF");
                LineWidth = 2;
                MeasureColor = (Color)ColorConverter.ConvertFromString("#FFFF00FF");
                MeasureLineWidth = 1;
                MeasureFontSize = 12;
            }
            catch (Exception e)
            {
                Helper.ShowWinUIMessageBox("Error restoring user preferences", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, e);
            }
        }

        public void RefreshShapes()
        {
            LarvaeViewModel.LarvaeEditorView.Dispatcher.Invoke(() =>
            {


                RefreshMeasures();


                ScaleShapes = ScaleShapes;

            });
        }

        public void RefreshMeasures(bool onlyLast = false)
        {

            float zoomfactor = LarvaeViewModel.LarvaeStatusbarViewModel.ZoomFactor;
            ObservableCollection<Line> measures = new ObservableCollection<Line>();
            ObservableCollection<TextBlock> text = new ObservableCollection<TextBlock>();

            var scale = LarvaeViewModel?.LarvaeFileViewModel.SelectedFile.Scale;
            decimal tmpScale = 1;
            if (scale != null && scale != 0)
            {
                tmpScale = (decimal)scale;
            }


            foreach (var line in originalMeasureShapes)
            {
                var isLongest = true;

                var length = (decimal)Math.Sqrt((int)Math.Abs(Math.Pow((line.X2 - line.X1), 2) + Math.Pow((line.Y2 - line.Y1), 2)));
                var totalLength = length;


                //if (!originalMeasureShapes.Any(x => x != line && int.Parse(((dynamic)x.Tag).LineGroup)) ==
                //        int.Parse(((dynamic)line.Tag).LineGroup))
                //{
                //    isLongest = true;
                //}
                //else
                //{
                foreach (var groupLine in originalMeasureShapes.Where(x => x != line && int.Parse(((dynamic)x.Tag).LineGroup) == int.Parse(((dynamic)line.Tag).LineGroup)))
                {

                    var groupLineLength = (decimal)Math.Sqrt(Math.Abs(Math.Pow((groupLine.X2 - groupLine.X1), 2) + Math.Pow((groupLine.Y2 - groupLine.Y1), 2)));
                    totalLength += groupLineLength;
                    if (groupLineLength == length && groupLine.X1 < line.X1)
                    {
                        isLongest = false;
                        break;
                    }
                    else if (groupLineLength > length)
                    {
                        isLongest = false;
                        break;
                    }
                }
                //}


                Line l = new Line()
                {
                    X1 = line.X1 * zoomfactor,
                    Y1 = line.Y1 * zoomfactor,
                    X2 = line.X2 * zoomfactor,
                    Y2 = line.Y2 * zoomfactor,
                    Stroke = line.Stroke,
                    StrokeThickness = line.StrokeThickness * zoomfactor,
                    StrokeDashCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeDashArray = new DoubleCollection() { 0.25, 1.5 },
                    Uid = line.Uid,
                    IsHitTestVisible = true,
                    Tag = line.Tag
                };

                Line l2 = new Line()
                {
                    X1 = l.X1,
                    Y1 = l.Y1,
                    X2 = l.X2,
                    Y2 = l.Y2,
                    StrokeThickness = 30 * zoomfactor,
                    IsHitTestVisible = true,
                    Uid = l.Uid,
                    Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
                };

                measures.Add(l);
                measures.Add(l2);

                if (!isLongest) continue;



                var tb = new TextBlock()
                {
                    Text = "Placeholder",
                    Foreground = l.Stroke,
                    FontSize = int.Parse(((dynamic)l.Tag).FontSize) * zoomfactor,
                };


                if (scale != null && scale != 0)
                {
                    var lengthText = "";

                    totalLength = totalLength / tmpScale;

                    if (totalLength < 0.001m)
                    {
                        var tempLength = totalLength * 1000000;
                        tempLength = decimal.Round((decimal)tempLength, 0);
                        lengthText = tempLength.ToString() + " nm";
                    }
                    else if (totalLength < 1m)
                    {
                        var tempLength = totalLength * 1000;
                        tempLength = decimal.Round((decimal)tempLength, 1);
                        lengthText = tempLength.ToString() + " µm";
                    }
                    else if (totalLength > 100m)
                    {
                        var tempLength = totalLength / 10;
                        tempLength = decimal.Round((decimal)tempLength, 2);
                        lengthText = tempLength.ToString() + " cm";
                    }
                    else
                    {
                        totalLength = decimal.Round((decimal)totalLength, 2);
                        lengthText = totalLength.ToString() + " mm";
                    }

                    tb.Text = lengthText;
                }
                else
                {
                    tb.Text = Math.Sqrt((int)Math.Abs(Math.Pow((line.X2 - line.X1), 2) + Math.Pow((line.Y2 - line.Y1), 2))).ToString("N0") + " px";
                }


                //atan2 for angle
                var radians = Math.Atan2(line.Y2 - line.Y1, line.X2 - line.X1);

                //radians into degrees
                var angle = radians * (180 / Math.PI);

                var size = Helper.MeasureString(tb);

                var left = (line.X2 * zoomfactor + line.X1 * zoomfactor - size.Width) / 2 + Math.Sin(radians) * size.Height * 0.6;
                if (left < 0) left = 0;

                var top = (line.Y2 * zoomfactor + line.Y1 * zoomfactor - size.Height) / 2 - Math.Cos(radians) * size.Height * 0.6;
                if (top < 0) top = 0;

                if (angle < -90 || angle > 90)
                {
                    angle += 180;
                }

                tb.Margin = new Thickness(left, top, 0, 0);

                tb.RenderTransform = new RotateTransform(angle, size.Width / 2, size.Height / 2);

                text.Add(tb);
            }


            MeasureShapes = measures;
            TextShapes = text;
        }

        public void RemoveShapes()
        {
            OriginalMeasureShapes = new ObservableCollection<Line>();
            MeasureShapes = new ObservableCollection<Line>();
            TextShapes = new ObservableCollection<TextBlock>();
        }


        public void AddLine(Model.Line l)
        {
            //LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last().AddLine(l);
            //ActiveCombinedLine =
            //    LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last();

            //RefreshShapes();
            //LarvaeViewModel.UpdateGraphs();
            //LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.IsChanged = true;
            //ShapeChangeFlag = true;
        }


        public void AdjustImage()
        {
            try
            {
                if (OriginalImage != null)
                {
                    if (Brightness == 0 && Contrast == 0)
                    {
                        LarvaeImage = OriginalImage;
                    }
                    else
                    {
                        LarvaeImage = SetBrightness(SetContrast(OriginalImage));
                    }

                }
            }
            catch (Exception e)
            {
                Helper.ShowWinUIMessageBox("Error adjusting image", "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    e);
            }
        }

        public BitmapImage SetBrightness(BitmapImage img)
        {
            unsafe
            {
                double brightness = Brightness;
                Bitmap bmap = BitmapConverter.BitmapImage2Bitmap(img);
                BitmapData bitmapData = bmap.LockBits(new System.Drawing.Rectangle(0, 0, bmap.Width, bmap.Height),
                    ImageLockMode.ReadWrite, bmap.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For((long)0, heightInPixels, y =>
               {
                   byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);

                   for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                   {
                       double oldBlue = currentLine[x] + brightness;
                       if (oldBlue < 0) oldBlue = 1;
                       if (oldBlue > 255) oldBlue = 255;

                       double oldGreen = currentLine[x + 1] + brightness;
                       if (oldGreen < 0) oldGreen = 1;
                       if (oldGreen > 255) oldGreen = 255;

                       double oldRed = currentLine[x + 2] + brightness;
                       if (oldRed < 0) oldRed = 1;
                       if (oldRed > 255) oldRed = 255;

                       currentLine[x] = (byte)oldBlue;
                       currentLine[x + 1] = (byte)oldGreen;
                       currentLine[x + 2] = (byte)oldRed;
                   }
               });
                bmap.UnlockBits(bitmapData);
                return BitmapConverter.Bitmap2BitmapImage(bmap);
            }
        }

        public BitmapImage SetContrast(BitmapImage img)
        {
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();

            unsafe
            {
                double contrast = Contrast;
                Bitmap bmap = BitmapConverter.BitmapImage2Bitmap(img);
                contrast = (100.0 + contrast) / 100.0;
                contrast *= contrast;
                BitmapData bitmapData = bmap.LockBits(new System.Drawing.Rectangle(0, 0, bmap.Width, bmap.Height),
                    ImageLockMode.ReadWrite, bmap.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For((long)0, heightInPixels, y =>
               {
                   byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);

                   for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                   {
                       double oldBlue = currentLine[x] / 255.0;
                       oldBlue -= 0.5;
                       oldBlue *= contrast;
                       oldBlue += 0.5;
                       oldBlue *= 255;
                       if (oldBlue < 0) oldBlue = 0;
                       if (oldBlue > 255) oldBlue = 255;

                       double oldGreen = currentLine[x + 1] / 255.0;
                       oldGreen -= 0.5;
                       oldGreen *= contrast;
                       oldGreen += 0.5;
                       oldGreen *= 255;
                       if (oldGreen < 0) oldGreen = 0;
                       if (oldGreen > 255) oldGreen = 255;

                       double oldRed = currentLine[x + 2] / 255.0;
                       oldRed -= 0.5;
                       oldRed *= contrast;
                       oldRed += 0.5;
                       oldRed *= 255;
                       if (oldRed < 0) oldRed = 0;
                       if (oldRed > 255) oldRed = 255;

                       currentLine[x] = (byte)oldBlue;
                       currentLine[x + 1] = (byte)oldGreen;
                       currentLine[x + 2] = (byte)oldRed;
                   }
               });
                bmap.UnlockBits(bitmapData);

                // Stop timing.
                stopwatch.Stop();

                // Write result.
                Console.WriteLine("Time elapsed for SetContrast: " + stopwatch.Elapsed);
                return BitmapConverter.Bitmap2BitmapImage(bmap);
            }
        }


        public void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LarvaeViewModel.LarvaeStatusbarViewModel.IsFittingImage)
            {
                LarvaeViewModel.LarvaeStatusbarViewModel.FitImage();
            }
            else
            {
                RefreshShapes();
            }
        }

        public void ParentCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //here comes the code for drawing, dotting, deleting

            if (IsContextmenuOpen) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (LarvaeViewModel.LarvaeFileViewModel.SelectedFile.IsReadOnly && !(Mode == EditorModeEnum.DrawScale || Mode == EditorModeEnum.Measure || Mode == EditorModeEnum.MakingMeasure))
                {
                    new WinUIMessageBoxService().Show("The selected file is ReadOnly!", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }


                switch (Mode)
                {
                    case EditorModeEnum.DrawLine:
                        DrawLine(e);
                        break;
                    case EditorModeEnum.MakingLine:
                        MakeLine(e);
                        break;
                    case EditorModeEnum.DrawScale:
                        DrawScale(e);
                        break;
                    case EditorModeEnum.Measure:
                        DrawMeasure(e);
                        break;
                    case EditorModeEnum.MakingMeasure:
                        MakingMeasure(e);
                        break;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                switch (Mode)
                {
                    case EditorModeEnum.MakingLine:
                        EndLine(e);
                        break;
                    case EditorModeEnum.MakingMeasure:
                        MakingMeasure(e);
                        break;
                }

            }
            UpdateButtons();
        }

        private void DrawScale(MouseButtonEventArgs e)
        {
            if (!ScaleShapes.Any())
            {
                float zoomfactor = LarvaeViewModel.LarvaeStatusbarViewModel.ZoomFactor;
                var position = e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas);
                Line line = new Line()
                {
                    X1 = position.X,
                    Y1 = position.Y,
                    X2 = position.X,
                    Y2 = position.Y,
                    Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(LineColor.ToString())),
                    StrokeThickness = ((int)LineWidth * zoomfactor),
                };
                ScaleShapes.Add(line);
            }
            else
            {
                IsScaleDrawn = true;
                Mode = EditorModeEnum.None;
            }
            RefreshShapes();
        }

        private void DrawMeasure(MouseButtonEventArgs e)
        {
            //if (!MeasureShapes.Any())
            //{
            float zoomfactor = LarvaeViewModel.LarvaeStatusbarViewModel.ZoomFactor;
            var position = e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas);
            var id = Guid.NewGuid();
            Line line = new Line()
            {
                X1 = position.X / zoomfactor,
                Y1 = position.Y / zoomfactor,
                X2 = position.X / zoomfactor,
                Y2 = position.Y / zoomfactor,
                Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(MeasureColor.ToString())),
                StrokeThickness = (int)MeasureLineWidth,
                StrokeDashCap = PenLineCap.Round,
                StrokeDashArray = new DoubleCollection() { 0.25, 1.5 },
                Uid = id.ToString(),



            };

            string lineGroup = OriginalMeasureShapes.Count.ToString();

            dynamic tag = new ExpandoObject();
            tag.FontSize = MeasureFontSize.ToString();
            tag.LineGroup = lineGroup;
            line.Tag = tag;

            OriginalMeasureShapes.Add(line);
            Mode = EditorModeEnum.MakingMeasure;
            LarvaeViewModel.EnableUI(false);

            //}
            //else
            //{
            //    Mode = EditorModeEnum.None;
            //}
            RefreshShapes();
        }

        private void MakingMeasure(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                OriginalMeasureShapes.Remove(OriginalMeasureShapes.Last());
                CancelMeasure();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                OriginalMeasureShapes.Add(new Line()
                {
                    X1 = OriginalMeasureShapes.Last().X2,
                    Y1 = OriginalMeasureShapes.Last().Y2,
                    X2 = OriginalMeasureShapes.Last().X2,
                    Y2 = OriginalMeasureShapes.Last().Y2,
                    Stroke = OriginalMeasureShapes.Last().Stroke,
                    StrokeThickness = OriginalMeasureShapes.Last().StrokeThickness,
                    Tag = OriginalMeasureShapes.Last().Tag
                });
            }
        }

        private void DrawLine(MouseButtonEventArgs e)
        {
            //if (LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation == null)
            //{
            //    if (!LarvaeViewModel.AgeReadingAnnotationViewModel.CanCreate) return;
            //    LarvaeViewModel.AgeReadingAnnotationViewModel.NewAnnotation();
            //    if (LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation == null) return;
            //}
            //if (LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Any()) return;

            //LarvaeViewModel.EnableUI(false);
            //CombinedLine temp = new CombinedLine();
            //float zoomfactor = ZoomFactor;
            //Model.Line l = new Model.Line()
            //{
            //    ID = Guid.NewGuid(),
            //    Color = LineColor.ToString(),
            //    X1 = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).X / zoomfactor),
            //    X2 = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).X / zoomfactor),
            //    Y1 = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).Y / zoomfactor),
            //    Y2 = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).Y / zoomfactor),
            //    Width = (int)LineWidth,
            //    ParentCombinedLine = temp,
            //    AnnotationID = LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.ID,
            //    LineIndex = 0
            //};
            //LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Add(temp);
            //Mode = EditorModeEnum.MakingLine;
            //AddLine(l);
        }

        private void MakeLine(MouseButtonEventArgs e)
        {
            try
            {
                if (Mode == EditorModeEnum.MakingLine)
                {
                    float zoomfactor = LarvaeViewModel.LarvaeStatusbarViewModel.ZoomFactor;

                    //LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last()
                    //        .Lines.Last()
                    //        .X2
                    //    = (int)(e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).X / zoomfactor);
                    //LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last()
                    //        .Lines.Last()
                    //        .Y2
                    //    = (int)(e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).Y / zoomfactor);

                    //Model.Line l =
                    //    LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last()
                    //        .Lines.Last();

                    //Model.Line line = new Model.Line()
                    //{
                    //    ID = Guid.NewGuid(),
                    //    Color = LineColor.ToString(),
                    //    X1 = l.X2,
                    //    X2 = l.X2,
                    //    Y1 = l.Y2,
                    //    Y2 = l.Y2,
                    //    Width = (int)LineWidth,
                    //    ParentCombinedLine =
                    //        LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last(),
                    //    AnnotationID = LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.ID,
                    //    LineIndex = LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation
                    //                    .CombinedLines.Last().Lines.Last().LineIndex + 1
                    //};

                    //AddLine(line);
                }
            }
            catch (Exception)
            {
                //
            }

        }

        private void EndLine(MouseButtonEventArgs e)
        {
            try
            {
                //if (LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Any())
                //{
                //    LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last()
                //        .Lines.RemoveAt(
                //            LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last()
                //                .Lines.Count -
                //            1);
                //    if (!LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last().Lines.Any())
                //    {
                //        LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Remove(
                //            LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last());
                //        ActiveCombinedLine = null;
                //        Mode = EditorModeEnum.DrawLine;
                //    }
                //    else
                //    {
                //        Mode = EditorModeEnum.DrawDot;
                //    }

                //    RefreshShapes();
                //    LarvaeViewModel.UpdateGraphs();
                //    UpdateButtons();
                //    LarvaeViewModel.AgeReadingAnnotationViewModel.RefreshActions();

                //    LarvaeViewModel.EnableUI(true);
                //}
            }
            catch (Exception)
            {
                Mode = EditorModeEnum.DrawLine;
                LarvaeViewModel.EnableUI(true);

            }
            LarvaeViewModel.EnableUI(true);
            UpdateButtons();


        }


        public void ParentCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var zoomfactor = LarvaeViewModel.LarvaeStatusbarViewModel.ZoomFactor;
            try
            {

                //if (Mode == EditorModeEnum.MakingLine && LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Any())
                //{
                //    LarvaeViewModel.LarvaeEditorView.Cursor = Cursors.Arrow;
                //    LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last().Lines.Last().X2
                //        = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).X / zoomfactor);
                //    LarvaeViewModel.AgeReadingAnnotationViewModel.WorkingAnnotation.CombinedLines.Last().Lines.Last().Y2
                //        = (int)((int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).Y / zoomfactor);
                //    LineShapes.Last().X2 = (int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).X;
                //    LineShapes.Last().Y2 = (int)e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas).Y;
                //}


                if (Mode == EditorModeEnum.DrawScale && ScaleShapes.Any())
                {
                    LarvaeViewModel.LarvaeEditorView.Cursor = Cursors.Arrow;
                    var position = e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas);
                    var line = ScaleShapes.Last();
                    line.X2 = position.X;
                    LarvaeViewModel.LarvaeEditorView.ScalePixels.Text =
                        Math.Sqrt(
                            (int)
                            Math.Abs(Math.Pow((line.X2 - line.X1) / zoomfactor, 2) +
                                     Math.Pow((line.Y2 - line.Y1) / zoomfactor, 2))).ToString("N0");
                }
                else if (Mode == EditorModeEnum.MakingMeasure)
                {
                    LarvaeViewModel.LarvaeEditorView.Cursor = Cursors.Arrow;
                    var position = e.GetPosition(LarvaeViewModel.LarvaeEditorView.ParentCanvas);
                    var line = originalMeasureShapes.Last();
                    line.X2 = position.X / zoomfactor;
                    line.Y2 = position.Y / zoomfactor;

                    RefreshMeasures(true); //todo option to only refresh last one
                }
                else
                {
                    LarvaeViewModel.LarvaeEditorView.Cursor = Cursors.Arrow;
                }
            }
            catch (Exception)
            {
                //
            }

        }

        public void DrawLineBtn_Checked(object sender, RoutedEventArgs e)
        {
            Mode = EditorModeEnum.DrawLine;
        }

        public void AutoMeasureScale(bool buttonPressed = false)
        {
            if (!buttonPressed)
            {
                if (MeasuredFileIDs.Contains(LarvaeViewModel.LarvaeFileViewModel.SelectedFile.ID)) return;
                else
                {
                    MeasuredFileIDs.Add(LarvaeViewModel.LarvaeFileViewModel.SelectedFile.ID);
                }
            }

            if (LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Path != null)
            {
                try
                {
                    GetPixelLength();
                }
                catch (Exception e)
                {
                    Helper.ShowWinUIMessageBox("Could not determine the scale automatically. Please measure the scale manually", "Info", MessageBoxButton.OK, MessageBoxImage.Information, e);
                }
            }
            RefreshMeasures();
        }

        public void MeasureTool()
        {
            Mode = Mode == EditorModeEnum.Measure ? EditorModeEnum.None : EditorModeEnum.Measure;
        }

        public void ManualMeasureScale()
        {
            Mode = EditorModeEnum.DrawScale;
            LarvaeViewModel.EnableUI(false);
            LarvaeViewModel.LarvaeEditorView.MeasureScalePanel.Visibility = Visibility.Visible;
        }

        public void DeleteMeasureScale()
        {
            LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale = null;
            ((dynamic)LarvaeViewModel.LarvaeFileView.FileList.FocusedRowData.Row).Scale = null;
            LarvaeViewModel.LarvaeFileView.LarvaeFileGrid.RefreshData();
            var dtofile = (DtoFile)Helper.ConvertType(LarvaeViewModel.LarvaeFileViewModel.SelectedFile, typeof(DtoFile));
            var deleteResult = Global.API.UpdateFile(dtofile);
            if (!deleteResult.Succeeded)
                Helper.ShowWinUIMessageBox(deleteResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ClearScaleLine()
        {
            ScaleShapes.Clear();
            IsScaleDrawn = false;
            LarvaeViewModel.LarvaeEditorView.ScalePixels.Text = "0";
            RefreshShapes();
            Mode = EditorModeEnum.DrawScale;
        }

        public void AcceptScale()
        {
            double pixels;
            double.TryParse(LarvaeViewModel.LarvaeEditorView.ScalePixels.Text.Replace(",", "").Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out pixels);
            double unitLength;
            double.TryParse(LarvaeViewModel.LarvaeEditorView.ScaleMilimeters.Text.Replace(",", "").Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out unitLength);

            double milimeters = unitLength;
            if (MeasureUnit == "cm")
            {
                milimeters *= 10;
            }
            else if (MeasureUnit == "µm")
            {
                milimeters /= 1000;
            }

            PixelLength = (decimal)(pixels / milimeters);
            var oldvalue = LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale;
            var newvalue = PixelLength;
            if (oldvalue != newvalue)
            {
                LarvaeViewModel.LarvaeFileViewModel.SelectedFile.Scale = PixelLength;
                LarvaeViewModel.LarvaeStatusbarViewModel.Info = $"Scale (px/mm): {PixelLength}";
                ((dynamic)LarvaeViewModel.LarvaeFileView.FileList.FocusedRowData.Row).Scale = PixelLength;
                LarvaeViewModel.LarvaeFileView.LarvaeFileGrid.RefreshData();
                LarvaeViewModel.LarvaeEditorView.ScaleButton.IsEnabled = true;
                DtoLarvaeFile dtofile = (DtoLarvaeFile)Helper.ConvertType(LarvaeViewModel.LarvaeFileViewModel.SelectedFile, typeof(DtoLarvaeFile));
                var updateFileResult = Global.API.UpdateLarvaeFile(dtofile);
                if (!updateFileResult.Succeeded)
                {
                    Helper.ShowWinUIMessageBox("Error saving File to Web API\n" + updateFileResult.ErrorMessage, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            ScaleShapes.Clear();
            IsScaleDrawn = false;
            LarvaeViewModel.LarvaeEditorView.ScalePixels.Text = "0";
            RefreshShapes();
            LarvaeViewModel.LarvaeEditorView.MeasureScalePanel.Visibility = Visibility.Collapsed;
            Mode = EditorModeEnum.None;
        }

        public void CancelScale()
        {
            ScaleShapes.Clear();
            IsScaleDrawn = false;
            LarvaeViewModel.LarvaeEditorView.ScalePixels.Text = "0";
            RefreshShapes();
            LarvaeViewModel.LarvaeEditorView.MeasureScalePanel.Visibility = Visibility.Collapsed;
            Mode = EditorModeEnum.None;
        }

        public void CancelMeasure()
        {
            MeasureShapes.Clear();
            IsMeasureDrawn = false;
            RefreshShapes();
            Mode = EditorModeEnum.Measure;
            LarvaeViewModel.EnableUI(true);

        }
    }
}
