using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace FaceDetection
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IVisionServiceClient visionClient = new VisionServiceClient("your key");
        FaceRectangle[] faceRectangle;

        public MainPage()
        {
            
            this.InitializeComponent();
        }

        //open filePicker
        public async void PicturePicker()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile pictureFile = await picker.PickSingleFileAsync();
            if (pictureFile != null)
            {
                // Application now has read/write access to the picked file
                setImage(pictureFile);
                uploadAndAnalyze(pictureFile);
            }
        }

        //upload and analyze picture
        public async void uploadAndAnalyze(StorageFile picture)
        {
            ResultText.Text = "analysing";
            try
            {
                IRandomAccessStream randomAccessStream = await picture.OpenReadAsync();
                using (Stream stream = randomAccessStream.AsStreamForRead())
                {
                    AnalysisResult analysisResult = await visionClient.AnalyzeImageAsync(stream,
                            new VisualFeature[] { VisualFeature.Faces, VisualFeature.Description });
                    setResultText(analysisResult);
                    faceDraw(analysisResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ResultText.Text = "maybe no network connection";
            }
        }

        //show the description of picture
        public void setResultText(AnalysisResult analysisResult)
        {
            ResultText.Text = analysisResult.Description.Captions[0].Text;
        }

        //set image
        public async void setImage(StorageFile pictureFile)
        {
            IRandomAccessStream fileStream = await pictureFile.OpenAsync(FileAccessMode.Read);
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.SetSource(fileStream);
            SelectedImage.Source = bmpImage;
            //canvas resize
            ImageCanvas.Width = bmpImage.PixelWidth;
            ImageCanvas.Height = bmpImage.PixelHeight;
        }

        public void faceDraw(AnalysisResult analysisResult)
        {
            faceRectangle = analysisResult.Faces.Select(face => face.FaceRectangle).ToArray();
            //clear last faceRectangle
            while (ImageCanvas.Children.Count > 1)
            {
                ImageCanvas.Children.RemoveAt(ImageCanvas.Children.Count - 1);
            }
            canvasControlDraw();
        }

        //mark the face on canvas
        public void canvasControlDraw()
        {
            if (faceRectangle != null)
            {
                if (faceRectangle.Length > 0)
                {
                    foreach (var faceRect in faceRectangle)
                    {
                        //args.DrawingSession.DrawRectangle(faceRect.Left, faceRect.Top, faceRect.Width, faceRect.Height, Colors.Red);
                        Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle();
                        rect.Stroke = new SolidColorBrush(Colors.Yellow);
                        rect.StrokeThickness = 5d;
                        Canvas.SetLeft(rect, faceRect.Left);
                        Canvas.SetTop(rect, faceRect.Top);
                        rect.Width = faceRect.Width;
                        rect.Height = faceRect.Height;
                        ImageCanvas.Children.Add(rect);
                    }
                }
            }
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            PicturePicker();
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            theSplitView.IsPaneOpen = !theSplitView.IsPaneOpen;
        }
    }
}
