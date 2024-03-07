using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BusinessLayer;
using Database;
using Microsoft.VisualBasic;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum DisplayStatus
        {
            All,
            Old,
            Intersecting,
        }
        public MainWindow()
        {
            interaction = new DatabaseInteraction("database.json");
            InitializeComponent();
            InputDate.Value = DateTime.Now;
            UpdateEventsList();
            UpdateClosestEvents();
            Closed += (sender, args) => interaction.Dispose();
        }

        private void UpdateEventsList()
        {
            ShowAllButton.IsEnabled = true;
            ShowOldButton.IsEnabled = true;
            ShowIntersectingButton.IsEnabled = true;
            switch (status)
            {
                case DisplayStatus.All:
                    ShowAllButton.IsEnabled = false;
                    DisplayAllEvents();
                    break;
                case DisplayStatus.Old:
                    ShowOldButton.IsEnabled = false;
                    DisplayOldEvents();
                    break;
                case DisplayStatus.Intersecting:
                    ShowIntersectingButton.IsEnabled = false;
                    DisplayIntersectingEvents();
                    break;
            }
        }

        private void DisplayAllEvents(){
            PlannedList.Children.Clear();
            foreach (PlannedEvent plannedEvent in interaction.GetAllEvents())
            {
                PlannedList.Children.Add(GenerateEventPanel(plannedEvent));
            }
        }

        private void DisplayOldEvents()
        {
            PlannedList.Children.Clear();
            foreach (PlannedEvent plannedEvent in interaction.FindOldEvents(DateTime.Now))
            {
                PlannedList.Children.Add(GenerateEventPanel(plannedEvent));
            }
        }

        private void DisplayIntersectingEvents()
        {
            PlannedList.Children.Clear();
            foreach (List<PlannedEvent> intersecting in interaction.FindIntersectingEvents())
            {
                foreach (PlannedEvent plannedEvent in intersecting)
                {
                    PlannedList.Children.Add(GenerateEventPanel(plannedEvent));
                }
                PlannedList.Children.Add(new Separator());
            }
        }

        private StackPanel GenerateEventPanel(PlannedEvent plannedEvent)
        {
            StackPanel plannedEventPanel = new StackPanel
            {
                Margin = new Thickness(1, 1, 0, 0),
                Orientation = Orientation.Horizontal
            };

            TextBox name = new TextBox()
            {
                Text = plannedEvent.Name,
                Margin = new Thickness(0, 0, 5, 0),
                Width = 255,
            };
            name.TextChanged += (sender, args) =>
            {
                interaction.ChangeName(plannedEvent, name.Text);
                UpdateClosestEvents();
            };
            plannedEventPanel.Children.Add(name);

            TextBox place = new TextBox()
            {
                Text = plannedEvent.Place,
                Margin = new Thickness(0, 0, 5, 0),
                Width = 145,
            };
            place.TextChanged += (sender, args) =>
            {
                interaction.ChangePlace(plannedEvent, place.Text);
                UpdateClosestEvents();
            };
            plannedEventPanel.Children.Add(place);

            DateTimePicker date = new DateTimePicker()
            {
                Value = plannedEvent.Start,
                Width = 150,
                Format=DateTimeFormat.Custom,
                FormatString="dd.MM.yyyy HH:mm:ss",
            };
            date.ValueChanged += (sender, args) =>
            {
                interaction.ReSchedule(plannedEvent, date.Value.Value);
                UpdateClosestEvents();
            };
            plannedEventPanel.Children.Add(date);

            TextBox duration = new TextBox()
            {
                Text = plannedEvent.Duration.ToString(),
                Margin = new Thickness(5, 0, 5, 0),
                Width = 30,
            };
            duration.PreviewTextInput += NumbersOnly;
            duration.TextChanged += (sender, args) =>
            {
                interaction.ChangeDuration(plannedEvent, uint.Parse(duration.Text));
                UpdateClosestEvents();
            };
            plannedEventPanel.Children.Add(duration);

            Button remove = new Button()
            {
                Content = "Remove",
                Width = 48,
            };
            remove.Click += (sender, args) =>
            {
                interaction.RemoveEvent(plannedEvent);
                UpdateClosestEvents();
                UpdateEventsList();
            };
            plannedEventPanel.Children.Add(remove);

            return plannedEventPanel;
        }

        private void UpdateClosestEvents()
        {
            ClosestEvents.Children.Clear();
            foreach (PlannedEvent plannedEvent in interaction.FindClosestEvents(DateTime.Now, 2))
            {
                ClosestEvents.Children.Add(new Label()
                {
                    Content = plannedEvent.Name,
                });
                ClosestEvents.Children.Add(new Label()
                {
                    Content = plannedEvent.Place,
                });
                ClosestEvents.Children.Add(new Label()
                {
                    Content = plannedEvent.Start.ToString("dd.MM.yyyy HH:mm:ss"),
                });
                ClosestEvents.Children.Add(new Label()
                {
                    Content = plannedEvent.Duration + " хвилин",
                });
                ClosestEvents.Children.Add(new Separator());
            }
        }

        private readonly DatabaseInteraction interaction;
        private DisplayStatus status = DisplayStatus.All;

        private void OnAddNewEvent(object sender, RoutedEventArgs e)
        {
            interaction.CreateEvent(InputName.Text, InputPlace.Text, InputDate.Value.Value, uint.Parse(InputDuration.Text));
            UpdateEventsList();
            UpdateClosestEvents();
        }

        private static readonly Regex _regex = new Regex("^\\d*$");
        private void NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        private void OnShowAll(object sender, RoutedEventArgs e)
        {
            status = DisplayStatus.All;
            UpdateEventsList();
        }

        private void OnShowOld(object sender, RoutedEventArgs e)
        {
            status = DisplayStatus.Old;
            UpdateEventsList();
        }

        private void OnShowIntersect(object sender, RoutedEventArgs e)
        {
            status = DisplayStatus.Intersecting;
            UpdateEventsList();
        }
    }
}