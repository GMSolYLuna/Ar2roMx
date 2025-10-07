using System.Configuration;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GunboundTools.Delegates;

namespace GunboundImageCreator.App
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using GunboundTools.Archive;
    using GunboundTools.Decoding;
    using GunboundTools.Encoding;
    using GunboundTools.Exceptions;
    using GunboundTools.Imaging;
    using Microsoft.Win32;
    using WinForms = System.Windows.Forms;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public const string ProductName = "AGIC";

        private DispatcherTimer _aniTimer;
        private DispatcherTimer _avaTimer;

        private static readonly RoutedCommand OpenCommand = new RoutedCommand();

        private bool _isBusy = true;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy)
                    return;

                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        private ObservableCollection<FileInfo> _imgFiles = new ObservableCollection<FileInfo>();
        public ObservableCollection<FileInfo> ImgFiles
        {
            get { return _imgFiles; }
            set
            {
                if (value == _imgFiles)
                    return;

                _imgFiles = value;
                OnPropertyChanged("ImgFiles");
            }
        }

        private ObservableCollection<ImageFrame> _imgFrames = new ObservableCollection<ImageFrame>();
        public ObservableCollection<ImageFrame> ImgFrames
        {
            get { return _imgFrames; }
            set
            {
                if (value == _imgFrames)
                    return;

                _imgFrames = value;
                OnPropertyChanged("ImgFrames");
            }
        }

        private GunboundImageFile _gbImageFile = new GunboundImageFile();
        public GunboundImageFile GbImageFile
        {
            get { return _gbImageFile; }
            set
            {
                if (value == _gbImageFile)
                    return;

                _gbImageFile = value;
                OnPropertyChanged("GbImageFile");
            }
        }

        private bool _isOpenedOrCreated;
        public bool IsOpenedOrCreated
        {
            get { return _isOpenedOrCreated; }
            set
            {
                _isOpenedOrCreated = value;
                OnPropertyChanged("IsOpenedOrCreated");
            }
        }

        private bool _isAniOpenedOrCreated;
        public bool IsAniOpenedOrCreated
        {
            get { return _isAniOpenedOrCreated; }
            set
            {
                _isAniOpenedOrCreated = value;
                OnPropertyChanged("IsAniOpenedOrCreated");
            }
        }

        private readonly Dictionary<TransparencyType, string> _transparencyType = new Dictionary
            <TransparencyType, string>
                                                                                      {
                                                                                          {
                                                                                              TransparencyType.Alpha,
                                                                                              Properties.Resources.
                                                                                              AlphaLabel
                                                                                              },
                                                                                          {
                                                                                              TransparencyType.None,
                                                                                              Properties.Resources.
                                                                                              NoneLabel
                                                                                              },
                                                                                          {
                                                                                              TransparencyType.Simple,
                                                                                              Properties.Resources.
                                                                                              SimpleLabel
                                                                                              },
                                                                                          {
                                                                                              TransparencyType.
                                                                                              SimpleBackground,
                                                                                              Properties.Resources.
                                                                                              BackgroundLabel
                                                                                              }

                                                                                      };

        public Dictionary<TransparencyType, string> TransparencyTypes
        {
            get { return _transparencyType; }
        }

        private GunboundAnimationFile _animationFile = new GunboundAnimationFile();
        public GunboundAnimationFile AnimationFile
        {
            get { return _animationFile; }
            set
            {
                if (value == _animationFile)
                    return;

                _animationFile = value;
                OnPropertyChanged("AnimationFile");
            }
        }

        private DateTime _productExpiration;
        public DateTime ProductExpiration
        {
            get { return _productExpiration; }
            set
            {
                if (value == _productExpiration)
                    return;

                _productExpiration = value;
                OnPropertyChanged("ProductExpiration");
            }
        }

        private double _productPrice;
        public double ProductPrice
        {
            get { return _productPrice; }
            set
            {
                _productPrice = value;
                OnPropertyChanged("ProductPrice");
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            OpenCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            OpenCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));

            LoadLanguageMenu();
            DataContext = this;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadLanguageMenu();

            var headFile = new GunboundImageFile();
            headFile.SetData(Properties.Resources.mh00000);
            var bodyFile = new GunboundImageFile();
            bodyFile.SetData(Properties.Resources.mb00000);

            _player = new Player(headFile, bodyFile);
            _player.LoadImages();

            _aniTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(1) };
            _avaTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(1) };

            _aniTimer.Tick += AnimationTimerTick;
            _avaTimer.Tick += AvatarPreviewTimerTick;
        }

        private int _currentFrame;
        private int _avaHeadElapsed;
        private int _currentAvaBodyFrame;
        private int _avaBodyElapsed;
        private int _currentFlagFrame;
        private int _avaFlagElapsed;
        private int _currentGlassFrame;
        private int _avaGlassElapsed;

        private Player _player;

        private void AvatarPreviewTimerTick(object sender, EventArgs e)
        {
            Animate(avaHeadPreviewImage, _player, _player.HeadFile, _player.HeadAnimation, ref _currentFrame, ref _avaHeadElapsed);
            Animate(avaBodyPreviewImage, _player, _player.BodyFile, _player.BodyAnimation, ref _currentAvaBodyFrame, ref _avaBodyElapsed);
            Animate(avaFlagPreviewImage, _player, _player.FlagFile, _player.FlagAnimation, ref _currentFlagFrame, ref _avaFlagElapsed);

            if ((_player.HeadFile != null && _player.GlassesFile != null) && _player.HeadFile.Images.Count == _player.GlassesFile.Images.Count)
            {
                _currentGlassFrame = _currentFrame;
                _avaGlassElapsed = _avaHeadElapsed;
            }

            Animate(avaGlassesPreviewImage, _player, _player.GlassesFile, _player.GlassesAnimation, ref _currentGlassFrame, ref _avaGlassElapsed);
        }

        private void Animate(AnimationFrameImage aniCtrl, Player player, GunboundImageFile imgFile, GunboundAnimationFile aniFile, ref int currentAniFrame, ref int elapsed)
        {
            if (aniFile.TimeLines.Count > 0 && imgFile.Images.Count > 0)
            {
                var x = aniFile.TimeLines[0];

                if (x == null || x.Frames.Count < currentAniFrame)
                    return;

                if (currentAniFrame + 1 > x.Frames.Count)
                    currentAniFrame = 0;

                var currentTimeLine = x;

                if (elapsed > 2 * currentTimeLine.Frames[currentAniFrame].Duration)
                {
                    var frame = currentTimeLine.Frames[currentAniFrame];

                    if (frame.KeyFrame + 1 > currentTimeLine.Frames.Count)
                        return;

                    var img = player.GetImage(frame.KeyFrame, imgFile);

                    using (var bitmap = new GunboundImageDecoder(img).GetImage())
                    {
                        aniCtrl.SetImage(bitmap, img.XCenter, img.YCenter);
                    }

                    currentAniFrame++;
                    elapsed = 0;
                }
                else
                {
                    elapsed++;
                }
            }
        }

        private void SetAvatar(string path)
        {
            ResetPlayer();

            var fInfo = new FileInfo(path);

            if (fInfo.Name.StartsWith("mf"))
            {
                _player.FlagFile = null;
                _player.FlagFile = _gbImageFile;
            }

            if (fInfo.Name.StartsWith("mh") || fInfo.Name.StartsWith("fh"))
            {
                _player.HeadFile = null;
                _player.HeadFile = _gbImageFile;
            }

            if (fInfo.Name.StartsWith("mb") || fInfo.Name.StartsWith("fb"))
            {
                _player.BodyFile = null;
                _player.BodyFile = _gbImageFile;
            }

            if (fInfo.Name.StartsWith("mg") || fInfo.Name.StartsWith("gh"))
            {
                _player.GlassesFile = null;
                _player.GlassesFile = _gbImageFile;
            }

            _player.LoadImages();
        }

        private int _currentAniFrame;
        private int _elapsed;

        private void AnimationTimerTick(object sender, EventArgs e)
        {

            if (AnimationFile.TimeLines.Count > 0)
            {
                var x = comboBoxAniTimeLines.SelectedItem as AnimationTimeline;

                if (x == null)
                    return;

                if (_currentAniFrame + 1 > x.Frames.Count)
                    _currentAniFrame = 0;

                var currentTimeLine = x;

                if (_elapsed > 2 * currentTimeLine.Frames[_currentAniFrame].Duration)
                {

                    var frame = currentTimeLine.Frames[_currentAniFrame];

                    if (frame.KeyFrame + 1 > _gbImageFile.Images.Count)
                        return;

                    var img = _gbImageFile.Images[frame.KeyFrame];

                    using (var bitmap = new GunboundImageDecoder(img).GetImage())
                    {
                        aniImage.SetImage(bitmap, img.XCenter, img.YCenter);
                    }

                    _currentAniFrame++;
                    _elapsed = 0;
                }
                else
                {
                    _elapsed++;
                }
            }
        }

        private void OpenImgFile(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
                                     {
                                         Filter = "Img Files|*.img"
                                     };

            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != true)
                return;

            var path = openFileDialog.FileName;

            OpenImg(path);
        }

        private void OpenImg(string path)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                ResetProgramValues(path);
                IsOpenedOrCreated = true;
                SetAvatar(path);

                var expression = comboBoxImageTypes.GetBindingExpression(Selector.SelectedValueProperty);
                if (expression != null)
                    expression.UpdateTarget();
            }
            catch (GameArchiveException err)
            {
                MessageBox.Show(err.Message, Properties.Resources.ErrorLabel, MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show(Properties.Resources.ErrorProcessingFile, Properties.Resources.ErrorLabel,
                                MessageBoxButton.OK, MessageBoxImage.Error);

                ClearAll();
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void ClearAll()
        {
            GbImageFile = null;
            AnimationFile = null;
            ImgFrames = null;
            IsBusy = true;
            pbarImgEditor.Value = 0;
        }

        private void ResetProgramValues(string filename)
        {
            AnimationFile = new GunboundAnimationFile();
            ImgFrames = new ObservableCollection<ImageFrame>();
            GbImageFile = new GunboundImageFile(filename);
            GbImageFile.LoadImagesProgressChange += GbImageFileLoadImagesProgressChange;
            GbImageFile.Load();
            GbImageFile.LoadImages();
            UpdateFrames();

            aniImage.Clear();

            comboBoxImageTypes.SelectedIndex = 0;
            lboxImages.SelectedIndex = 0;

            //AnimationFile.Clear();
            //ImgFrames.Clear();
            //_gbImageFile.Clear();
            //_gbImageFile.Path = filename;
            //_gbImageFile.Load();
            //_gbImageFile.LoadImages();

            //UpdateFrames();

            //comboBoxImageTypes.SelectedIndex = 0;
            //lboxImages.SelectedIndex = 0;
        }

        private void GbImageFileLoadImagesProgressChange(object sender, LoadingImagesArgs e)
        {
            IsBusy = e.ProgressPercent == 100;
            pbarImgEditor.Value = e.ProgressPercent;

            if (e.ProgressPercent == 100)
                pbarImgEditor.Value = 0;
        }

        private void UpdateFrames()
        {
            var count = 0;

            foreach (var gunboundImg in _gbImageFile.Images)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    using (var bitmap = new GunboundImageDecoder(gunboundImg).GetImage())
                    {
                        var headerTitle = gunboundImg.Transparency != TransparencyType.SimpleBackground
                                              ? String.Format(Properties.Resources.ImageHeaderLabel, count)
                                              : String.Format(Properties.Resources.BackgroundHeaderLabel, count);
                        var imgFrame = new ImageFrame { FrameHeader = headerTitle };

                        imgFrame.SetImage(bitmap);
                        ImgFrames.Add(imgFrame);
                    }

                    count++;
                }, DispatcherPriority.Background, null);
            }
        }

        private void UpdateImageNames()
        {
            for (var i = 0; i < GbImageFile.Images.Count; i++)
            {
                var gunboundImg = GbImageFile.Images[i];
                gunboundImg.Name = gunboundImg.Transparency != TransparencyType.SimpleBackground
                                              ? String.Format(Properties.Resources.ImageHeaderLabel, i)
                                              : String.Format(Properties.Resources.BackgroundHeaderLabel, i);
                ImgFrames[i].FrameHeader = gunboundImg.Transparency != TransparencyType.SimpleBackground
                                              ? String.Format(Properties.Resources.ImageHeaderLabel, i + 1)
                                              : String.Format(Properties.Resources.BackgroundHeaderLabel, i + 1);
            }
        }

        private void NewImgFile(object sender, ExecutedRoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
                                     {
                                         Filter = "Img Files|*.img"
                                     };

            if (saveFileDialog.ShowDialog() == true)
            {
                GbImageFile = new GunboundImageFile(saveFileDialog.FileName);
                GbImageFile.Create();
                AnimationFile = new GunboundAnimationFile();
                ImgFrames = new ObservableCollection<ImageFrame>();

                comboBoxImageTypes.SelectedIndex = 0;
                lboxImages.SelectedIndex = 0;
                IsOpenedOrCreated = true;
                aniImage.Clear();

                //AnimationFile.Clear();
                //ImgFrames.Clear();
                //_gbImageFile.Clear();
                //_gbImageFile.Path = saveFileDialog.FileName;
                //_gbImageFile.Create();
                //_gbImageFile.LoadImages();

                //UpdateFrames();

                //comboBoxImageTypes.SelectedIndex = 0;
                //lboxImages.SelectedIndex = 0;
                //IsOpenedOrCreated = true;

                SetAvatar(saveFileDialog.FileName);
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        private void BtnOpenBmpClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
                                     {
                                         Filter = "Image Files(*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png",
                                         Multiselect = true
                                     };

            if (openFileDialog.ShowDialog() != true)
                return;

            foreach (var fileName in openFileDialog.FileNames)
            {
                var bmp = new Bitmap(fileName);

                var img =
                    new GunboundImageEncoder(bmp, (TransparencyType)comboBoxAddImageType.SelectedValue, 0, 0).
                        Encode();

                _gbImageFile.AddImage(img);

                var imageFrame = new ImageFrame();
                imageFrame.SetImage(bmp);
                ImgFrames.Add(imageFrame);
                UpdateImageNames();
                //_gbImageFile.Save();
            }

            //var path = _gbImageFile.Path;

            //_gbImageFile.Clear();
            //_gbImageFile.Path = path;
            //_gbImageFile.Load();
            //_gbImageFile.LoadImages();
            //_imgFrames.Clear();
            //UpdateFrames();
        }

        private void BtnSaveImgClick(object sender, RoutedEventArgs e)
        {
            _gbImageFile.Save();
        }

        private void BtnOpenAniFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fInfo = new FileInfo(_gbImageFile.Path);
                var dirInfo = fInfo.DirectoryName;
                var fileNameWithoutExt = fInfo.Name.Substring(0,
                                                              fInfo.Name.IndexOf(fInfo.Extension,
                                                                                 StringComparison.Ordinal));
                AnimationFile.Path = String.Format(@"{0}\{1}.epa", dirInfo, fileNameWithoutExt);
                AnimationFile.Load();
                AnimationFile.LoadTimeLines();
                IsAniOpenedOrCreated = true;
                comboBoxAniTimeLines.SelectedIndex = 0;
                lboxKeyFrames.SelectedIndex = 0;
            }
            catch (GameArchiveException archiveErr)
            {
                MessageBox.Show(archiveErr.Message, Properties.Resources.ErrorLabel, MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(Properties.Resources.ErrorProcessingFile, Properties.Resources.ErrorLabel,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteTimeLineClick(object sender, RoutedEventArgs e)
        {
            AnimationFile.RemoveTimeLine((AnimationTimeline)comboBoxAniTimeLines.SelectedItem);
            comboBoxAniTimeLines.SelectedIndex = 0;
        }

        private void BtnRemoveKeyFrameClick(object sender, RoutedEventArgs e)
        {
            var timeLine = (AnimationTimeline)comboBoxAniTimeLines.SelectedItem;
            timeLine.RemoveFrame((AnimationFrame)lboxKeyFrames.SelectedItem);

            var expression = lboxKeyFrames.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (expression != null)
                expression.UpdateTarget();

            lboxKeyFrames.SelectedIndex = 0;
        }

        private void BtnCreateAniFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fInfo = new FileInfo(_gbImageFile.Path);
                var dirInfo = fInfo.DirectoryName;
                var fileNameWithoutExt = fInfo.Name.Substring(0,
                                                              fInfo.Name.IndexOf(fInfo.Extension,
                                                                                 StringComparison.Ordinal));

                var fstream = File.Create(String.Format(@"{0}\{1}.epa", dirInfo, fileNameWithoutExt));
                AnimationFile.Path = fstream.Name;
                fstream.Close();

                AnimationFile.Create();
                AnimationFile.LoadTimeLines();
                IsAniOpenedOrCreated = true;
                comboBoxAniTimeLines.SelectedIndex = 0;
                lboxKeyFrames.SelectedIndex = 0;

            }
            catch (GameArchiveException archiveErr)
            {
                MessageBox.Show(archiveErr.Message, Properties.Resources.ErrorLabel, MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(Properties.Resources.ErrorProcessingFile, Properties.Resources.ErrorLabel,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCreateTimeLineClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtAniType.Text))
            {
                MessageBox.Show(Properties.Resources.EmptyAnimationType, Properties.Resources.AlertLabel,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (_animationFile.TimeLines.Any(animationTimeline => animationTimeline.AnimationType == txtAniType.Text))
            {
                MessageBox.Show(Properties.Resources.AnimationTypeExists, Properties.Resources.AlertLabel,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            _animationFile.AddTimeLine(new AnimationTimeline(txtAniType.Text));
        }

        private void BtnAddKeyFrameClick(object sender, RoutedEventArgs e)
        {
            var timeline = (AnimationTimeline)comboBoxAniTimeLines.SelectedItem;
            timeline.AddFrame(new AnimationFrame());
        }

        private void BtnSaveAnimationClick(object sender, RoutedEventArgs e)
        {
            _animationFile.Save();
        }

        private void ExpanderAnimationExpanded(object sender, RoutedEventArgs e)
        {
            Height = 786;
            CenterWindowOnScreen();
        }

        private void ExpanderAnimationCollapsed(object sender, RoutedEventArgs e)
        {
            Height = 450;
            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void BtnExportClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() != WinForms.DialogResult.OK) return;

            var cont = 1;

            foreach (var decoder in _gbImageFile.Images.Select(gunboundImg => new GunboundImageDecoder(gunboundImg)))
            {
                using (var bmp = decoder.GetImage())
                {
                    if (cboxImgType.IsChecked == true)
                    {
                        bmp.Save(String.Format(@"{0}\Image {1}.bmp", folderDialog.SelectedPath, cont), ImageFormat.Bmp);
                    }
                    else
                    {
                        bmp.Save(String.Format(@"{0}\Image {1}.png", folderDialog.SelectedPath, cont), ImageFormat.Png);
                    }
                    cont++;
                }
            }
        }

        private void BtnDeleteImageClick(object sender, RoutedEventArgs e)
        {
            var selected = lboxImages.SelectedItem;
            var gbimg = selected as GunboundImg;
            var selectedIndex = lboxImages.SelectedIndex;

            if (gbimg == null)
                return;

            GbImageFile.Images.Remove(gbimg);
            ImgFrames.RemoveAt(selectedIndex);

            lboxImages.SelectedIndex = selectedIndex > 0 ? --selectedIndex : selectedIndex;

            //var gbimg = lboxImages.SelectedItem as GunboundImg;
            //var selectedIndex = lboxImages.SelectedIndex;

            //if (gbimg == null)
            //    return;

            //_gbImageFile.Images.Remove(gbimg);

            //_imgFrames.Clear();
            //UpdateFrames();

            //lboxImages.SelectedIndex = selectedIndex > 0 ? --selectedIndex : selectedIndex;
        }

        private void ChangeLanguage(string lang)
        {
            File.SetAttributes(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, FileAttributes.Normal);

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("Culture");
            config.AppSettings.Settings.Add("Culture", lang);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Properties.Resources.Culture = new System.Globalization.CultureInfo(lang);

            File.SetAttributes(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, FileAttributes.ReadOnly);
        }

        private void LoadLanguageMenu()
        {
            var lang = ConfigurationManager.AppSettings["Culture"];

            foreach (var mnuItem in FindVisualChildren<MenuItem>(menuLanguages))
            {
                var menuTag = mnuItem.Tag.ToString();
                mnuItem.IsChecked = menuTag == lang;
            }
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                var objectCount = VisualTreeHelper.GetChildrenCount(depObj);

                for (var i = 0; i < objectCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void MenuLanguageClick(object sender, RoutedEventArgs e)
        {
            ChangeLanguage(((MenuItem)sender).Tag.ToString());
            MessageBox.Show(this, Properties.Resources.LenguageChanged, Properties.Resources.LanguageChangedLabel,
                            MessageBoxButton.OK, MessageBoxImage.Information);
            LoadLanguageMenu();
        }

        private void BtnResetClick(object sender, RoutedEventArgs e)
        {
            ResetPlayer();
        }

        private void ResetPlayer()
        {
            var headFile = new GunboundImageFile();
            headFile.SetData(Properties.Resources.mh00000);
            var bodyFile = new GunboundImageFile();
            bodyFile.SetData(Properties.Resources.mb00000);
            _player = new Player(headFile, bodyFile);

            avaGlassesPreviewImage.Clear();
            avaFlagPreviewImage.Clear();

            _player.LoadImages();
        }

        private void BtnSetAvatarClick(object sender, RoutedEventArgs e)
        {
            ResetPlayer();

            if (_gbImageFile.Images.Count > 0)
                SetAvatar(_gbImageFile.Path);
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = tabCtrlMain.SelectedIndex;

            if (_aniTimer == null || _avaTimer == null)
                return;

            if (x == 1)
            {
                _aniTimer.Start();
                _avaTimer.Stop();
            }
            else
            {
                if (x == 2)
                {
                    _aniTimer.Stop();
                    _avaTimer.Start();
                }
                else
                {
                    _aniTimer.Stop();
                    _avaTimer.Stop();
                }
            }
        }

        private void BtnMoveImgUpClick(object sender, RoutedEventArgs e)
        {
            var selected = lboxImages.SelectedItem;

            if (selected == null)
                return;

            var oldIndex = lboxImages.Items.IndexOf(selected);

            if (oldIndex < 1)
                return;

            GbImageFile.Images.Move(oldIndex, oldIndex - 1);
            ImgFrames.Move(oldIndex, oldIndex - 1);
            lboxImages.ScrollIntoView(selected);
        }

        private void BtnMoveImgDownClick(object sender, RoutedEventArgs e)
        {
            var selected = lboxImages.SelectedItem;

            if (selected == null)
                return;

            var oldIndex = lboxImages.Items.IndexOf(selected);

            if (oldIndex >= lboxImages.Items.Count - 1)
                return;

            GbImageFile.Images.Move(oldIndex, oldIndex + 1);
            ImgFrames.Move(oldIndex, oldIndex + 1);
            lboxImages.ScrollIntoView(selected);
        }

        private void BtnSelectFolderClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() != WinForms.DialogResult.OK)
                return;

            var path = folderDialog.SelectedPath;

            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles().Where(f => f.Extension.ToLower() == ".img");
            ImgFiles = new ObservableCollection<FileInfo>(files);
        }

        private void ImgFilesDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = (FileInfo)((ListBoxItem)sender).Content;

            if (selected == null)
                return;

            OpenImg(selected.FullName);
        }
    }
}
