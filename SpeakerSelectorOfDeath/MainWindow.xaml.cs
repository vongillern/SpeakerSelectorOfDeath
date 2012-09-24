using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;

namespace SpeakerSelectorOfDeath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModel();

            var speaker1 = new Speaker();
            speaker1.Name = "speaker1";
            speaker1.Sessions.Add(new Session { Title = "session1A" });
            speaker1.Sessions.Add(new Session { Title = "session1B" });
            speaker1.Sessions.Add(new Session { Title = "session1C" });
            viewModel.Speakers.Add(speaker1);

            var speaker2 = new Speaker();
            speaker2.Name = "speaker2";
            speaker2.Sessions.Add(new Session { Title = "session2A" });
            speaker2.Sessions.Add(new Session { Title = "session2B" });
            speaker2.Sessions.Add(new Session { Title = "session2C" });
            viewModel.Speakers.Add(speaker2);

            viewModel.SelectedSessions.Add(speaker1.Sessions.First());
            viewModel.SelectedSessions.Add(speaker2.Sessions.First());

            var data = WriteObject<ViewModel>(viewModel);
            
        }

        public static string WriteObject<T>(T value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T));

                ser.WriteObject(stream, value);
                
                var data =  Encoding.UTF8.GetString(stream.GetBuffer());

                return data;
            }
            
        }
    }

    [Serializable]
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Speaker> _speakers = new ObservableCollection<Speaker>();
        public ObservableCollection<Speaker> Speakers
        {
            get { return _speakers; }
            set
            {
                if (_speakers != value)
                {
                    _speakers = value;
                    FirePropertyChanged("Speakers");
                }
            }
        }

        private ObservableCollection<Session> _selectedSessions = new ObservableCollection<Session>();
        public ObservableCollection<Session> SelectedSessions
        {
            get { return _selectedSessions; }
            set
            {
                if (_selectedSessions != value)
                {
                    _selectedSessions = value;
                    FirePropertyChanged("SelectedSessions");
                }
            }
        }

        private ObservableCollection<Session> _unselectedSessions = new ObservableCollection<Session>();
        public ObservableCollection<Session> UnselectedSessions
        {
            get { return _unselectedSessions; }
            set
            {
                if (_unselectedSessions != value)
                {
                    _unselectedSessions = value;
                    FirePropertyChanged("UnselectedSessions");
                }
            }
        }

        private ObservableCollection<Room> _rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _rooms; }
            set
            {
                if (_rooms != value)
                {
                    _rooms = value;
                    FirePropertyChanged("Rooms");
                }
            }
        }

        private ObservableCollection<TimeSlot> _timeSlots = new ObservableCollection<TimeSlot>();
        public ObservableCollection<TimeSlot> TimeSlots
        {
            get { return _timeSlots; }
            set
            {
                if (_timeSlots != value)
                {
                    _timeSlots = value;
                    FirePropertyChanged("TimeSlots");
                }
            }
        }
        
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }

    [Serializable]
    public class Speaker : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    FirePropertyChanged("Name");
                }
                
            }
        }

        private string _homeTown;
        public string HomeTown
        {
            get { return _homeTown; }
            set
            {
                if (_homeTown != value)
                {
                    _homeTown = value;
                    FirePropertyChanged("HomeTown");
                }
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    FirePropertyChanged("Email");
                }
            }
        }

        private string _website;
        public string Website
        {
            get { return _website; }
            set
            {
                if (_website != value)
                {
                    _website = value;
                    FirePropertyChanged("Website");
                }
            }
        }

        private string _headshotUrl;
        public string HeadshotUrl
        {
            get { return _headshotUrl; }
            set
            {
                if (_headshotUrl != value)
                {
                    _headshotUrl = value;
                    FirePropertyChanged("HeadshotUrl");
                }
            }
        }

        private string _bio;
        public string Bio
        {
            get { return _bio; }
            set
            {
                if (_bio != value)
                {
                    _bio = value;
                    FirePropertyChanged("Bio");
                }
            }
        }

        private string _notesToOrganizer;
        public string NotesToOrganizer
        {
            get { return _notesToOrganizer; }
            set
            {
                if (_notesToOrganizer != value)
                {
                    _notesToOrganizer = value;
                    FirePropertyChanged("NotesToOrganizer");
                }
            }
        }

        private ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
        public ObservableCollection<Session> Sessions
        {
            get { return _sessions; }
            set
            {
                if (_sessions != value)
                {
                    _sessions = value;
                    FirePropertyChanged("Sessions");
                }
            }
        }
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }

    [Serializable]
    public class Session : INotifyPropertyChanged
    {
        private string _level;
        public string Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    FirePropertyChanged("Level");
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    FirePropertyChanged("Title");
                }
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    FirePropertyChanged("Description");
                }
            }
        }

        private Selection _selection;
        public Selection Selection
        {
            get { return _selection; }
            set
            {
                if (_selection != value)
                {
                    _selection = value;
                    FirePropertyChanged("Selection");
                }
            }
        }
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }

    [Serializable]
    public class Room : INotifyPropertyChanged
    {
        private string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set
            {
                if (_roomName != value)
                {
                    _roomName = value;
                    FirePropertyChanged("RoomName");
                }
            }
        }

        private string _trackName;
        public string TrackName
        {
            get { return _trackName; }
            set
            {
                if (_trackName != value)
                {
                    _trackName = value;
                    FirePropertyChanged("TrackName");
                }
            }
        }
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }

    [Serializable]
    public class TimeSlot : INotifyPropertyChanged
    {
        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    FirePropertyChanged("StartTime");
                }
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    FirePropertyChanged("EndDate");
                }
            }
        }
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
        
    }

    [Serializable]
    public class Selection : INotifyPropertyChanged
    {

        private Room _room;
        public Room Room
        {
            get { return _room; }
            set
            {
                if (_room != value)
                {
                    _room = value;
                    FirePropertyChanged("Room");
                }
            }
        }

        private TimeSlot _timeSlot;
        public TimeSlot TimeSlot
        {
            get { return _timeSlot; }
            set
            {
                if (_timeSlot != value)
                {
                    _timeSlot = value;
                    FirePropertyChanged("TimeSlot");
                }
            }
        }

        private Session _session;
        public Session Session
        {
            get { return _session; }
            set
            {
                if (_session != value)
                {
                    _session = value;
                    FirePropertyChanged("Session");
                }
            }
        }
        

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }
}
