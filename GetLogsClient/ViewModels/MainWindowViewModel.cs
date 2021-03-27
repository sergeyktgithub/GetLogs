using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GetLogsClient.ListBoxContent;
using GetLogsClient.Models;
using GetLogsClient.NetServices;
using NetCom;
using NetCom.Helpers;
using NetComModels;
using ZipExtension;
using Msg = GetLogsClient.ListBoxContent.Msg;

namespace GetLogsClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private bool _radioButtonIlogsChecked;
        private bool _radioButtonLogsChecked;
        private DateTime _fromDateTime;
        private DateTime _toDateTime;
        private string _saveDirectory;
        private int _accIdIndex;
        private string _accIdText;
        private LastState _lastState = new LastState();
        private LoaderFile _loaderFile;
        private CancellationTokenSource _cts;
        private string _accIdSelected;
        private int _selectedPatternIndex;
        private string _patternText;
        private bool _buttonLoadLogsEnabled;
        private bool _buttonGiveGlanceEnabled;
        private Msg _selectedMsg;
        private List<PreparedArchive> _preparedArchives;
        private PackageQueue _packageQueue;
        private UdpListener _udpListener;
        private AboutLogsOnList _aboutLogsOnList;
        private AboutILogsOnList _aboutILogsOnList;
        private bool _isDownload;

        public bool RadioButtonIlogsChecked
        {
            get => _radioButtonIlogsChecked;
            set => SetProperty(ref _radioButtonIlogsChecked, value, nameof(RadioButtonIlogsChecked), () =>
            {
                ButtonGiveGlanceEnabled = true;
                ButtonLoadLogsEnabled = false;
            }, () => true);
        }

        public bool RadioButtonLogsChecked
        {
            get => _radioButtonLogsChecked;
            set => SetProperty(ref _radioButtonLogsChecked, value, nameof(RadioButtonLogsChecked), () =>
                {
                    ButtonGiveGlanceEnabled = true;
                    ButtonLoadLogsEnabled = false;
                }, () => true);
        }

        public bool ButtonGiveGlanceEnabled
        {
            get => _buttonGiveGlanceEnabled;
            set => SetProperty(ref _buttonGiveGlanceEnabled, value, nameof(ButtonGiveGlanceEnabled));
        }

        public bool ButtonLoadLogsEnabled
        {
            get => _buttonLoadLogsEnabled;
            set => SetProperty(ref _buttonLoadLogsEnabled, value, nameof(ButtonLoadLogsEnabled));
        }

        public DateTime FromDateTime
        {
            get => _fromDateTime;
            set => SetProperty(ref _fromDateTime, value, nameof(FromDateTime), SaveState, () => _lastState.FromDateTime != value);
        }

        public DateTime ToDateTime  
        {
            get => _toDateTime;
            set => SetProperty(ref _toDateTime, value, nameof(ToDateTime), SaveState, () => _lastState.ToDateTime != value);
        }

        public string SaveDirectory
        {
            get => _saveDirectory;
            set => SetProperty(ref _saveDirectory, value, nameof(SaveDirectory), SaveState, () => _lastState.SavePath != value);
        }

        public ObservableCollection<string> AccIdList { get; set; }

        public int AccIdIndex
        {
            get => _accIdIndex;
            set => SetProperty(ref _accIdIndex, value, nameof(AccIdIndex), SaveState, () => value > -1 && _lastState.AccIdIndex != value);
        }

        public string AccIdText
        {
            get => _accIdText;
            set => SetProperty(ref _accIdText, value, nameof(AccIdText), () =>
            {
                AccIdList.Add(value);
                SaveState();
            }, () => !string.IsNullOrEmpty(value) && !AccIdList.Contains(value));
        }

        public string AccIdSelected
        {
            get => _accIdSelected;
            set => SetProperty(ref _accIdSelected, value);
        }

        public ObservableCollection<string> PatternList { get; set; }

        public int SelectedPatternIndex
        {
            get => _selectedPatternIndex;
            set => SetProperty(ref _selectedPatternIndex, value, nameof(SelectedPatternIndex), 
                SaveState,() => value > -1 && _lastState.SelectedPatternIndex != value);
        }

        public string PatternText
        {
            get => _patternText;
            set => SetProperty(ref _patternText, value, nameof(PatternText), () =>
            {
                PatternList.Add(value);
                SaveState();
            }, () => !PatternList.Contains(value));
        }

        private string GetLogsClientPath { get; set; }

        private string LastStateJsonFilePath { get; set; }

        public ObservableCollection<Msg> MsgList { get; set; }

        public Msg SelectedMsg
        {
            get => _selectedMsg;
            set => SetProperty(ref _selectedMsg, value);
        }

        public MainWindowViewModel()
        {
            _cts = new CancellationTokenSource();

            GetLogsClientPath = Environment.GetEnvironmentVariable("APPDATA") + "\\GetLogsClient";
            LastStateJsonFilePath = Environment.GetEnvironmentVariable("APPDATA") + "\\GetLogsClient\\LastState.json";

            AccIdList = new ObservableCollection<string>();
            PatternList = new ObservableCollection<string>();

            ButtonLoadLogsEnabled = false;
            ButtonGiveGlanceEnabled = true;

            RadioButtonLogsChecked = true;

            MsgList = new ObservableCollection<Msg>();
        }

        public void ExternalInitializer()
        {
            ReadLastState();
            InitializeNetworkServices();
        }

        public void ViewFileInExternalEditor()
        {
            if (SelectedMsg != null && SelectedMsg is FileLogMsg fileLog && File.Exists(fileLog.Path))
            {
                var ext = Path.GetExtension(fileLog.Path);
                if (ext == ".txt")
                {
                    var pathToNotepad = "C:\\Program Files (x86)\\Notepad++\\notepad++.exe";
                    if (File.Exists(pathToNotepad) == false)
                    {
                        MessageBox.Show($"Notepad++ не найден по пути: {pathToNotepad}");
                        return;
                    }

                    var filePath = "\"" + fileLog.Path.Replace("/", "\\") + "\"";
                    Process.Start(pathToNotepad, filePath);
                }
                
                if (ext == ".ilog")
                {
                    var pathToIlogPlayer = "c:\\Program Files (x86)\\IlogPlayer\\IlogPlayer.exe";

                    if (File.Exists(pathToIlogPlayer) == false)
                    {
                        MessageBox.Show($"IlogPlayer не найден по пути: {pathToIlogPlayer}");
                        return;
                    }

                    var filePath = "\"" + fileLog.Path.Replace("/", "\\") + "\"";
                    var argument = "--path=" + filePath;
                    Process.Start(pathToIlogPlayer, argument);
                }
            }
        }

        public async void DownloadArchive()
        {
            AddMsg("Подготовка загрузки ...");

            try
            {
                if (string.IsNullOrEmpty(SaveDirectory))
                {
                    AddMsg("Не задан путь сохранения");
                    return;
                }

                _packageQueue.Clear();

                if (_preparedArchives.Count > 0)
                {
                    foreach (var preparedArchive in _preparedArchives)
                    {
                        var archNameWithoutExt = Path.GetFileNameWithoutExtension(preparedArchive.Name);
                        var archDir = Path.Combine(SaveDirectory, archNameWithoutExt ?? throw new InvalidOperationException());
                        var archPath = Path.Combine(archDir, preparedArchive.Name);

                        Directory.CreateDirectory(archDir);

                        await _loaderFile.FileRequestAsync(preparedArchive.FullPath, archPath, preparedArchive.Source);

                        AddMsg($"Извлечение архива: {archPath}");
                        var files = Zip.ExtractToArchiveDirectory(archPath);

                        foreach (var file in files)
                        {
                            AddMsg(new FileLogMsg(file));
                        }
                    }
                }
                else
                {
                    ButtonGiveGlanceEnabled = false;
                    AddMsg("Нечего загружать повторите поиск");
                }
            }
            catch (Exception ex)
            {
                AddMsg($"Ошибка: {ex.Message}");
            }
        }

        public async void GiveGlanceLogFiles()
        {
            try
            {
                ButtonGiveGlanceEnabled = false;

                AddMsg("Ждемс ...");

                var pattern = "";
                if (!string.IsNullOrEmpty(PatternText))
                {
                    pattern = PatternText;
                }

                var accId = AccIdSelected;
                if (string.IsNullOrEmpty(accId))
                {
                    AddMsg($"AccId не задан");
                    return;
                }

                ButtonLoadLogsEnabled = false;

                _packageQueue.Clear();

                if (RadioButtonLogsChecked)
                {
                    _preparedArchives = await _aboutLogsOnList.CheckExistLogsAsync(pattern, accId, FromDateTime, ToDateTime);
                }

                if (RadioButtonIlogsChecked)
                {
                    _preparedArchives = await _aboutILogsOnList.CheckExistILogsAsync(accId, FromDateTime, ToDateTime);
                }

                if (_preparedArchives.Count > 0)
                {
                    foreach (var preparedArchive in _preparedArchives)
                    {
                        foreach (var file in preparedArchive.Files)
                        {
                            AddMsg(new FileLogMsg(file));
                        }

                        var totalSizeMb = preparedArchive.TotalSize / 1024 / 1024;

                        if (string.IsNullOrEmpty(pattern))
                            AddMsg($"{preparedArchive.Source} : Найдено файл(ов): {preparedArchive.Files.Count}, Размер = {totalSizeMb} Mb, From = {FromDateTime}, To = {ToDateTime}");
                        else
                            AddMsg($"{preparedArchive.Source} : Найдено файл(ов): {preparedArchive.Files.Count}, Размер = {totalSizeMb} Mb, Pattern = {pattern}, From = {FromDateTime}, To = {ToDateTime}");
                    }
                   
                    ButtonLoadLogsEnabled = true;
                }
                else
                {
                    AddMsg($"Не удалось найти логи: Pattern = {pattern}, From = {FromDateTime}, To = {ToDateTime}");
                    _preparedArchives = null;
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
            finally
            {
                ButtonGiveGlanceEnabled = true;
            }
        }

        public void Dispose()
        {
            SaveState();

            _cts?.Dispose();
            _loaderFile?.Dispose();
            _packageQueue?.Dispose();
        }

        private void ReadLastState()
        {
            if (File.Exists(LastStateJsonFilePath) == false)
            {
                AddMsg($"Не удалось загрузить последнее состояние, файл не найден {LastStateJsonFilePath}");
                return;
            }

            var lastState = JsonSerializer.Deserialize<LastState>(File.ReadAllText(LastStateJsonFilePath));

            if (lastState.AccIdList != null)
            {
                foreach (var item in lastState.AccIdList)
                {
                    AccIdList.Add(item);
                }
            }

            AccIdIndex = lastState.AccIdIndex < 0 ? 0 : lastState.AccIdIndex;

            if (lastState.PatternList != null)
            {
                foreach (var item in lastState.PatternList)
                {
                    PatternList.Add(item);
                }
            }

            SelectedPatternIndex = lastState.SelectedPatternIndex < 0 ? 0 : lastState.SelectedPatternIndex;

            FromDateTime = lastState.FromDateTime;
            ToDateTime = lastState.ToDateTime;
            SaveDirectory = string.IsNullOrEmpty(lastState.SavePath) ? "" : lastState.SavePath;

            UpdateLastStateConfig();
        }

        private void SaveState()
        {
            if (_lastState == null)
            {
                return;
            }

            UpdateLastStateConfig();

            var json = JsonSerializer.Serialize(_lastState);

            if (!Directory.Exists(GetLogsClientPath))
                Directory.CreateDirectory(GetLogsClientPath);

            File.WriteAllText(LastStateJsonFilePath, json);
        }

        private void InitializeNetworkServices()
        {
            _udpListener = new UdpListener(GlobalProperties.ClientMsgPort, _cts.Token);
            _packageQueue = new PackageQueue(_udpListener, _cts.Token);

            var srcMsgEndPoint = new IPEndPoint(IPAddress.Any, GlobalProperties.ClientMsgPort);
            var srcFileEndPoint = new IPEndPoint(IPAddress.Any, NetHelper.GetRandomServerPort());
            
            List<TwoEndPoints> _twoEndPointsList = new List<TwoEndPoints>();
            //string[] ipList = { "10.99.132.100", "255.255.255.255", "10.100.16.100", "10.100.23.100", "10.100.45.100" };
            //string[] ipList = {"10.99.132.100", "10.11.193.84" };
            string[] ipList = {
                "10.99.122.100",
                "10.99.123.100",
                "10.99.124.100",
                "10.99.125.100",
                "10.99.126.100",
                "10.99.127.100",
                "10.99.128.100",
                "10.99.129.100",
                "10.99.130.100",
                "10.99.131.100",
                "10.99.132.100",
                "10.99.133.100",
                "10.99.134.100",
                "10.99.135.100",
                "10.99.136.100",
                "10.99.137.100",
                "10.99.138.100",
                "10.99.139.100",
                "10.99.140.100",
                "10.99.141.100",
                "10.99.142.100",
                "10.99.143.100",
                "10.99.144.100",
                "10.99.145.100",
                "10.99.146.100",
                "10.99.147.100",
                "10.99.99.16",
                "10.99.149.100",
                "10.99.150.100",
                "10.99.151.100",
                "10.99.152.100",
                "10.99.153.100",
                "10.99.154.100",
                "10.99.155.100",
                "10.99.156.100",
                "10.99.159.100",
                "10.99.160.100",
                "10.100.2.100",
                "10.100.3.100",
                "10.100.4.100",
                "10.100.5.100",
                "10.100.6.100",
                "10.100.7.100",
                "10.100.8.100",
                "10.100.9.100",
                "10.100.10.100",
                "10.100.11.100",
                "10.100.12.100",
                "10.100.13.100",
                "10.100.14.100",
                "10.100.15.100",
                "10.100.16.100",
                "10.100.17.100",
                "10.100.18.100",
                "10.100.19.100",
                "10.100.20.100",
                "10.100.21.100",
                "84.38.185.129",
                "10.100.23.100",
                "10.100.24.100",
                "31.184.215.62",
                "84.38.185.111",
                "10.100.27.100",
                "10.100.29.100",
                "10.100.33.100",
                "10.100.34.100",
                "10.100.35.100",
                "10.100.36.100",
                "10.100.37.100",
                "10.100.38.100",
                "10.100.39.100",
                "10.100.40.100",
                "10.100.41.100",
                "10.100.44.100",
                "10.100.45.100",
                "10.100.46.100",
                "10.100.48.100",
                "10.100.59.100",
                "10.100.60.100",
                "10.100.61.100",
                "10.100.62.100",
                "10.100.63.100",
                "10.100.64.100",
                "10.100.65.100",
                "10.100.66.100",
                "10.100.67.100",
                "10.100.68.100",
                "10.100.70.100",
                "10.100.71.100",
                "10.100.72.100",
                "10.100.73.100",
                "10.100.74.100",
                "10.100.75.100",
                "10.100.76.100",
                "10.100.77.100",
                "10.100.78.100",
                "10.100.79.100",
                "10.100.80.100",
                "10.100.81.100",
                "10.100.82.100",
                "10.100.83.100",
                "10.100.84.100",
                "10.100.85.100",
                "10.100.86.100",
                "10.100.87.100",
                "10.100.88.100"
            };

            foreach (var ip in ipList)
            {
                var destBroadcastEndPoint = new IPEndPoint(IPAddress.Parse(ip), GlobalProperties.ServerMsgPort);
                _twoEndPointsList.Add(new TwoEndPoints(srcMsgEndPoint, destBroadcastEndPoint));
            }

            _aboutLogsOnList = new AboutLogsOnList(_twoEndPointsList, _packageQueue);
            _aboutLogsOnList.ProcessMsgEvent += (sender, str) => AddMsg(str);

            _aboutILogsOnList = new AboutILogsOnList(_twoEndPointsList, _packageQueue);
            _aboutILogsOnList.ProcessMsgEvent += (sender, str) => AddMsg(str);

            _loaderFile = new LoaderFile(srcMsgEndPoint, srcFileEndPoint, _packageQueue, _cts.Token);
            _loaderFile.BeginDownloadEvent += (sender, str) => AddMsg(str);
            _loaderFile.EndDownloadEvent += (sender, str) => AddMsg(str);
            _loaderFile.ErrorEvent += (sender, str) => AddMsg(str);
            _loaderFile.GeneralMsgEvent += (sender, str) => AddMsg(str);

            int maxpercent = 0;
            _loaderFile.ProgressEvent += (sender, received, totalBytes) =>
            {
                int percent = (int)(received / (float) totalBytes * 100);
                if (percent > 0 && percent % 10 == 0 && percent > maxpercent)
                {
                    maxpercent = percent;
                    AddMsg($"Загружено: {(int)percent}%");
                }
            };
        }

        private void UpdateLastStateConfig()
        {
            _lastState.AccIdList = AccIdList.ToArray();
            _lastState.AccIdIndex = AccIdIndex;

            _lastState.PatternList = PatternList.ToArray();
            _lastState.SelectedPatternIndex = SelectedPatternIndex;

            _lastState.FromDateTime = _fromDateTime;
            _lastState.ToDateTime = _toDateTime;
            _lastState.SavePath = SaveDirectory;
        }

        private void AddMsg(Msg msg)
        {
            Application.Current.Dispatcher.Invoke(() => { MsgList.Insert(0, msg); });
        }

        private void AddMsg(string msg)
        {
            AddMsg(new Msg($"[{DateTime.Now:T}] : {msg}"));
        }
    }
}