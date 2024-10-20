using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Theming;
using Material.Icons;

namespace OkapiLauncher.Controls
{
    /// <summary>
    /// Interaction logic for ViewSwitchButton.xaml
    /// </summary>
    [ObservableObject]
    public partial class ViewSwitchButton : UserControl
    {
        public ViewSwitchButton()
        {
            InitializeComponent();
            PART_Root.DataContext = this;
            ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
        }

        private void Current_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ProcessActive();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ViewSwitchButton), new PropertyMetadata(string.Empty));




        public MaterialIconKind? IconKind
        {
            get => (MaterialIconKind?)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        // Using a DependencyProperty as the backing store for IconKind.
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(MaterialIconKind?), typeof(ViewSwitchButton), new PropertyMetadata(defaultValue: null));



        public object ExpectedObject
        {
            get => GetValue(ExpectedObjectProperty);
            set => SetValue(ExpectedObjectProperty, value);
        }

        // Using a DependencyProperty as the backing store for ViewModelTypeName.
        public static readonly DependencyProperty ExpectedObjectProperty =
            DependencyProperty.Register(nameof(ExpectedObject), typeof(object), typeof(ViewSwitchButton), new PropertyMetadata(default(object), null));



        public object? CurrentObject
        {
            get => GetValue(CurrentObjectProperty);
            set => SetValue(CurrentObjectProperty, value);
        }

        // Using a DependencyProperty as the backing store for CurrentViewModelTypeName.
        public static readonly DependencyProperty CurrentObjectProperty =
            DependencyProperty.Register(nameof(CurrentObject), typeof(object), typeof(ViewSwitchButton), new PropertyMetadata(default(object), OnObjectChanged));



        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsActive.
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ViewSwitchButton), new PropertyMetadata(default(bool), OnIsActiveChanged));

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewSwitchButton button)
            {
                button.IsActive = button.ExpectedObject is not null && Equals(button.ExpectedObject, button.CurrentObject);
            }
        }



        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for Command.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ViewSwitchButton), new PropertyMetadata(default(ICommand), OnCommandChanged));



        public object? CommandParameter
        {
            get => (object?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for CommandParamater.
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ViewSwitchButton), new PropertyMetadata(default(object), OnCommandParameterChanged));



        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewSwitchButton button)
            {
                button.ProcessActive();
            }
        }
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewSwitchButton button)
            {
                if (e.OldValue is ICommand old)
                {
                    old.CanExecuteChanged -= button.ViewSwitchButton_CanExecuteChanged;
                }
                if (e.NewValue is ICommand @new)
                {
                    @new.CanExecuteChanged += button.ViewSwitchButton_CanExecuteChanged;
                }
                button.ProcessActive();
            }
        }
        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewSwitchButton button)
            {
                button.ProcessActive();
            }
        }

        private void ViewSwitchButton_CanExecuteChanged(object? sender, EventArgs e)
        {
            ProcessActive();
        }


        private Brush InactiveBackground
        {
            get
            {
                var start = Color.FromArgb(0, 0, 0, 0);
                var darking = Color.FromArgb(96, 0, 0, 0);
                return new LinearGradientBrush()
                {
                    StartPoint = new(0, 1),
                    EndPoint = new(0, 0),
                    GradientStops = { new(start, 0), new(darking, 0.75) },
                };
            }
        }

        private void ProcessActive()
        {
            bool canExecute = GetCanExecute();
            if (IsActive)
            {
                PART_This.OpacityMask = Brushes.White;
                PART_Header.FontWeight = FontWeights.Bold;
                PART_Header.Foreground = (Brush)this.FindResource("MahApps.Brushes.AccentBase");
                PART_Icon.Foreground = (Brush)this.FindResource("MahApps.Brushes.AccentBase");
                PART_This.IsEnabled = true;
            }
            else if (!canExecute)
            {
                PART_This.OpacityMask = new SolidColorBrush(Colors.Black * 0.15f);
                PART_Header.FontWeight = FontWeights.Regular;
                PART_Header.Foreground = (Brush)this.FindResource("MahApps.Brushes.ThemeForeground");
                PART_Icon.Foreground = (Brush)this.FindResource("MahApps.Brushes.ThemeForeground");
                PART_This.IsEnabled = false;
            }
            else
            {
                PART_This.OpacityMask = new SolidColorBrush(Colors.Black * 0.66f);
                //new LinearGradientBrush()
                //{
                //    StartPoint = new(0, 1),
                //    EndPoint = new(0, 0),
                //    GradientStops = { new(Colors.White, 0), new(Colors.Transparent, 1) },
                //};
                PART_Header.FontWeight = FontWeights.Regular;
                PART_Header.Foreground = (Brush)this.FindResource("MahApps.Brushes.ThemeForeground");
                PART_Icon.Foreground = (Brush)this.FindResource("MahApps.Brushes.ThemeForeground");
                PART_This.IsEnabled = true;

            }
        }

        private bool GetCanExecute()
        {
            return Command?.CanExecute(CommandParameter) ?? false;
        }

        private Brush ActiveBackground
        {
            get
            {
                return Brushes.Transparent;//(Brush)this.FindResource("PrimaryBrush");
            }
        }

        private void PART_This_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!GetCanExecute())
            {
                return;
            }
            if (IsActive)
            {
                return;
            }
            PART_This.OpacityMask = Brushes.White;

        }
        private void PART_This_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!GetCanExecute())
            { 
                return; 
            }
            if (IsActive)
            {
                return;
            }
            PART_This.OpacityMask = new SolidColorBrush(Colors.Black * 0.66f);
        }
        [ObservableProperty]
        private double _pathThickness = 0;

        private void PART_This_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Command is ICommand comm)
            {
                if (comm.CanExecute(CommandParameter))
                {
                    comm.Execute(CommandParameter);
                }
            }
            e.Handled = true;
        }
    }
}
