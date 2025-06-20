using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;


namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            try
            {
                InitializeComponent();
                this.Loaded += OnLoaded;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        public void UpdateProgress(string message, int percent)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressText.Text = message;
                    ProgressBar.Value = percent;
                    PercentageText.Text = $"{percent}%";
                });
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Даем окну возможность полностью отобразиться
                await Task.Delay(200); // Небольшая пауза (окно прогружается)

                //// Теперь ждем ещё 1.5 секунды перед стартом анимации
                //await Task.Delay(1500);

                // Загружаем гифку
                var imageUri = new Uri("pack://application:,,,/resources/gifs/logo2.gif", UriKind.Absolute);
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = imageUri;
                image.EndInit();

                ImageBehavior.SetAnimatedSource(GifImage, image);

                // Замедляем воспроизведение
                SetGifSpeed(GifImage, 0.5);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void SetGifSpeed(Image imageControl, double speedFactor)
        {
            try
            {
                if (imageControl == null || speedFactor <= 0.0 || speedFactor == 1.0)
                    return;

                var controller = ImageBehavior.GetAnimationController(imageControl);
                if (controller == null)
                    return;

                var animationField = controller.GetType().GetField("_animation", BindingFlags.NonPublic | BindingFlags.Instance);
                var animation = animationField?.GetValue(controller) as ObjectAnimationUsingKeyFrames;
                if (animation == null) return;

                foreach (var keyFrame in animation.KeyFrames)
                {
                    if (keyFrame is DiscreteObjectKeyFrame discrete)
                    {
                        discrete.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(discrete.KeyTime.TimeSpan.TotalMilliseconds / speedFactor));
                    }
                }

                controller.GotoFrame(0);
                controller.Play();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
    }
}
