using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using richtextbox.db;
using richtextbox.userctrls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace richtextbox {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowVM();
        }

        public class MainWindowVM : ViewModelBase {

            public MainWindowVM() {
                try {
                    Categories = new DbOps().FindAll();
                    /*TopCategory = new Category();
                    Categories.ForEach(i => TopCategory.SubCategories.Add(i));*/
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            #region Command definition

            public RelayCommand<TransforContentRoutedEventArgs> Event_SerializingFlowDocumentCommand {
                get { return new RelayCommand<TransforContentRoutedEventArgs>(Event_SerializingFlowDocument); }
            }

            public RelayCommand<TransforContentRoutedEventArgs> Event_DeserializingFlowDocumentCommand {
                get { return new RelayCommand<TransforContentRoutedEventArgs>(Event_DeserializingFlowDocument); }
            }

            public RelayCommand<RoutedPropertyChangedEventArgs<Object>> Event_TreeSelectionChangedCommand {
                get { return new RelayCommand<RoutedPropertyChangedEventArgs<Object>>(Event_TreeSelectionChanged); }
            }
            #endregion

            #region Event Handlers
            private void Event_TreeSelectionChanged(RoutedPropertyChangedEventArgs<Object> obj) {
                if(obj.NewValue==null || !(obj.NewValue is Category))
                    return;
                CurrentCategory = obj.NewValue as Category;
                SelectedSerializingVersion = CurrentCategory.Version;
            }

            private void Event_SerializingFlowDocument(TransforContentRoutedEventArgs arg) {
                bool re = Serialize(arg.DocumentOwner.Document, SelectedSerializingVersion, CurrentCategory.Id);
                if(re && !SelectedSerializingVersion.Equals(SelectedSerializingVersion)) {
                    CurrentCategory.Version = SelectedSerializingVersion;
                }
            }

            private void Event_DeserializingFlowDocument(TransforContentRoutedEventArgs arg) {
                Category cate = arg.Category;
                RichTextBox rtb = arg.DocumentOwner;
                byte[] data = new DbOps().LoadContent(cate.Id);
                try {
                    if(data != null) {
                        rtb.Document = Deserialize(cate.Version, data);
                    } else {
                        rtb.Document = new FlowDocument();
                    }
                } catch(Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            private bool Serialize(FlowDocument document, string version, int id) {
                byte[] data=null;
                if("RTF".Equals(version)) {
                    data =  SerializeRtf(document);
                } else if("XAML_PACKAGE".Equals(version)) {
                    data =  SerializeXamlPackage(document);
                } else if("XAML_WRITER_LOADER".Equals(version)) {
                    data=  SerializeXamlReaderWriter(document);
                }

                return new DbOps().Update(data, version, id);
            }

            private FlowDocument Deserialize(string version, byte[] data) {
                if("RTF".Equals(version)) {
                    return DeserializeRtf(data);
                } else if("XAML_PACKAGE".Equals(version)) {
                    return DeserializeXamlPackage(data);
                } else if("XAML_WRITER_LOADER".Equals(version)){
                    return DeserializeXamlReaderWriter(data);
                }
                return null;
            }


            private byte[] SerializeXamlReaderWriter(FlowDocument doc) {
                MemoryStream ms = new MemoryStream();
                XamlWriter.Save(doc, ms);
                byte[] data = ms.ToArray();
                return data;
            }

                        private byte[] SerializeXamlPackage(FlowDocument doc) {
                            return FlowDocumentEncode(doc, DataFormats.XamlPackage);
                        }

                        private byte[] SerializeRtf(FlowDocument doc) {
                            return FlowDocumentEncode(doc, DataFormats.Rtf); 
                        }

                        private static byte[] FlowDocumentEncode(FlowDocument flowDocument, string dataFormat) {
                            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                            byte[] content = null;
                            using(MemoryStream buffer = new MemoryStream()) {
                                textRange.Save(buffer, dataFormat);
                                content = buffer.ToArray();
                            }
                            return content;
                        }

            private FlowDocument DeserializeXamlReaderWriter(byte[] data) {
                MemoryStream ms = new MemoryStream(data);
                FlowDocument rtbLoad = (FlowDocument)XamlReader.Load(ms);
                return rtbLoad;
            }

            private FlowDocument DeserializeXamlPackage(byte[] data) {
                FlowDocument doc = new FlowDocument();
                LoadWithFormater(data, doc, new string[] { DataFormats .XamlPackage});
                return doc;
            }

            private FlowDocument DeserializeRtf(byte[] data) {
                FlowDocument doc = new FlowDocument();
                LoadWithFormater(data, doc, new string[] { DataFormats.Rtf });
                return doc;
            }

            private static void LoadWithFormater(byte[] data, FlowDocument workDoc, string[] formater) {
                TextRange range = new TextRange(workDoc.ContentStart, workDoc.ContentEnd);
                bool loaded = false;
                foreach(var fm in formater) {
                    using(MemoryStream ms = new MemoryStream(data)) {
                        try {
                            range.Load(ms, fm);
                            loaded = true;
                            break;
                        } catch(Exception ex) {
                        }
                        ms.Close();
                    }
                }
                if(!loaded) {
                    throw new Exception("Failed to load rich text content.");
                }
            }
            #endregion


            #region

            /* private Category topCategory;
             public Category TopCategory { get => topCategory; set { topCategory = value; RaisePropertyChanged(); } }*/

            private List<Category> categories= new List<Category>();
            public List<Category> Categories {
                get { return categories; }
                set { this.categories = value; RaisePropertyChanged(); }
            }

            private Category currentCategory;
            public Category CurrentCategory {
                get { return currentCategory; }
                set { this.currentCategory = value; RaisePropertyChanged(); }
            }

            public List<string> SerializingVersions {
                get { return serializingVersionDescription; }
                set { }
            }

            private string selectedSerializingVersion = serializingVersionDescription[2];
            public string SelectedSerializingVersion {
                get { return selectedSerializingVersion; }
                set { selectedSerializingVersion = value; RaisePropertyChanged(); }
            }

            #endregion


            public static List<string> serializingVersionDescription = new List<string>() {
                "XAML_WRITER_LOADER", 
                "RTF", 
                "XAML_PACKAGE" };
        }
    }
}
