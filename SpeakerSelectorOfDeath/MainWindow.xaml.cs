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
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SpeakerSelectorOfDeath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            //var data = WriteObject<ViewModel>(viewModel);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            _viewModel = new ViewModel();

            //ISpeakerProvider speakerProvider = new IccSpeakerProvider(@"C:\Users\Jon\Downloads\icc2015.csv");

            //var speakers = speakerProvider.GetSpeakerSessions();

            //_viewModel.Speakers.AddRange(speakers);
            //_viewModel.UnselectedSessions.AddRange(speakers.SelectMany(s => s.Sessions));

            //InitializeRoomsAndTimes();

            this.DataContext = _viewModel;
        }


        private void InitializeRoomsAndTimes()
        {
            //List<string> roomNames = new List<string> { "Auditorium", "116E", "118E", "119E", "121E", "125E", "123E", "126E" };
            List<string> roomNames = new List<string> 
            { 
                "Room A (50)",
                "Room B (50)",
                "Room C (30)",
                "Room D (30)",
                "Room E (30)",
                "Room F (30)",
                "Room G (30)",
                "Room H (30)",
            };

            _viewModel.TimeSlots = new ObservableCollection<TimeSlot> 
            { 
                TimeSlot.Create(9, 0, 75),
                TimeSlot.Create(10, 30, 75),
                TimeSlot.Create(12, 45, 75),
                TimeSlot.Create(14, 15, 75),
                TimeSlot.Create(15, 45, 75),
            };

            foreach (var roomName in roomNames)
            {
                var room = new Room { RoomName = roomName };

                foreach (var timeSlot in _viewModel.TimeSlots)
                {
                    room.Selections.Add(new Selection { Room = room, TimeSlot = timeSlot });
                }

                _viewModel.Rooms.Add(room);

            }

            

        }


        #region Session Drag and Drop

        //
        //http://blogs.gotdotnet.com/jaimer/archive/2007/07/12/drag-drop-in-wpf-explained-end-to-end.aspx
        //

        Point _startPoint = new Point();
        bool IsDragging = false;

        //we will only allow sessions to be dragged onto selections

        private void Session_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag(sender, e);
                }
            }

        }

        private void Session_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void StartDrag(object sender, MouseEventArgs e)
        {
            IsDragging = true;

            Panel panel = (Panel)sender;
            object dataContext = panel.DataContext;

            if (dataContext != null)
            {
                DataObject data = new DataObject(dataContext.GetType(), dataContext);
                DragDropEffects de = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
            }
            IsDragging = false;
        }

        private void StartDragCustomCursor(object sender, MouseEventArgs e)
        {
            Panel panel = (Panel)sender;
            object dataContext = panel.DataContext;

            GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);

            ((FrameworkElement)sender).GiveFeedback += handler;
            IsDragging = true;
            DataObject data = new DataObject(dataContext.GetType(), dataContext);

            DragDropEffects de = DragDrop.DoDragDrop(((FrameworkElement)sender), data, DragDropEffects.Move);
            ((FrameworkElement)sender).GiveFeedback -= handler;
            IsDragging = false;
        }

        void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                Mouse.SetCursor(Cursors.Hand);

                e.UseDefaultCursors = false;
                e.Handled = true;
            }
            finally { }
        }

        private void Selection_Drop(object sender, DragEventArgs e)
        {
            
            //if we are dropping a session on a selection

            Panel panel = sender as Panel;
            if (panel != null && panel.DataContext is Selection)
            {
                var targetDropSelection = (Selection)panel.DataContext;

                if (e.Data.GetDataPresent(typeof(Session)))
                {
                    Session droppingSession = (Session)e.Data.GetData(typeof(Session));

                    if (droppingSession != targetDropSelection.Session)
                    {
                        if (droppingSession.Selection != null && targetDropSelection.Session != null)
                        {
                            //if we're dragging a session that is already scheduled, to a different spot, just swap them instead of 
                            //adding one to the unselected sessions list

                            var originalSourceSelection = droppingSession.Selection;
                            var sessionDroppedOnTo = targetDropSelection.Session;

                            droppingSession.Selection = targetDropSelection;
                            originalSourceSelection.Session = sessionDroppedOnTo;
                            
                            return;
                        }

                        //if there was an old session, add it to unselected list
                        if (targetDropSelection.Session != null)
                        {
                            targetDropSelection.Session.Selection = null;
                            _viewModel.UnselectedSessions.Add(targetDropSelection.Session);
                        }

                        if (droppingSession.Selection != null)
                            droppingSession.Selection.Session = null;

                        //if unselected had the dropped session, remove it
                        _viewModel.UnselectedSessions.Remove(droppingSession);

                        targetDropSelection.Session = droppingSession;
                    }

                }

            }
            else if (sender == UnselectedSessionsBox)
            {
                if (e.Data.GetDataPresent(typeof(Session)))
                {
                    Session droppingSession = (Session)e.Data.GetData(typeof(Session));
                    //don't do anything with a drop from unselected to unselected
                    if (!_viewModel.UnselectedSessions.Contains(droppingSession))
                    {
                        droppingSession.Selection.Session = null;
                        droppingSession.Selection = null;

                        _viewModel.UnselectedSessions.Add(droppingSession);
                    }
                }
            }
            //panel.Background = Brushes.Transparent;
        }

        private void Selection_DragEnter(object sender, DragEventArgs e)
        {
            //Panel potentialDropTarget = (Panel)sender;
            //if (potentialDropTarget.DataContext is Session)
            //{
            //    if (e.Data.GetDataPresent(typeof(Session)))
            //    {
            //        Session droppingSession = (Session)e.Data.GetData(typeof(Session));
            //        if (droppingSession.ContainsInTree(potentialDropTarget.DataContext as RuleCategory))
            //            potentialDropTarget.Background = Brushes.Pink;
            //        else
            //            potentialDropTarget.Background = Brushes.LightGreen;
            //    }
            //    else
            //        potentialDropTarget.Background = Brushes.LightGreen;
            //}
            //else if (potentialDropTarget.DataContext is Rule)
            //    potentialDropTarget.Background = Brushes.Pink;
        }

        private void Selection_DragLeave(object sender, DragEventArgs e)
        {
            //Panel panel = (Panel)sender;
            //panel.Background = Brushes.Transparent;
        }

        #endregion


        //this sucks
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Speaker Selections";
            sfd.DefaultExt = "ssod";
            sfd.Filter = "Speaker Selections OF DEATH (*.ssod)|*.ssod";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = File.Open(sfd.FileName, FileMode.OpenOrCreate))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(stream, _viewModel);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Speaker Selections";
            ofd.DefaultExt = "ssod";
            ofd.Filter = "Speaker Selections OF DEATH (*.ssod)|*.ssod";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == true)
            {
                using (Stream stream = File.Open(ofd.FileName, FileMode.OpenOrCreate))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    ViewModel viewModel = serializer.Deserialize(stream) as ViewModel;
                    _viewModel = viewModel;
                    this.DataContext = _viewModel;
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Greg's stuff";
            sfd.DefaultExt = "csv";
            sfd.Filter = "Gregs selections (*.csv)|*.csv";
            sfd.RestoreDirectory = true;
            HashSet<string> emails = new HashSet<string>();
            if (sfd.ShowDialog() == true)
            {
                var fileBuilder = new StringBuilder();
                int speakerKey = 1;
                int sessionKey = 1;
                foreach (Speaker speaker in _viewModel.Speakers)
                {
                    foreach (Session session in speaker.Sessions)
                    {
                        var lineBuilder = new StringBuilder();

                        lineBuilder.Append(speakerKey); lineBuilder.Append(",");
                        lineBuilder.Append(sessionKey); lineBuilder.Append(",");
                        lineBuilder.Append(session.Selection != null); lineBuilder.Append(",");
                        lineBuilder.Append(session.Selection != null ? session.Selection.Room.RoomName : ""); lineBuilder.Append(",");
                        lineBuilder.Append(session.Selection != null ? session.Selection.TimeSlot.StartDate.ToString("h:mm") : ""); lineBuilder.Append(",");
                        lineBuilder.Append(speaker.Name); lineBuilder.Append(",");
                        lineBuilder.Append(session.Level); lineBuilder.Append(",");
                        lineBuilder.Append(session.Title); lineBuilder.Append(",");

                        fileBuilder.AppendLine(lineBuilder.ToString());                        

                        sessionKey++;
                    }
                    speakerKey++;

                    emails.Add(speaker.Email);
                }

                using (StreamWriter writer = new StreamWriter(sfd.FileName))
                {
                    writer.Write(fileBuilder.ToString());
                }
            }
        }

        private void EmailCsv_Click(object sender, RoutedEventArgs e)
        {
            var haveSessions = new List<string>();
            var noSessions = new List<string>();

            foreach (Speaker speaker in _viewModel.Speakers)
            {
                if (speaker.Sessions.Any(s => s.Selection != null))
                    haveSessions.Add(speaker.Email);
                else
                    noSessions.Add(speaker.Email);
                
            }
        }
    }

    [Serializable]
    public class ViewModel : INotifyPropertyChanged
    {


        private string _search = "";
        public string Search
        {
            get { return _search; }
            set
            {
                if (_search == value)
                    return;
                _search = value;
                Highlight();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Search"));
            }
        }

        private void Highlight()
        {
            

            var searchRegex = new Regex(_search, RegexOptions.IgnoreCase);
            if (_search == "")
                searchRegex = new Regex(@"asdofiwoinfoinas;donifosianwoinwef", RegexOptions.IgnoreCase);

            foreach (var speaker in Speakers)
            {
                var speakerHighlight = searchRegex.IsMatch(speaker.Name);

                foreach (var session in speaker.Sessions)
                {
                    session.Highlight = searchRegex.IsMatch(session.Title) || speakerHighlight;
                }
            }
        }

        
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

        private ObservableCollection<TimeSlot> _timeSlots;
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

        public void AddSession(Session session)
        {
            _sessions.Add(session);
            session.Speaker = this;
        }

        private ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
        public ObservableCollection<Session> Sessions
        {
            get { return _sessions; }
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

        private int _rating;
        public int Rating
        {
            get { return _rating; }
            set
            {
                if (_rating != value)
                {
                    _rating = value;
                    FirePropertyChanged("Rating");
                }
            }
        }
        

        private Speaker _speaker;
        public Speaker Speaker
        {
            get { return _speaker; }
            set
            {
                if (_speaker != value)
                {
                    _speaker = value;
                    FirePropertyChanged("Speaker");
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
                    if (_selection != null)
                        _selection.Session = this;
                    FirePropertyChanged("Selection");
                }
            }
        }


        private bool _highlight;
        public bool Highlight
        {
            get { return _highlight; }
            set
            {
                if (_highlight == value)
                    return;
                _highlight = value;
                FirePropertyChanged("Highlight");
            }
        }
        

        public override string ToString()
        {
            return "Session: " + Title;
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

        private ObservableCollection<Selection> _selections = new ObservableCollection<Selection>();
        public ObservableCollection<Selection> Selections
        {
            get { return _selections; }
            set
            {
                if (_selections != value)
                {
                    _selections = value;
                    FirePropertyChanged("Selections");
                }
            }
        }

        public override string ToString()
        {
            return "Room: " + RoomName;
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
        public static TimeSlot Create(int hour, int minute, int minuteLength)
        {
            //we might mess with this later if we ever had a multi-day conference
            //but right now it doesn't really matter
            var baseDate = new DateTime(2012, 10, 27);

            return new TimeSlot
            {
                StartDate = baseDate.AddHours(hour).AddMinutes(minute),
                EndDate = baseDate.AddHours(hour).AddMinutes(minute + minuteLength),
            };
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    FirePropertyChanged("StartDate");
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

        public override string ToString()
        {
            return "TimeSlot: " + StartDate.ToString("HH:MM");
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
                    if (_session != null)
                        _session.Selection = this;
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

    public static class Extensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (T t in items)
            {
                collection.Add(t);
            }
        }
        
    }

    public class SessionTemplateSelector : DataTemplateSelector
    {

        /// <summary>
        /// initializes SessionTemplateSelector
        /// </summary>
        public SessionTemplateSelector()
        {
        }
        
        public DataTemplate NonNullTemplate { get; set; }
        

        

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            return NonNullTemplate;

            //return base.SelectTemplate(item, container);
        }
        
            
        
    }

    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                {
                    return new SolidColorBrush(Colors.Pink);
                }
            }
            return new SolidColorBrush(Colors.LightGreen);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


}
