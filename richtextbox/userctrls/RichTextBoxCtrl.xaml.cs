using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace richtextbox.userctrls {
    /// <summary>
    /// Interaction logic for RichTextBoxCtrl.xaml
    /// </summary>
    public partial class RichTextBoxCtrl : UserControl {
        public RichTextBoxCtrl() {
            InitializeComponent();
            ContentHolder.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, CommandBinding_Save));
        }

        #region Event Handlers
        private static void OnCurrentCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            RichTextBoxCtrl source = (RichTextBoxCtrl)d;
            var newCategory = e.NewValue as Category;
            try {
                source.RaiseOperatingEvent(newCategory, RichTextBoxCtrl.LoadingRichContentEvent);

            } catch(Exception ex) {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region Dependent propertiy definition
        public static DependencyProperty CurrentCategoryProperty = DependencyProperty.Register(
            "CurrentCategory", typeof(Category),
            typeof(RichTextBoxCtrl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentCategoryChanged)));

        public Category CurrentCategory {
            get { return (Category)GetValue(CurrentCategoryProperty); }
            set { SetValue(CurrentCategoryProperty, value); }
        }
        #endregion
        

        public delegate void ContentEventHandler(object sender, TransforContentRoutedEventArgs e);

        #region Routed Event Definition
        public static readonly RoutedEvent SavingRichContentEvent = EventManager.RegisterRoutedEvent(
           "SavingRichContent", RoutingStrategy.Bubble,
           typeof(ContentEventHandler), typeof(RichTextBoxCtrl));

        public event ContentEventHandler SavingRichContent {
            add {
                AddHandler(SavingRichContentEvent, value);
            }
            remove {
                RemoveHandler(SavingRichContentEvent, value);
            }
        }


        public static readonly RoutedEvent LoadingRichContentEvent = EventManager.RegisterRoutedEvent(
           "LoadingRichContent", RoutingStrategy.Bubble,
           typeof(ContentEventHandler), typeof(RichTextBoxCtrl));

        public event ContentEventHandler LoadingRichContent {
            add {
                AddHandler(LoadingRichContentEvent, value);
            }
            remove {
                RemoveHandler(LoadingRichContentEvent, value);
            }
        }

        #endregion

        #region methods
        private void RaiseOperatingEvent(Category newCategory, RoutedEvent e) {
            TransforContentRoutedEventArgs newEventArgs = new TransforContentRoutedEventArgs();
            newEventArgs.RoutedEvent = e;
            newEventArgs.Category = newCategory;
            newEventArgs.DocumentOwner = ContentHolder;
            RaiseEvent(newEventArgs);
        }

        private void CommandBinding_Save(object sender, ExecutedRoutedEventArgs e) {
            RaiseOperatingEvent(CurrentCategory, SavingRichContentEvent);
        }

        private void Event_ShowURI(object sender, RoutedEventArgs e) {
            //TODO
        }
        #endregion
    }

    public class TransforContentRoutedEventArgs : RoutedEventArgs {

        public TransforContentRoutedEventArgs()
            : base() {
        }
        public TransforContentRoutedEventArgs(RoutedEvent routedEvent)
            : base(routedEvent) {
        }
        public TransforContentRoutedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source) {
        }

        public RichTextBox DocumentOwner { get; set; }
        public Category Category { get; set; }
    }
}
