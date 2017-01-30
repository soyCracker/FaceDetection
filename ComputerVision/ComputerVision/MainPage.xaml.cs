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

namespace ComputerVision
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IVisionServiceClient visionClient = new VisionServiceClient("13c7e151ac374642bd998e40625804f9");
        FaceRectangle[] faceRectangle;

        public MainPage()
        {
            this.InitializeComponent();
        }

        //open filePicker
        public async void picturePicker()
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
            resultText.Text = "analysing";
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
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //show the description of picture
        public void setResultText(AnalysisResult analysisResult)
        {
            resultText.Text = analysisResult.Description.Captions[0].Text;
        }

        //set image
        public async void setImage(StorageFile pictureFile)
        {          
            IRandomAccessStream fileStream = await pictureFile.OpenAsync(FileAccessMode.Read);
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.SetSource(fileStream);
            selectedImage.Source = bmpImage;
            //set canvas to resize
            canvas.Width = bmpImage.PixelWidth;
            canvas.Height = bmpImage.PixelHeight;
        }

        public void faceDraw(AnalysisResult analysisResult)
        {
            faceRectangle = analysisResult.Faces.Select(face => face.FaceRectangle).ToArray();
            //clear last faceRectangle
            while (canvas.Children.Count > 1)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }
            canvasControlDraw();
        }

        //show the face on canvas
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
                        canvas.Children.Add(rect);
                    }
                }
            }
        }

        private void pickButton_Click(object sender, RoutedEventArgs e)
        {
            picturePicker();
        }
    }
}
