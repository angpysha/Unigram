using System;
using System.Linq;
using System.Text;
using Unigram.Common;
using Unigram.Services;
using Unigram.Services.Settings;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Unigram.Services
{
    public interface ISettingsService
    {
        int Session { get; }
        ulong Version { get; }

        void UpdateVersion();

        NotificationsSettings Notifications { get; }
        StickersSettings Stickers { get; }
        AutoDownloadSettings AutoDownload { get; set; }
        AppearanceSettings Appearance { get; }
        PasscodeLockSettings PasscodeLock { get; }

        int UserId { get; set; }

        bool IsWorkModeVisible { get; set; }
        bool IsWorkModeEnabled { get; set; }

        string FilesDirectory { get; set; }

        int VerbosityLevel { get; }
        bool UseTestDC { get; set; }

        bool IsSendByEnterEnabled { get; set; }
        bool IsReplaceEmojiEnabled { get; set; }
        bool IsContactsSyncEnabled { get; set; }
        bool IsContactsSyncRequested { get; set; }
        bool IsSecretPreviewsEnabled { get; set; }
        bool IsAutoPlayEnabled { get; set; }
        bool IsSendGrouped { get; set; }

        int LastMessageTtl { get; set; }

        string NotificationsToken { get; set; }
        int[] NotificationsIds { get; set; }

        int SelectedBackground { get; set; }
        int SelectedColor { get; set; }

        int PeerToPeerMode { get; set; }
        libtgvoip.DataSavingMode UseLessData { get; set; }

        void SetChatPinnedMessage(long chatId, long messageId);
        long GetChatPinnedMessage(long chatId);

        void Clear();
    }

    public class SettingsServiceBase
    {
        protected readonly ApplicationDataContainer _container;

        public SettingsServiceBase(ApplicationDataContainer container = null)
        {
            _container = container ?? ApplicationData.Current.LocalSettings;
        }



        public bool AddOrUpdateValue(string key, Object value)
        {
            return AddOrUpdateValue(_container, key, value);
        }

        protected bool AddOrUpdateValue(ApplicationDataContainer container, string key, Object value)
        {
            bool valueChanged = false;

            if (container.Values.ContainsKey(key))
            {
                if (container.Values[key] != value)
                {
                    container.Values[key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                container.Values.Add(key, value);
                valueChanged = true;
            }

            return valueChanged;
        }


        public valueType GetValueOrDefault<valueType>(string key, valueType defaultValue)
        {
            return GetValueOrDefault<valueType>(_container, key, defaultValue);
        }

        protected valueType GetValueOrDefault<valueType>(ApplicationDataContainer container, string key, valueType defaultValue)
        {
            valueType value;

            if (container.Values.ContainsKey(key))
            {
                value = (valueType)container.Values[key];
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }

        public virtual void Clear()
        {
            _container.Values.Clear();
        }
    }

    public class SettingsService : SettingsServiceBase, ISettingsService
    {
        private static SettingsService _current;
        public static SettingsService Current
        {
            get
            {
                if (_current == null)
                    _current = new SettingsService();

                return _current;
            }
        }

        private readonly int _session;
        private readonly ApplicationDataContainer _local;
        private readonly ApplicationDataContainer _own;

        private SettingsService()
        {
            _local = ApplicationData.Current.LocalSettings;
        }

        public SettingsService(int session)
            : base(session > 0 ? ApplicationData.Current.LocalSettings.CreateContainer(session.ToString(), ApplicationDataCreateDisposition.Always) : null)
        {
            _session = session;
            _local = ApplicationData.Current.LocalSettings;
            _own = ApplicationData.Current.LocalSettings.CreateContainer($"{session}", ApplicationDataCreateDisposition.Always);
        }

        #region App version

        public const ulong CurrentVersion = (2UL << 48) | (2UL << 32) | (1850UL << 16);
        public const string CurrentChangelog = "- Sort pinned chats from Settings > Advanced.\r\n- Clear recent stickers from Settings > Stickers.\r\n- Bug fixes and improvements.";

        public int Session => _session;

        private ulong? _version;
        public ulong Version
        {
            get
            {
                if (_version == null)
                    _version = GetValueOrDefault("LongVersion", 0UL);

                return _version ?? 0;
            }
            private set
            {
                _version = value;
                AddOrUpdateValue("LongVersion", value);
            }
        }

        public void UpdateVersion()
        {
            Version = CurrentVersion;
        }

        #endregion

        private NotificationsSettings _notifications;
        public NotificationsSettings Notifications
        {
            get
            {
                return _notifications = _notifications ?? new NotificationsSettings(_container);
            }
        }

        private StickersSettings _stickers;
        public StickersSettings Stickers
        {
            get
            {
                return _stickers = _stickers ?? new StickersSettings(_container);
            }
        }

        private AutoDownloadSettings _autoDownload;
        public AutoDownloadSettings AutoDownload
        {
            get
            {
                return _autoDownload = _autoDownload ?? new AutoDownloadSettings(_own.CreateContainer("AutoDownload", ApplicationDataCreateDisposition.Always));
            }
            set
            {
                _autoDownload = value ?? AutoDownloadSettings.Default;
                _autoDownload.Save(_own.CreateContainer("AutoDownload", ApplicationDataCreateDisposition.Always));
            }
        }

        private static AppearanceSettings _appearance;
        public AppearanceSettings Appearance
        {
            get
            {
                return _appearance = _appearance ?? new AppearanceSettings();
            }
        }

        private static PasscodeLockSettings _passcodeLock;
        public PasscodeLockSettings PasscodeLock
        {
            get
            {
                return _passcodeLock = _passcodeLock ?? new PasscodeLockSettings();
            }
        }

        private bool? _isWorkModeVisible;
        public bool IsWorkModeVisible
        {
            get
            {
                if (_isWorkModeVisible == null)
                    _isWorkModeVisible = GetValueOrDefault("IsWorkModeVisible", false);

                return _isWorkModeVisible ?? false;
            }
            set
            {
                _isWorkModeVisible = value;
                AddOrUpdateValue("IsWorkModeVisible", value);
            }
        }

        private bool? _isWorkModeEnabled;
        public bool IsWorkModeEnabled
        {
            get
            {
                if (_isWorkModeEnabled == null)
                    _isWorkModeEnabled = GetValueOrDefault("IsWorkModeEnabled", false);

                return _isWorkModeEnabled ?? false;
            }
            set
            {
                _isWorkModeEnabled = value;
                AddOrUpdateValue("IsWorkModeEnabled", value);
            }
        }

        private string _filesDirectory;
        public string FilesDirectory
        {
            get
            {
                if (_filesDirectory == null)
                    _filesDirectory = GetValueOrDefault("FilesDirectory", null as string);

                return _filesDirectory;
            }
            set
            {
                _filesDirectory = value;
                AddOrUpdateValue("FilesDirectory", value);
            }
        }

        private int? _verbosityLevel;
        public int VerbosityLevel
        {
            get
            {
                if (_verbosityLevel == null)
#if DEBUG
                    _verbosityLevel = GetValueOrDefault(_local, "VerbosityLevel", 5);

                return _verbosityLevel ?? 5;
#else
                    _verbosityLevel = GetValueOrDefault(_local, "VerbosityLevel", 0);

                return _verbosityLevel ?? 0;
#endif
            }
            set
            {
                _verbosityLevel = value;
                AddOrUpdateValue(_local, "VerbosityLevel", value);
            }
        }

        private bool? _useTestDC;
        public bool UseTestDC
        {
            get
            {
                if (_useTestDC == null)
                    _useTestDC = GetValueOrDefault(_local, "UseTestDC", false);

                return _useTestDC ?? false;
            }
            set
            {
                _useTestDC = value;
                AddOrUpdateValue(_local, "UseTestDC", value);
            }
        }

        private int? _userId;
        public int UserId
        {
            get
            {
                if (_userId == null)
                    _userId = GetValueOrDefault(_own, "UserId", 0);

                return _userId ?? 0;
            }
            set
            {
                _userId = value;
                AddOrUpdateValue(_local, $"User{value}", Session);
                AddOrUpdateValue(_own, "UserId", value);
            }
        }

        private double? _dialogsWidthRatio;
        public double DialogsWidthRatio
        {
            get
            {
                if (_dialogsWidthRatio == null)
                    _dialogsWidthRatio = GetValueOrDefault(_local, "DialogsWidthRatio", 5d / 14d);

                return _dialogsWidthRatio ?? 5d / 14d;
            }
            set
            {
                _dialogsWidthRatio = value;
                AddOrUpdateValue(_local, "DialogsWidthRatio", value);
            }
        }

        private bool? _isSendByEnterEnabled;
        public bool IsSendByEnterEnabled
        {
            get
            {
                if (_isSendByEnterEnabled == null)
                    _isSendByEnterEnabled = GetValueOrDefault("IsSendByEnterEnabled", true);

                return _isSendByEnterEnabled ?? true;
            }
            set
            {
                _isSendByEnterEnabled = value;
                AddOrUpdateValue("IsSendByEnterEnabled", value);
            }
        }

        private bool? _isReplaceEmojiEnabled;
        public bool IsReplaceEmojiEnabled
        {
            get
            {
                if (_isReplaceEmojiEnabled == null)
                    _isReplaceEmojiEnabled = GetValueOrDefault("IsReplaceEmojiEnabled", true);

                return _isReplaceEmojiEnabled ?? true;
            }
            set
            {
                _isReplaceEmojiEnabled = value;
                AddOrUpdateValue("IsReplaceEmojiEnabled", value);
            }
        }

        private bool? _isContactsSyncEnabled;
        public bool IsContactsSyncEnabled
        {
            get
            {
                if (_isContactsSyncEnabled == null)
                    _isContactsSyncEnabled = GetValueOrDefault("IsContactsSyncEnabled", true);

                return _isContactsSyncEnabled ?? true;
            }
            set
            {
                _isContactsSyncEnabled = value;
                AddOrUpdateValue("IsContactsSyncEnabled", value);
            }
        }

        private bool? _isContactsSyncRequested;
        public bool IsContactsSyncRequested
        {
            get
            {
                if (_isContactsSyncRequested == null)
                    _isContactsSyncRequested = GetValueOrDefault("IsContactsSyncRequested", false);

                return _isContactsSyncRequested ?? true;
            }
            set
            {
                _isContactsSyncRequested = value;
                AddOrUpdateValue("IsContactsSyncRequested", value);
            }
        }

        private bool? _isSecretPreviewsEnabled;
        public bool IsSecretPreviewsEnabled
        {
            get
            {
                if (_isSecretPreviewsEnabled == null)
                    _isSecretPreviewsEnabled = GetValueOrDefault("IsSecretPreviewsEnabled", false);

                return _isSecretPreviewsEnabled ?? true;
            }
            set
            {
                _isSecretPreviewsEnabled = value;
                AddOrUpdateValue("IsSecretPreviewsEnabled", value);
            }
        }

        private bool? _isAutoPlayEnabled;
        public bool IsAutoPlayEnabled
        {
            get
            {
                if (_isAutoPlayEnabled == null)
                    _isAutoPlayEnabled = GetValueOrDefault("IsAutoPlayEnabled", true);

                return _isAutoPlayEnabled ?? true;
            }
            set
            {
                _isAutoPlayEnabled = value;
                AddOrUpdateValue("IsAutoPlayEnabled", value);
            }
        }

        private bool? _isSendGrouped;
        public bool IsSendGrouped
        {
            get
            {
                if (_isSendGrouped == null)
                    _isSendGrouped = GetValueOrDefault("IsSendGrouped", true);

                return _isSendGrouped ?? true;
            }
            set
            {
                _isSendGrouped = value;
                AddOrUpdateValue("IsSendGrouped", value);
            }
        }

        private int? _lastMessageTtl;
        public int LastMessageTtl
        {
            get
            {
                if (_lastMessageTtl == null)
                    _lastMessageTtl = GetValueOrDefault("LastMessageTtl", 7);

                return _lastMessageTtl ?? 7;
            }
            set
            {
                _lastMessageTtl = value;
                AddOrUpdateValue("LastMessageTtl", value);
            }
        }

        private int? _previousSession;
        public int PreviousSession
        {
            get
            {
                if (_previousSession == null)
                    _previousSession = GetValueOrDefault(_local, "PreviousSession", 0);

                return _activeSession ?? 0;
            }
            set
            {
                _previousSession = value;
                AddOrUpdateValue(_local, "PreviousSession", value);
            }
        }

        private int? _activeSession;
        public int ActiveSession
        {
            get
            {
                if (_activeSession == null)
                    _activeSession = GetValueOrDefault(_local, "SelectedAccount", 0);

                return _activeSession ?? 0;
            }
            set
            {
                _activeSession = value;
                AddOrUpdateValue(_local, "SelectedAccount", value);
            }
        }

        private string _notificationsToken;
        public string NotificationsToken
        {
            get
            {
                if (_notificationsToken == null)
                    _notificationsToken = GetValueOrDefault<string>(_local, "ChannelUri", null);

                return _notificationsToken;
            }
            set
            {
                _notificationsToken = value;
                AddOrUpdateValue(_local, "ChannelUri", value);
            }
        }

        private int[] _notificationsIds;
        public int[] NotificationsIds
        {
            get
            {
                if (_notificationsIds == null)
                {
                    var value = GetValueOrDefault<string>(_local, "NotificationsIds", null);
                    if (value == null)
                    {
                        _notificationsIds = new int[0];
                    }
                    else
                    {
                        _notificationsIds = value.Split(',').Select(x => int.Parse(x)).ToArray();
                    }
                }

                return _notificationsIds;
            }
            set
            {
                _notificationsIds = value;
                AddOrUpdateValue(_local, "NotificationsIds", string.Join(",", value));
            }
        }

        private int? _selectedBackground;
        public int SelectedBackground
        {
            get
            {
                if (_selectedBackground == null)
                    _selectedBackground = GetValueOrDefault("SelectedBackground", 1000001);

                return _selectedBackground ?? 1000001;
            }
            set
            {
                _selectedBackground = value;
                AddOrUpdateValue("SelectedBackground", value);
            }
        }

        private int? _selectedColor;
        public int SelectedColor
        {
            get
            {
                if (_selectedColor == null)
                    _selectedColor = GetValueOrDefault("SelectedColor", 0);

                return _selectedColor ?? 0;
            }
            set
            {
                _selectedColor = value;
                AddOrUpdateValue("SelectedColor", value);
            }
        }

        private int? _peerToPeerMode;
        public int PeerToPeerMode
        {
            get
            {
                if (_peerToPeerMode == null)
                    _peerToPeerMode = GetValueOrDefault("PeerToPeerMode", 1);

                return _peerToPeerMode ?? 1;
            }
            set
            {
                _peerToPeerMode = value;
                AddOrUpdateValue("PeerToPeerMode", value);
            }
        }

        private libtgvoip.DataSavingMode? _useLessData;
        public libtgvoip.DataSavingMode UseLessData
        {
            get
            {
                if (_useLessData == null)
                    _useLessData = (libtgvoip.DataSavingMode)GetValueOrDefault("UseLessData", 0);

                return _useLessData ?? libtgvoip.DataSavingMode.Never;
            }
            set
            {
                _useLessData = value;
                AddOrUpdateValue("UseLessData", (int)value);
            }
        }

        public void SetChatPinnedMessage(long chatId, long messageId)
        {
            var container = _own.CreateContainer("PinnedMessages", ApplicationDataCreateDisposition.Always);
            AddOrUpdateValue(container, $"{chatId}", messageId);
        }

        public long GetChatPinnedMessage(long chatId)
        {
            var container = _own.CreateContainer("PinnedMessages", ApplicationDataCreateDisposition.Always);
            return GetValueOrDefault(container, $"{chatId}", 0L);
        }

        public void CleanUp()
        {
            // Here should be cleaned up all the settings that are shared with background tasks.
            _peerToPeerMode = null;
            _useLessData = null;
        }

        public new void Clear()
        {
            _container.Values.Clear();

            if (_own != null)
            {
                _own.Values.Clear();
            }

            if (_local != null)
            {
                _local.Values.Remove($"User{UserId}");
            }
        }
    }
}
